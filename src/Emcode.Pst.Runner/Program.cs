using Emcode.Pst.Application;


// if (args.Length == 0)
// {
//     Console.WriteLine("Usage: Emcode.Pst.Runner <path-to-pst>");
//     return;
// }

// var path = args[0];
var options = new PstOpenOptions
{
    ReadOnly = true,
    ValidateChecksums = true
};

var path = "C:\\Users\\Aan\\source\\repos\\PST Projects\\doc\\Samples\\sample1.pst";

using var pst = PstFile.Open(path, options);
Console.WriteLine($"Opened PST: {pst.Path}");
if (pst.Header is not null)
{
    Console.WriteLine($"Header Signature: 0x{pst.Header.Signature:X8}");
    Console.WriteLine($"Header Client Signature: 0x{pst.Header.ClientSignature:X8}");
    Console.WriteLine($"Header Version: {pst.Header.Version}.{pst.Header.VersionMinor}");
    Console.WriteLine($"Header Format: {pst.Header.Format}");
    Console.WriteLine($"Header Crypt Method: {pst.Header.CryptMethod}");
    Console.WriteLine($"Header File Size: {pst.Header.FileSize} bytes");
}
Console.WriteLine("Folder count: " + pst.Folders.Count);
foreach (var folder in pst.Folders)
{
    Console.WriteLine($"Folder: {folder.Name} ({folder.Messages.Count} messages)");
    foreach (var message in folder.EnumerateMessages())
    {
        Console.WriteLine($"  Message: {message.Subject ?? "(no subject)."}. Size: {message.Size} bytes.");
        File.WriteAllText("D:\\temp\\output.html", message.HtmlBody);
        goto exit;
    }
}
exit:
Console.WriteLine("Done.");