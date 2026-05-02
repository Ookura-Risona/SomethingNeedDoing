namespace DalamudCodeEditor.TextEditor;

public class InputManager(Editor editor) : EditorComponent(editor)
{
    public Keyboard Keyboard = new(editor);

    public Mouse Mouse = new(editor);
}
