using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emcode.Pst.Domain;
using Emcode.Pst.Shared;

namespace Emcode.Pst.Infrastructure;

/// <summary>
/// Parser sederhana untuk file .eml yang menghasilkan draft pesan.
/// </summary>
internal sealed class PstEmlParser
{
    /// <summary>
    /// Membaca dan mem-parse file .eml menjadi draft pesan.
    /// </summary>
    /// <param name="emlPath">Path file .eml.</param>
    /// <returns>Draft pesan hasil parsing.</returns>
    public PstMessageDraft Parse(string emlPath)
    {
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        var raw = File.ReadAllText(emlPath, Encoding.UTF8);
        return ParseContent(raw);
    }

    /// <summary>
    /// Membaca dan mem-parse file .eml menjadi draft pesan secara asynchronous.
    /// </summary>
    /// <param name="emlPath">Path file .eml.</param>
    /// <param name="cancellationToken">Token pembatalan operasi.</param>
    /// <returns>Draft pesan hasil parsing.</returns>
    public async Task<PstMessageDraft> ParseAsync(string emlPath, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(emlPath, nameof(emlPath));
        await using var stream = File.OpenRead(emlPath);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var raw = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ParseContent(raw);
    }

    private static PstMessageDraft ParseContent(string raw)
    {
        raw = NormalizeLineEndings(raw);
        SplitHeadersAndBody(raw, out var headerText, out var bodyText);
        var headers = ParseHeaders(headerText);

        var subject = GetHeader(headers, "Subject");
        var fromHeader = GetHeader(headers, "From");
        var toHeader = GetHeader(headers, "To");
        var ccHeader = GetHeader(headers, "Cc");
        var bccHeader = GetHeader(headers, "Bcc");
        var messageId = GetHeader(headers, "Message-Id");
        var dateHeader = GetHeader(headers, "Date");
        var contentType = GetHeader(headers, "Content-Type");
        var threadTopic = GetHeader(headers, "Thread-Topic");
        var references = GetHeader(headers, "References");

        var (fromName, fromAddress) = ParseSingleAddress(fromHeader);
        var recipients = new List<PstDraftRecipient>();
        recipients.AddRange(ParseAddressList(toHeader, PstRecipientType.To));
        recipients.AddRange(ParseAddressList(ccHeader, PstRecipientType.Cc));
        recipients.AddRange(ParseAddressList(bccHeader, PstRecipientType.Bcc));

        string? textBody = null;
        string? htmlBody = null;
        var attachments = new List<PstDraftAttachment>();

        if (!string.IsNullOrWhiteSpace(contentType) && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase))
        {
            var boundary = GetParameterValue(contentType, "boundary");
            if (!string.IsNullOrWhiteSpace(boundary))
            {
                var parts = SplitMultipart(bodyText, boundary!);
                foreach (var part in parts)
                {
                    SplitHeadersAndBody(part, out var partHeaderText, out var partBodyText);
                    var partHeaders = ParseHeaders(partHeaderText);
                    var partContentType = GetHeader(partHeaders, "Content-Type");
                    var disposition = GetHeader(partHeaders, "Content-Disposition");
                    var transferEncoding = GetHeader(partHeaders, "Content-Transfer-Encoding");
                    var contentId = GetHeader(partHeaders, "Content-Id");

                    var decodedBytes = DecodeBody(partBodyText, transferEncoding, partContentType);
                    if (IsAttachmentPart(disposition, partContentType))
                    {
                        var fileName = GetParameterValue(disposition, "filename")
                            ?? GetParameterValue(partContentType, "name");
                        attachments.Add(new PstDraftAttachment
                        {
                            FileName = fileName,
                            LongFileName = fileName,
                            ContentType = partContentType,
                            ContentId = TrimAngleBrackets(contentId),
                            IsInline = disposition?.Contains("inline", StringComparison.OrdinalIgnoreCase) == true,
                            ContentBytes = decodedBytes
                        });
                        continue;
                    }

                    if (partContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        htmlBody ??= DecodeText(partBodyText, partContentType, transferEncoding);
                        continue;
                    }

                    if (partContentType?.Contains("text/plain", StringComparison.OrdinalIgnoreCase) == true || textBody is null)
                    {
                        textBody ??= DecodeText(partBodyText, partContentType, transferEncoding);
                    }
                }
            }
        }

