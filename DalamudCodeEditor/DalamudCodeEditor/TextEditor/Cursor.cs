using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Cursor(Editor editor) : DirtyTrackable(editor)
{
    public void SetPosition(Coordinate position)
    {
        if (State.CursorPosition == position)
        {
            return;
        }

        editor.State.CursorPosition = position;
        MarkDirty();
        EnsureVisible();
    }

    public Coordinate GetPosition()
    {
        return State.CursorPosition.Sanitized(editor);
    }

    public void EnsureVisible()
    {
        if (Renderer.IsRendering)
        {
            Scroll.RequestScrollToCursor();
            return;
        }

        var scrollX = ImGui.GetScrollX();
        var scrollY = ImGui.GetScrollY();

        var height = ImGui.GetWindowHeight();
        var width = ImGui.GetWindowWidth();

        var top = 1 + (int)Math.Ceiling(scrollY / Renderer.LineHeight);
        var bottom = (int)Math.Ceiling((scrollY + height) / Renderer.LineHeight);


        var pos = GetPosition();

        if (pos.Line < top)
        {
            ImGui.SetScrollY(Math.Max(0f, (pos.Line - 1) * Renderer.LineHeight));
        }

        if (pos.Line > bottom - 4)
        {
            ImGui.SetScrollY(Math.Max(0f, (pos.Line + 4) * Renderer.LineHeight - height));
        }

        var cx = Buffer.TextDistanceToLineStart(pos);
        var gutter = Renderer.GutterWidth;
        var cursorX = gutter + cx;

        if (cursorX < scrollX + 4)
        {
            ImGui.SetScrollX(Math.Max(0f, cursorX - 4));
        }

        if (cursorX > scrollX + width - 4)
        {
            ImGui.SetScrollX(Math.Max(0f, cursorX + 4 - width));
        }
    }

    private void MoveCursor(Func<Coordinate, Coordinate> movementFunc)
    {
        var shift = InputManager.Keyboard.Shift;
        var previous = GetPosition();
        var newPosition = movementFunc(previous);

        if (newPosition == previous)
        {
            return;
        }

        SetPosition(newPosition);

        if (shift)
        {
            Coordinate anchor;

            if (Selection.End == previous)
            {
                anchor = Selection.Start;
            }
            else if (Selection.Start == previous)
            {
                anchor = Selection.End;
            }
            else
            {
                anchor = previous;
            }

            Selection.Set(anchor, newPosition);
        }
        else
        {
            Selection.SetToPoint(GetPosition());
        }

        EnsureVisible();
    }

    public void MoveUp(int delta = 1)
    {
        MoveCursor(pos =>
        {
            if (IsOnFirstLine())
            {
                return pos.ToHome();
            }

            return pos.WithLine(Math.Max(0, pos.Line - delta));
        });
    }

    public void MoveDown(int delta = 1)
    {
        MoveCursor(pos =>
        {
            if (IsOnLastLine())
            {
                return pos.ToEnd(editor);
            }

            return pos.WithLine(Math.Min(Buffer.LineCount - 1, pos.Line + delta));
        });
    }

    public void PageUp()
    {
        MoveUp(Renderer.GetPageSize() - 4);
    }

    public void PageDown()
    {
        MoveDown(Renderer.GetPageSize() - 4);
    }

    public void MoveTop()
    {
        MoveCursor(pos => pos.ToHome().ToFirstLine());
    }

    public void MoveBottom()
    {
        MoveCursor(pos => pos.ToLastLine(editor));
    }

    public void MoveLeft()
    {
        if (IsAtStartOfFile())
        {
            return;
        }

        MoveCursor(pos =>
        {
            if (IsAtStartOfLine())
            {
                return pos.WithLine(pos.Line - 1).ToEnd(editor).Sanitized(editor);
            }

            var line = Buffer.GetLine(pos.Line);
            if (InputManager.Keyboard.Ctrl)
            {
                var group = line.GetGroupedGlyphsBeforeCursor(Cursor);
                return pos.WithColumn(pos.Column - group.Count).Sanitized(editor);
            }

            var cluster = line.GetGraphemeClusterBeforeCursor(Cursor);
            return pos.WithColumn(pos.Column - cluster.Count).Sanitized(editor);
        });
    }

    public void MoveRight()
    {
        if (IsAtEndOfFile())
        {
            return;
        }

        MoveCursor(pos =>
        {
            if (IsAtEndOfLine())
            {
                return pos.WithLine(pos.Line + 1).ToHome().Sanitized(editor);
            }

            if (InputManager.Keyboard.Ctrl)
            {
                var line = Buffer.GetCurrentLine();
                var target = line.GetGroupedGlyphsAfterCursor(Cursor);
                return pos.WithColumn(pos.Column + target.Count).Sanitized(editor);
            }

            return pos.WithColumn(pos.Column + 1).Sanitized(editor);
        });
    }


    public void MoveHome()
    {
        MoveCursor(pos => pos.ToHome());
    }

    public void MoveEnd()
    {
        MoveCursor(pos => pos.ToEnd(editor));
    }

    public bool IsOnFirstLine()
    {
        return GetPosition().IsOnFirstLine();
    }

    public bool IsOnLastLine()
    {
        return GetPosition().IsOnLastLine(editor);
    }

    public bool IsAtStartOfLine()
    {
        return GetPosition().IsAtStartOfLine();
    }

    public bool IsAtEndOfLine()
    {
        return GetPosition().IsAtEndOfLine(editor);
    }

    public bool IsAtStartOfFile()
    {
        return GetPosition().IsAtStartOfFile();
    }

    public bool IsAtEndOfFile()
    {
        return GetPosition().IsAtEndOfFile(editor);
    }
}
