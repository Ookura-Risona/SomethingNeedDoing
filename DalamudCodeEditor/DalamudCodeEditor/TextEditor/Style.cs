using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Style(Editor editor) : EditorComponent(editor)
{
    public int TabSize { get; private set; } = 4;

    public readonly char TabCharacter = ' ';

    public List<char> Tab
    {
        get
        {
            var characters = new List<char>();
            for (var i = 0; i < TabSize; i++)
            {
                characters.Add(TabCharacter);
            }

            return characters;
        }
    }

    public bool DrawBorder { get; private set; } = false;

    public ImGuiWindowFlags EditorFlags { get; private set; } =
        ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.NoMove;

    public bool ShowLineNumbers { get; private set; } = true;

    public bool ShowWhitespace { get; private set; } = true;

    public float LineSpacing { get; private set; } = 1;

    public void SetShowLineNumbers(bool value)
    {
        ShowLineNumbers = value;
    }

    public void SetShowWhitespace(bool value)
    {
        ShowWhitespace = value;
    }
}
