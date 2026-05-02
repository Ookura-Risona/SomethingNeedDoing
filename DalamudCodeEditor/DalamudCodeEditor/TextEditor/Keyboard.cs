using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Keyboard(Editor editor) : EditorComponent(editor)
{
    public delegate void InputAction();

    private InputAction RequireWritable(InputAction action)
    {
        return () =>
        {
            if (!editor.IsReadOnly)
            {
                action();
            }
        };
    }

    private readonly List<(KeyBinding binding, InputAction action)> KeyBindings = new();

    public ImGuiIOPtr IO
    {
        get => ImGui.GetIO();
    }

    public bool Shift
    {
        get => IO.KeyShift;
    }

    public bool Ctrl
    {
        get => IO.ConfigMacOSXBehaviors ? IO.KeySuper : IO.KeyCtrl;
    }

    public bool Alt
    {
        get => IO.ConfigMacOSXBehaviors ? IO.KeyCtrl : IO.KeyAlt;
    }

    public void InitializeKeyboardBindings()
    {
        // Undo/Redo
        KeyBindings.Add((new KeyBinding(ImGuiKey.Z).CtrlDown(), RequireWritable(UndoManager.Undo)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Backspace).AltDown(), RequireWritable(UndoManager.Undo)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Y).CtrlDown(), RequireWritable(UndoManager.Redo)));

        // Cursor Movement
        KeyBindings.Add((new KeyBinding(ImGuiKey.UpArrow).ShiftIgnored(), () => Cursor.MoveUp()));
        KeyBindings.Add((new KeyBinding(ImGuiKey.DownArrow).ShiftIgnored(), () => Cursor.MoveDown()));
        KeyBindings.Add((new KeyBinding(ImGuiKey.LeftArrow).ShiftIgnored().CtrlIgnored(), () => Cursor.MoveLeft()));
        KeyBindings.Add((new KeyBinding(ImGuiKey.RightArrow).ShiftIgnored().CtrlIgnored(), () => Cursor.MoveRight()));
        KeyBindings.Add((new KeyBinding(ImGuiKey.PageUp).ShiftIgnored().CtrlIgnored(), Cursor.PageUp));
        KeyBindings.Add((new KeyBinding(ImGuiKey.PageDown).ShiftIgnored().CtrlIgnored(), Cursor.PageDown));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Home).CtrlDown().ShiftIgnored(), Cursor.MoveTop));
        KeyBindings.Add((new KeyBinding(ImGuiKey.End).CtrlDown().ShiftIgnored(), Cursor.MoveBottom));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Home).ShiftIgnored(), Cursor.MoveHome));
        KeyBindings.Add((new KeyBinding(ImGuiKey.End).ShiftIgnored(), Cursor.MoveEnd));

        // Other
        KeyBindings.Add((new KeyBinding(ImGuiKey.Delete), RequireWritable(Buffer.Delete)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Delete).CtrlDown(), RequireWritable(Buffer.DeleteGroup)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Backspace), RequireWritable(Buffer.Backspace)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Backspace).CtrlDown(), RequireWritable(Buffer.BackspaceGroup)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.A).CtrlDown(), Selection.SelectAll));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Enter), RequireWritable(Enter)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.KeypadEnter), RequireWritable(Enter)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Tab).ShiftIgnored(), RequireWritable(() => Buffer.EnterMultipleCharacters(Style.Tab))));

        // Clipboard
        KeyBindings.Add((new KeyBinding(ImGuiKey.C).CtrlDown(), Clipboard.Copy));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Insert).CtrlDown(), Clipboard.Copy));
        KeyBindings.Add((new KeyBinding(ImGuiKey.V).CtrlDown(), RequireWritable(Clipboard.Paste)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Insert).ShiftDown(), RequireWritable(Clipboard.Paste)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.X).CtrlDown(), RequireWritable(Clipboard.Cut)));
        KeyBindings.Add((new KeyBinding(ImGuiKey.Delete).ShiftDown(), RequireWritable(Clipboard.Cut)));
    }

    public void HandleInput()
    {
        if (!ImGui.IsWindowFocused())
        {
            return;
        }

        IO.WantCaptureKeyboard = true;
        IO.WantTextInput = true;

        foreach (var (binding, action) in KeyBindings)
        {
            if (ImGui.IsKeyPressed(binding.Key) && binding.Matches(Ctrl, Shift, Alt))
            {
                action();
                return;
            }
        }

        if (editor.IsReadOnly)
        {
            return;
        }

        for (var i = 0; i < IO.InputQueueCharacters.Size; i++)
        {
            var utf8 = (uint)IO.InputQueueCharacters[i];
            if (utf8 == 0 || utf8 < 32 && utf8 != '\n')
            {
                continue;
            }

            foreach (var character in char.ConvertFromUtf32((int)utf8))
            {
                Buffer.EnterCharacter(character);
            }
        }
    }

    private void Enter()
    {
        Buffer.EnterCharacter('\n');
        var pos = Cursor.GetPosition();
        pos.Column = 0;
        Cursor.SetPosition(pos);
    }
}
