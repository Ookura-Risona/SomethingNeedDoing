using System.Text.RegularExpressions;

namespace SomethingNeedDoing.Utils;

public static class RegexExtensions
{
    public static string ExtractAndUnquote(this Match match, string groupName)
    {
        var group = match.Groups[groupName];
        var groupValue = group.Value;

        if (groupValue.StartsWith('"') && groupValue.EndsWith('"'))
            groupValue = groupValue.Trim('"');

        return groupValue;
    }
}
