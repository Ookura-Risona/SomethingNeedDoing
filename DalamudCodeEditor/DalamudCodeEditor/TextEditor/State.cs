namespace DalamudCodeEditor.TextEditor;

public class State(Editor editor) : EditorComponent(editor)
{
    public Coordinate SelectionStart = new();

    public Coordinate SelectionEnd = new();

    public Coordinate CursorPosition = new();

    public void SwapEndsIfNeeded()
    {
        if (SelectionStart > SelectionEnd)
        {
            (SelectionStart, SelectionEnd) = (SelectionEnd, SelectionStart);
        }
    }

    public State Clone()
    {
        return new State(editor)
        {
            SelectionStart = SelectionStart,
            SelectionEnd = SelectionEnd,
            CursorPosition = CursorPosition,
        };
    }


    public void SetSelection(Coordinate start, Coordinate end, SelectionMode mode = SelectionMode.Normal)
    {
        var previousStart = SelectionStart;
        var previousEnd = SelectionEnd;

        SelectionStart = start.Sanitized(editor);
        SelectionEnd = end.Sanitized(editor);

        if (mode == SelectionMode.Word)
        {
            SelectionStart = Buffer.FindWordStart(SelectionStart);
            if (!Buffer.IsOnWordBoundary(SelectionEnd))
            {
                State.SelectionEnd = Buffer.FindWordEnd(Buffer.FindWordStart(SelectionEnd));
            }
        }

        if (mode == SelectionMode.Line)
        {
            var lineNo = State.SelectionEnd.Line;
            SelectionStart = new Coordinate(State.SelectionStart.Line, 0);
            SelectionEnd = new Coordinate(lineNo, Buffer.GetLineMaxColumn(lineNo));
        }

        if (SelectionStart != previousStart || SelectionEnd != previousEnd)
        {
            Cursor.MarkDirty();
        }
    }
}
