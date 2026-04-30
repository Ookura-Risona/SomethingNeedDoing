namespace DalamudCodeEditor.TextEditor;

public class Selection(Editor editor) : EditorComponent(editor)
{
    public Coordinate Start
    {
        get => State.SelectionStart;
    }

    public Coordinate End
    {
        get => State.SelectionEnd;
    }

    public bool HasSelection
    {
        get => Start.Line != End.Line || Start.Column != End.Column;
    }

    public string Text
    {
        get
        {
            var (start, end) = GetOrderedPositions();
            return Buffer.GetText(start, end);
        }
    }

    public void SetStart(Coordinate start)
    {
        Set(start, End);
    }

    public void SetEnd(Coordinate end)
    {
        Set(Start, end);
    }

    public void SetToPoint(Coordinate point)
    {
        Set(point, point);
    }

    public (Coordinate, Coordinate) GetOrderedPositions()
    {
        return Start <= End ? (Start, End) : (End, Start);
    }

    public void Set(Coordinate point, SelectionMode mode = SelectionMode.Normal)
    {
        Set(point, point, mode);
    }

    public void Set(Coordinate a, Coordinate b, SelectionMode mode = SelectionMode.Normal)
    {
        State.SetSelection(a, b, mode);
    }

    public void SelectAll()
    {
        Set(new Coordinate(0, 0), new Coordinate(Buffer.GetLines().Count, 0));
    }
}
