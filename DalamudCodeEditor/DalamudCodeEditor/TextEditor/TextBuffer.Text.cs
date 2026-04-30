namespace DalamudCodeEditor.TextEditor;

public partial class TextBuffer
{
    public void InsertText(string value)
    {
        if (value.Trim() == "")
        {
            return;
        }

        var selectionSart = Selection.GetOrderedPositions().Item1;

        var pos = Cursor.GetPosition();
        var start = pos < selectionSart ? pos : selectionSart;
        var totalLines = pos.Line - start.Line;

        totalLines += InsertTextAt(pos, value);

        Selection.Set(pos);
        Cursor.SetPosition(pos);
        Colorizer.Colorize(start.Line - 1, totalLines + 2);
    }

    public int InsertTextAt(Coordinate where, string value)
    {
        MarkDirty();
        return TextInsertionHelper.InsertTextAt(lines, where, value, Style.TabSize);
    }
}