        if (textBody is null && htmlBody is null)
        {
            var isHtml = contentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true;
            if (isHtml)
            {
                htmlBody = DecodeText(bodyText, contentType, GetHeader(headers, "Content-Transfer-Encoding"));
            }
            else
            {
                textBody = DecodeText(bodyText, contentType, GetHeader(headers, "Content-Transfer-Encoding"));
            }
        }

        var sentTime = ParseDate(dateHeader);

        return new PstMessageDraft
        {
            MessageClass = "IPM.Note",
            Subject = subject,
            FromName = fromName,
            FromAddress = fromAddress,
            Body = textBody,
            HtmlBody = htmlBody,
            MessageId = messageId,
            SentTime = sentTime,
            ClientSubmitTime = sentTime,
            LastModificationTime = sentTime,
            IsDraft = true,
            TransportMessageHeaders = headerText,
            ConversationTopic = threadTopic ?? references ?? subject,
            Recipients = recipients,
            Attachments = attachments
        };
    }

    private static string NormalizeLineEndings(string input)
    {
        return input.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static void SplitHeadersAndBody(string raw, out string headers, out string body)
    {
        var separator = "\n\n";
        var index = raw.IndexOf(separator, StringComparison.Ordinal);
        if (index < 0)
        {
            headers = raw;
            body = string.Empty;
            return;
        }

        headers = raw[..index];
        body = raw[(index + separator.Length)..];
    }

    private static Dictionary<string, string> ParseHeaders(string headerText)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = headerText.Split('\n');
        string? currentKey = null;
        var currentValue = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if ((line.StartsWith(" ") || line.StartsWith("\t")) && currentKey is not null)
            {
                currentValue.Append(' ').Append(line.Trim());
                continue;
            }

            if (currentKey is not null)
            {
                headers[currentKey] = currentValue.ToString();
            }

            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
            {
                currentKey = null;
                currentValue.Clear();
                continue;
            }

            currentKey = line[..separatorIndex].Trim();
            currentValue.Clear();
            currentValue.Append(line[(separatorIndex + 1)..].Trim());
        }

        if (currentKey is not null)
        {
            headers[currentKey] = currentValue.ToString();
        }

        return headers;
    }

    private static string? GetHeader(Dictionary<string, string> headers, string key)
    {
        return headers.TryGetValue(key, out var value) ? value : null;
    }

    private static string? GetParameterValue(string? headerValue, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return null;
        }

        var parts = headerValue.Split(';');
        foreach (var part in parts)
        {
            var section = part.Trim();
            if (!section.StartsWith(parameterName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var index = section.IndexOf('=');
            if (index < 0)
            {
                continue;
            }

            var value = section[(index + 1)..].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    private static List<string> SplitMultipart(string body, string boundary)
    {
        var parts = new List<string>();
        var marker = $"--{boundary}";
        var endMarker = $"--{boundary}--";
        var lines = body.Split('\n');
        var builder = new StringBuilder();
        var inPart = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (line.StartsWith(endMarker, StringComparison.Ordinal))
            {
                if (inPart)
                {
                    parts.Add(builder.ToString());
                }
                break;
            }

            if (line.StartsWith(marker, StringComparison.Ordinal))
            {
                if (inPart)
                {
                    parts.Add(builder.ToString());
                    builder.Clear();
                }

                inPart = true;
                continue;
            }

            if (inPart)
            {
                builder.AppendLine(rawLine);
            }
        }

        if (inPart && builder.Length > 0)
        {
            parts.Add(builder.ToString());
        }

        return parts;
    }

    private static bool IsAttachmentPart(string? disposition, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(disposition) &&
            disposition.Contains("attachment", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(disposition) &&
            disposition.Contains("inline", StringComparison.OrdinalIgnoreCase) &&
            (disposition.Contains("filename", StringComparison.OrdinalIgnoreCase) ||
             contentType?.Contains("name=", StringComparison.OrdinalIgnoreCase) == true))
        {
            return true;
        }

        return false;
    }

    private static byte[]? DecodeBody(string body, string? transferEncoding, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Array.Empty<byte>();
        }

        if (transferEncoding?.Equals("base64", StringComparison.OrdinalIgnoreCase) == true)
        {
            var normalized = RemoveWhitespace(body);
            try
            {
                return Convert.FromBase64String(normalized);
            }
            catch (FormatException)
            {
                return Encoding.UTF8.GetBytes(body);
            }
        }

        var text = DecodeText(body, contentType, transferEncoding);
        return Encoding.UTF8.GetBytes(text);
    }

    private static string DecodeText(string body, string? contentType, string? transferEncoding)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        if (transferEncoding?.Equals("base64", StringComparison.OrdinalIgnoreCase) == true)
        {
            var normalized = RemoveWhitespace(body);
            try
            {
                var bytes = Convert.FromBase64String(normalized);
                return DecodeBytes(bytes, contentType);
            }
            catch (FormatException)
            {
                return body.TrimEnd();
            }
        }

        if (transferEncoding?.Equals("quoted-printable", StringComparison.OrdinalIgnoreCase) == true)
        {
            return DecodeQuotedPrintable(body, contentType);
        }

        return body.TrimEnd();
    }

    private static string DecodeQuotedPrintable(string body, string? contentType)
    {
        var bytes = new List<byte>();
        for (var i = 0; i < body.Length; i++)
        {
            var ch = body[i];
            if (ch == '=' && i + 2 < body.Length)
            {
                if (body[i + 1] == '\r' || body[i + 1] == '\n')
                {
                    while (i + 1 < body.Length && (body[i + 1] == '\r' || body[i + 1] == '\n'))
                    {
                        i++;
                    }
                    continue;
                }

                var hex = body.Substring(i + 1, 2);
                if (byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                {
                    bytes.Add(value);
                    i += 2;
                    continue;
                }
            }

            bytes.Add((byte)ch);
        }

        return DecodeBytes(bytes.ToArray(), contentType);
    }

    private static string DecodeBytes(byte[] bytes, string? contentType)
    {
        var charset = GetParameterValue(contentType, "charset");
        if (string.IsNullOrWhiteSpace(charset))
        {
            return Encoding.UTF8.GetString(bytes);
        }

        try
        {
            return Encoding.GetEncoding(charset!).GetString(bytes);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }

    private static string RemoveWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (!char.IsWhiteSpace(ch))
            {
                builder.Append(ch);
            }
        }
        return builder.ToString();
    }

    private static IEnumerable<PstDraftRecipient> ParseAddressList(string? headerValue, PstRecipientType type)
    {
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            yield break;
        }

        var parts = headerValue.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var (name, address) = ParseSingleAddress(trimmed);
            yield return new PstDraftRecipient
            {
                RecipientType = type,
                DisplayName = name,
                EmailAddress = address,
                SmtpAddress = address
            };
        }
    }

    private static (string? name, string? address) ParseSingleAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, null);
        }

        var trimmed = value.Trim();
        var start = trimmed.IndexOf('<');
        var end = trimmed.IndexOf('>');
        if (start >= 0 && end > start)
        {
            var name = trimmed[..start].Trim().Trim('"');
            var address = trimmed[(start + 1)..end].Trim();
            return (string.IsNullOrWhiteSpace(name) ? null : name, address);
        }

        return (null, trimmed.Trim('"'));
    }

    private static string? TrimAngleBrackets(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("<", StringComparison.Ordinal) && trimmed.EndsWith(">", StringComparison.Ordinal))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var result))
        {
            return result;
        }

        return null;
    }
}
