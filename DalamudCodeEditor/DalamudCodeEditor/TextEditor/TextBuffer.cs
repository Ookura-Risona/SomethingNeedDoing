using System.Text;

namespace DalamudCodeEditor.TextEditor;

public partial class TextBuffer(Editor editor) : DirtyTrackable(editor)
{
    private readonly List<Line> lines = [new()];

    public int LineCount
    {
        get => lines.Count;
    }

    public bool IsEmpty
    {
        get => LineCount == 0;
    }

    public void SetText(string text)
    {
        MarkDirty();
        lines.Clear();
        AddLine();

        foreach (var chr in text)
        {
            if (chr == '\r')
            {
                continue;
            }

            if (chr == '\n')
            {
                AddLine();
            }
            else if (chr == '\t')
            {
                for (var i = 0; i < editor.Style.TabSize; i++)
                {
                    lines.Last().Add(new Glyph(Style.TabCharacter));
                }
            }
            else
            {
                lines.Last().Add(new Glyph(chr));
            }
        }

        Colorizer.Colorize(0, LineCount);
        Scroll.RequestScrollToTop();
    }

    public string GetText()
    {
        StringBuilder sb = new();
        for (var i = 0; i < lines.Count; i++)
        {
            foreach (var glyph in lines[i])
            {
                sb.Append(glyph.Rune);
            }

            if (i < lines.Count - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    public void Clear()
    {
        MarkDirty();
        lines.Clear();
        AddLine();
    }

    public string GetText(Coordinate start, Coordinate end)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var result = new StringBuilder();

        for (var line = start.Line; line <= end.Line && line < lines.Count; line++)
        {
            var lineGlyphs = lines[line];

            var colStart = line == start.Line ? start.Column : 0;
            var colEnd = line == end.Line ? end.Column : lineGlyphs.Count;

            colStart = Math.Clamp(colStart, 0, lineGlyphs.Count);
            colEnd = Math.Clamp(colEnd, 0, lineGlyphs.Count);

            for (var col = colStart; col < colEnd; col++)
            {
                result.Append(lineGlyphs[col].Rune);
            }

            if (line < end.Line)
            {
                result.Append('\n');
            }
        }

        return result.ToString();
    }

    public void EnterCharacter(char c)
    {
        var shift = InputManager.Keyboard.Shift;
        UndoManager.Create(() =>
        {
            if (c == '\t' && Selection.HasSelection && SelectionSpansMultipleLines())
            {
                HandleTabIndentation(shift);
            }
            else
            {
                if (Selection.HasSelection)
                {
                    Buffer.DeleteSelection();
                }

                InsertCharacterAtCursor(c);
            }
        });

        Buffer.MarkDirty();
        Colorizer.Colorize(Cursor.GetPosition().Line - 1, 3);
        Cursor.EnsureVisible();
    }

    public void EnterMultipleCharacters(IEnumerable<char> characters)
    {
        var shift = InputManager.Keyboard.Shift;
        UndoManager.Create(() =>
        {
            foreach (var c in characters)
            {
                if (c == '\t' && Selection.HasSelection && SelectionSpansMultipleLines())
                {
                    HandleTabIndentation(shift);
                }
                else
                {
                    if (Selection.HasSelection)
                    {
                        Buffer.DeleteSelection();
                    }

                    InsertCharacterAtCursor(c);
                }
            }
        });

        Buffer.MarkDirty();
        Colorizer.Colorize(Cursor.GetPosition().Line - 1, 3);
        Cursor.EnsureVisible();
    }

    private void HandleTabIndentation(bool shift)
    {
        var (start, end) = Selection.GetOrderedPositions();
        var originalEnd = end;

        start = new Coordinate(start.Line, 0);
        if (end.Column == 0 && end.Line > 0)
        {
            end = new Coordinate(end.Line - 1, Buffer.GetLineMaxColumn(end.Line - 1));
        }
        else
        {
            end = new Coordinate(end.Line, Buffer.GetLineMaxColumn(end.Line));
        }

        var modified = false;

        for (var i = start.Line; i <= end.Line; i++)
        {
            if (i < 0 || i >= lines.Count)
            {
                continue;
            }

            var line = lines[i];
            if (shift)
            {
                if (line.Count > 0)
                {
                    if (line[0].IsTab())
                    {
                        line.RemoveAt(0);
                        modified = true;
                    }
                    else
                    {
                        for (var j = 0; j < Style.TabSize && line.Count > 0 && line[0].Rune.Value == ' '; j++)
                        {
                            line.RemoveAt(0);
                            modified = true;
                        }
                    }
                }
            }
            else
            {
                line.Insert(0, new Glyph('\t', PaletteIndex.Background));
                modified = true;
            }
        }

        if (!modified)
        {
            return;
        }

        State.SelectionStart = new Coordinate(start.Line, Buffer.GetCharacterColumn(start.Line, 0));
        State.SelectionEnd = originalEnd.Column != 0
            ? new Coordinate(end.Line, Buffer.GetLineMaxColumn(end.Line))
            : new Coordinate(end.Line - 1, Buffer.GetLineMaxColumn(end.Line - 1));
    }

    private bool SelectionSpansMultipleLines()
    {
        return State.SelectionStart.Line != State.SelectionEnd.Line;
    }

    private void InsertCharacterAtCursor(char c)
    {
        var coord = Cursor.GetPosition();
        var line = GetCurrentLine();

        if (c == '\n')
        {
            InsertLine(coord.Line + 1, new Line());
            var newLine = lines[coord.Line + 1];

            var splitIndex = coord.Column;

            newLine.AddRange(line.Skip(splitIndex));
            line.RemoveRange(splitIndex, line.Count - splitIndex);

            Cursor.SetPosition(new Coordinate(coord.Line + 1, 0));
        }
        else
        {
            var insertIndex = coord.Column;

            if (!Rune.TryCreate(c, out var rune))
            {
                return;
            }

            Span<char> chars = stackalloc char[2];
            var len = rune.EncodeToUtf16(chars);

            for (var i = 0; i < len; i++)
            {
                line.Insert(insertIndex + i, new Glyph(chars[i]));
            }

            var newColumn = coord.Column + GlyphHelper.GetGlyphDisplayWidth(new Glyph(rune));
            Cursor.SetPosition(new Coordinate(coord.Line, newColumn));
        }
    }


    public List<Line> GetLines()
    {
        return lines;
    }
}
