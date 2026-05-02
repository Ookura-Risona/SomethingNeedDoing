using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace DalamudCodeEditor.TextEditor;

public partial class Editor
{
    // Components
    public readonly TextBuffer Buffer;

    public readonly Style Style;

    public readonly Renderer Renderer;

    public readonly Colorizer Colorizer;

    public readonly UndoManager UndoManager;

    public readonly InputManager InputManager;

    public readonly Cursor Cursor;

    public readonly Scroll Scroll;

    public readonly Selection Selection;

    public readonly Clipboard Clipboard;

    public State State;

    public long StartTime;

    // Properties
    public bool IsReadOnly { get; private set; }

    public Editor()
    {
        Buffer = new TextBuffer(this);
        Style = new Style(this);
        Renderer = new Renderer(this);
        Colorizer = new Colorizer(this);
        UndoManager = new UndoManager(this);
        InputManager = new InputManager(this);
        Cursor = new Cursor(this);
        Scroll = new Scroll(this);
        Selection = new Selection(this);
        Clipboard = new Clipboard(this);
        State = new State(this);

        InputManager.Keyboard.InitializeKeyboardBindings();
        Language = new LuaLanguageDefinition();

        Colorizer.Colorize();
    }

    public void Draw(string title, Vector2 size = new())
    {
        State.CursorPosition = Cursor.GetPosition();

        Renderer.Start();
        Buffer.MarkClean();
        Cursor.MarkClean();

        using var color = ImRaii.PushColor(ImGuiCol.ChildBg, Palette.Background);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        using var child = ImRaii.Child(title, size, Style.DrawBorder, Style.EditorFlags);

        // Handle inputs within the child window
        InputManager.Keyboard.HandleInput();
        InputManager.Mouse.HandleInput();

        Colorizer.ProcessColorizationQueue();

        Renderer.Render();

        Renderer.End();
        Scroll.ScrollToCursor();
    }

    public void SetReadOnly(bool value)
    {
        IsReadOnly = value;
    }
}
