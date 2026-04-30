using System.Numerics;
using Dalamud.Bindings.ImGui;
using DalamudCodeEditor.TextEditor;

namespace DalamudCodeEditor;

public static class Screen
{
    public static Coordinate ScreenPositionToCoordinates(Vector2 position, Editor editor)
    {
        var origin = ImGui.GetCursorScreenPos();
        var local = position - origin;

        var renderer = editor.Renderer;
        var lineHeight = renderer.LineHeight;
        var gutterWidth = renderer.GutterWidth;
        var spaceWidth = ImGui.CalcTextSize(" ").X;

        var line = Math.Clamp((int)Math.Floor(local.Y / lineHeight), 0, editor.Buffer.LineCount - 1);
        var column = editor.Buffer.GetLine(line).GetColumnAtX(local.X - gutterWidth, spaceWidth, editor.Style.TabSize);

        return new Coordinate(line, column).Sanitized(editor);
    }
}
