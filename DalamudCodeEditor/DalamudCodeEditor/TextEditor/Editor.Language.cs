namespace DalamudCodeEditor.TextEditor;

public partial class Editor
{
    private LanguageDefinition languageDefinition = new();

    public LanguageDefinition Language
    {
        get => languageDefinition;

        set
        {
            languageDefinition = value;
            // mRegexList.Clear();

            // foreach (var r in languageDefinition.RegexTokens)
            // {
            // mRegexList.Add(new Tuple<Regex, PaletteIndex>(r.regex, r.color));
            // }

            Colorizer.Colorize();
        }
    }
}
