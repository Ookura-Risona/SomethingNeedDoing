using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Mouse(Editor editor) : EditorComponent(editor)
{
    private float LastClick = -1f;

    private ImGuiIOPtr IO
    {
        get => ImGui.GetIO();
    }

    private Keyboard Keyboard
    {
        get => InputManager.Keyboard;
    }

    public Vector2 Position
    {
        get => ImGui.GetMousePos();
    }

    public bool LeftDown
    {
        get => ImGui.IsMouseDown(ImGuiMouseButton.Left);
    }

    public bool LeftDrag
    {
        get => ImGui.IsMouseDragging(ImGuiMouseButton.Left);
    }

    public class MouseState
    {
        public bool Click { get; init; } = false;

        public bool DoubleClick { get; init; } = false;

        public bool TripleClick { get; init; } = false;
    }

    public MouseState GetState()
    {
        var time = ImGui.GetTime();

        var click = ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        var doubleClick = ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left);

        var tripleClick = click && !doubleClick && LastClick != -1f &&
                          time - LastClick < IO.MouseDoubleClickTime;

        var state = new MouseState
        {
            Click = click,
            DoubleClick = doubleClick,
            TripleClick = tripleClick,
        };

        if (state.TripleClick)
        {
            LastClick = -1f;
        }
        else if (state.DoubleClick || state.Click)
        {
            LastClick = (float)time;
        }

        return state;
    }

    public void HandleInput()
    {
        if (!ImGui.IsWindowHovered() || Keyboard.Shift || Keyboard.Alt)
        {
            return;
        }

        var state = GetState();
        var position = Screen.ScreenPositionToCoordinates(Position, editor);

        if (!Keyboard.Ctrl)
        {
            if (state.TripleClick)
            {
                Cursor.SetPosition(position);
                Selection.Set(position, SelectionMode.Line);
                return;
            }

            if (state.DoubleClick)
            {
                Cursor.SetPosition(position);
                Selection.Set(position, SelectionMode.Word);
                return;
            }
        }

        if (state.Click)
        {
            Cursor.SetPosition(position);
            var mode = Keyboard.Ctrl ? SelectionMode.Word : SelectionMode.Normal;
            Selection.Set(position, mode);
            return;
        }

        if (LeftDown && LeftDrag)
        {
            IO.WantCaptureMouse = true;
            Cursor.SetPosition(position);
            Selection.SetEnd(position);
        }
    }
}
