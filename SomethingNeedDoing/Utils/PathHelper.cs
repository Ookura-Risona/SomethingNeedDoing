using System.IO;

namespace SomethingNeedDoing.Utils;

public static class PathHelper
{
    public static bool ValidatePath(string path)
    {
        // Check whitespace and invalid characters
        if (string.IsNullOrWhiteSpace(path) && !path.Any(c => Path.GetInvalidPathChars().Contains(c)))
            return false;

        // Ensure path is absolute and directory exists
        if (!Path.IsPathRooted(path) || !Directory.Exists(path))
            return false;

        return true;
    }

    public static string NormalizePath(string input)
    {
        if (!ValidatePath(input))
            return input;

        var path = input.Replace('/', '\\');
        return Path.GetFullPath(path).Replace('\\', '/');
    }
}
