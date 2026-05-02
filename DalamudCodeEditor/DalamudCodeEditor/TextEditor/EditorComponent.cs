namespace DalamudCodeEditor.TextEditor;

public abstract class EditorComponent(Editor _editor)
{
    public TextBuffer Buffer
    {
        get => _editor.Buffer;
    }

    public Style Style
    {
        get => _editor.Style;
    }

    public Renderer Renderer
    {
        get => _editor.Renderer;
    }

    public Colorizer Colorizer
    {
        get => _editor.Colorizer;
    }

    public UndoManager UndoManager
    {
        get => _editor.UndoManager;
    }

    public InputManager InputManager
    {
        get => _editor.InputManager;
    }

    public Cursor Cursor
    {
        get => _editor.Cursor;
    }

    public Scroll Scroll
    {
        get => _editor.Scroll;
    }

    public Selection Selection
    {
        get => _editor.Selection;
    }

    public Clipboard Clipboard
    {
        get => _editor.Clipboard;
    }

    public State State
    {
        get => _editor.State;
    }

    public Palette Palette
    {
        get => _editor.Palette;
    }
}
