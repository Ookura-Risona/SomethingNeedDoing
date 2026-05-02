using System.Text.RegularExpressions;

namespace DalamudCodeEditor;

public class LanguageDefinition
{
    public List<string> Keywords { get; } = new();

    public Dictionary<string, Identifier> Identifiers { get; } = new();

    public string CommentStart { get; init; } = "";

    public string CommentEnd { get; init; } = "";

    public string SingleLineComment { get; init; } = "";

    public Func<string, IEnumerable<Token>>? TokenizeLine { get; init; } = null;

    public List<(Regex regex, PaletteIndex color)> RegexTokens { get; } = new();
}
