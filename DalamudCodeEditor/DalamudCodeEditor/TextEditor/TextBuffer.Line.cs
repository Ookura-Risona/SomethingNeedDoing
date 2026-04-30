namespace DalamudCodeEditor.TextEditor;

public partial class TextBuffer
{
    public int LongestLine
    {
        get => GetLongestLine();
    }

    private int GetLongestLine()
    {
        var longest = 0;

        foreach (var line in lines)
        {
            var length = 0;

            foreach (var glyph in line)
            {
                length += glyph.Rune.Value == '\t' ? Style.TabSize : 1; // Tab counts as 4 visual spaces
            }

            if (length > longest)
            {
                longest = length;
            }
        }

        return longest;
    }

    public void InsertLine(int index, Line line)
    {
        MarkDirty();
        lines.Insert(index, line);
    }

    public void RemoveLine(int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            RemoveLine(start);
        }
    }

    public void RemoveLine(int line)
    {
        lines.RemoveAt(line);
        Buffer.MarkDirty();
    }


    public void ReplaceLine(int index, Line line)
    {
        MarkDirty();
        lines[index] = line;
    }

    public void AddLine(Line line)
    {
        MarkDirty();
        lines.Add(line);
    }

    public void AddLine()
    {
        AddLine([]);
    }

    public Line GetLine(int index)
    {
        return lines[index];
    }

    public Line GetCurrentLine()
    {
        return lines[Cursor.GetPosition().Line];
    }
}
