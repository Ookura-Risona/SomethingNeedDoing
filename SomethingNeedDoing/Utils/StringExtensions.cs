namespace SomethingNeedDoing.Utils;

public static class StringExtensions
{
    /// <summary>
    /// Normalizes a file path to handle different directory separators and formats.
    /// </summary>
    public static string NormalizeFilePath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var normalized = System.Text.RegularExpressions.Regex.Replace(path, @"[\\/]+", System.IO.Path.DirectorySeparatorChar.ToString()); // replace consecutive slashes with singles

        if (normalized.Length >= 2 && char.IsLetter(normalized[0]) && normalized[1] == ':') // capitalise drive letters
            normalized = char.ToUpperInvariant(normalized[0]) + normalized[1..];

        if (normalized.StartsWith(@"\\")) // unc
            return normalized;

        if (!System.IO.Path.IsPathRooted(normalized))
            normalized = System.IO.Path.GetFullPath(normalized);

        return normalized;
    }
}
