namespace DalamudCodeEditor.TextEditor;

public partial class TextBuffer
{
    public void Delete()
    {
        if (Buffer.IsEmpty || Cursor.IsAtEndOfFile())
        {
            return;
        }

        UndoManager.Create(() =>
        {
            if (Selection.HasSelection)
            {
                DeleteSelection();
                return;
            }

            var pos = Cursor.GetPosition();
            var line = Buffer.GetLine(pos.Line);

            if (Cursor.IsAtEndOfLine())
            {
                var nextLine = Buffer.GetLine(pos.Line + 1);
                line.AddRange(nextLine);
                Buffer.RemoveLine(pos.Line + 1);
            }
            else
            {
                var deleteIndex = pos.Column;

                if (deleteIndex >= 0 && deleteIndex < line.Count)
                {
                    line.RemoveAt(deleteIndex);
                }
            }

            Buffer.MarkDirty();
            Colorizer.Colorize(pos.Line, 1);
            Cursor.EnsureVisible();
        });
    }

    public void DeleteGroup()
    {
        if (Buffer.IsEmpty || Cursor.IsAtEndOfFile())
        {
            return;
        }

        if (Cursor.IsAtEndOfLine())
        {
            Delete();
            return;
        }

        UndoManager.Create(() =>
        {
            if (Selection.HasSelection)
            {
                DeleteSelection();
                return;
            }

            var line = GetCurrentLine();
            var target = line.GetGroupedGlyphsAfterCursor(Cursor);
            line.RemoveRange(Cursor.GetPosition().Column, target.Count);
        });
    }


    internal void DeleteRange(Coordinate start, Coordinate end)
    {
        if (start == end)
        {
            return;
        }

        if (start > end)
        {
            (start, end) = (end, start);
        }

        var lines = Buffer.GetLines();

        if (start.Line >= lines.Count || end.Line >= lines.Count)
        {
            return;
        }

        if (start.Line == end.Line)
        {
            var line = lines[start.Line];
            var startCol = Math.Clamp(start.Column, 0, line.Count);
            var endCol = Math.Clamp(end.Column, 0, line.Count);

            line.RemoveRange(startCol, endCol - startCol);
        }
        else
        {
            var firstLine = lines[start.Line];
            var lastLine = lines[end.Line];

            var startCol = Math.Clamp(start.Column, 0, firstLine.Count);
            var endCol = Math.Clamp(end.Column, 0, lastLine.Count);

            var merged = new Line();
            merged.AddRange(firstLine.Take(startCol));
            merged.AddRange(lastLine.Skip(endCol));

            lines[start.Line] = merged;

            Buffer.RemoveLine(start.Line + 1, end.Line + 1);
        }

        Buffer.MarkDirty();
    }

    public void Backspace()
    {
        if (Buffer.IsEmpty || Cursor.IsAtStartOfFile())
        {
            return;
        }

        UndoManager.Create(() =>
        {
            if (Selection.HasSelection)
            {
                DeleteSelection();
                return;
            }

            var pos = Cursor.GetPosition();

            if (Cursor.IsAtStartOfLine())
            {
                var prevLineIndex = pos.Line - 1;
                var prevLine = Buffer.GetLine(prevLineIndex);
                var currentLine = Buffer.GetLine(pos.Line);

                State.CursorPosition.Line = prevLineIndex;
                State.CursorPosition.Column = prevLine.Count;

                Buffer.RemoveLine(pos.Line);
                prevLine.AddRange(currentLine);
            }
            else
            {
                var line = Buffer.GetLine(pos.Line);
                var deleteIndex = pos.Column - 1;

                if (deleteIndex < 0 || deleteIndex >= line.Count)
                {
                    return;
                }

                line.RemoveAt(deleteIndex);
                State.CursorPosition.Column = deleteIndex;
            }

            Buffer.MarkDirty();
            Cursor.EnsureVisible();
            Colorizer.Colorize(State.CursorPosition.Line, 1);
        });
    }

    public void BackspaceGroup()
    {
        if (Buffer.IsEmpty || Cursor.IsAtStartOfFile())
        {
            return;
        }

        if (Cursor.IsAtStartOfLine())
        {
            Backspace();
            return;
        }

        UndoManager.Create(() =>
        {
            if (Selection.HasSelection)
            {
                DeleteSelection();
                return;
            }

            var line = GetCurrentLine();
            var target = line.GetGroupedGlyphsBeforeCursor(Cursor);
            var pos = Cursor.GetPosition();

            line.RemoveRange(pos.Column - target.Count, target.Count);
            Cursor.SetPosition(pos.WithColumn(pos.Column - target.Count));
        });
    }


    public void DeleteSelection()
    {
        if (State.SelectionEnd == State.SelectionStart)
        {
            return;
        }

        var (start, end) = Selection.GetOrderedPositions();

        DeleteRange(start, end);

        Selection.Set(start);
        Cursor.SetPosition(start);
        Colorizer.Colorize(start.Line, 1);
    }
}
