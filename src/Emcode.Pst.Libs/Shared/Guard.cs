namespace Emcode.Pst.Shared;

/// <summary>
/// Helper validasi argumen untuk menjaga kontrak input.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Memastikan string tidak null atau whitespace.
    /// </summary>
    /// <param name="value">Nilai yang divalidasi.</param>
    /// <param name="paramName">Nama parameter.</param>
    /// <returns>Nilai yang valid.</returns>
    public static string NotNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Memastikan object tidak null.
    /// </summary>
    /// <typeparam name="T">Tipe referensi.</typeparam>
    /// <param name="value">Nilai yang divalidasi.</param>
    /// <param name="paramName">Nama parameter.</param>
    /// <returns>Nilai yang valid.</returns>
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }
}
