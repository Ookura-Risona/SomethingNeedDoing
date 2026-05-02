namespace DalamudCodeEditor.TextEditor;

public static class TextInsertionHelper
{
    public static int InsertTextAt(List<Line> lines, Coordinate where, string value, int tabSize)
    {
        var totalLines = 0;

        while (where.Line >= lines.Count)
        {
            lines.Add(new Line());
        }

        var index = where.Column;

        foreach (var rune in value.EnumerateRunes())
        {
            if (rune.Value == '\r')
            {
                continue;
            }

            if (rune.Value == '\n')
            {
                var currentLine = lines[where.Line];

                var newLine = new Line();

                if (index < currentLine.Count)
                {
                    newLine.AddRange(currentLine.Skip(index));
                    currentLine.RemoveRange(index, currentLine.Count - index);
                }

                lines.Insert(where.Line + 1, newLine);

                where.Line++;
                where.Column = 0;
                index = 0;
                totalLines++;
            }
            else
            {
                while (where.Line >= lines.Count)
                {
                    lines.Add(new Line());
                }

                var line = lines[where.Line];

                var glyph = new Glyph(rune, PaletteIndex.Default);
                line.Insert(index++, glyph);

                where.Column++;
            }
        }

        return totalLines;
    }
}
