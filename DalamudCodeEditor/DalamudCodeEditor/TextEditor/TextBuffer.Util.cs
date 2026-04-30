using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public partial class TextBuffer
{
    public int GetLineMaxColumn(int line)
    {
        if (line < 0 || line >= LineCount)
        {
            return 0;
        }

        var glyphs = lines[line];
        var column = 0;

        for (var i = 0; i < glyphs.Count; i++)
        {
            var rune = glyphs[i].Rune;

            if (rune.Value == '\t')
            {
                column = column / Style.TabSize * Style.TabSize + Style.TabSize;
            }
            else
            {
                column++;
            }
        }

        return column;
    }

    public Coordinate FindWordStart(Coordinate from)
    {
        if (from.Line >= lines.Count)
        {
            return from;
        }

        var line = lines[from.Line];
        var index = GetCharacterIndex(from);

        if (index >= line.Count)
        {
            if (line.Count == 0)
            {
                return new Coordinate(from.Line, 0);
            }

            index = line.Count - 1;
        }

        if (line[index].IsWhiteSpace())
        {
            while (index > 0 && line[index - 1].IsWhiteSpace())
            {
                index--;
            }

            if (index > 0)
            {
                index--;
            }
            else
            {
                return new Coordinate(from.Line, GetCharacterColumn(from.Line, 0));
            }
        }

        var initialColor = line[index].Color;

        while (index > 0)
        {
            var prevChar = line[index - 1];

            if (prevChar.IsWhiteSpace())
            {
                break;
            }

            if (prevChar.Color != initialColor)
            {
                break;
            }

            index--;
        }

        return new Coordinate(from.Line, GetCharacterColumn(from.Line, index));
    }

    public Coordinate FindWordEnd(Coordinate aFrom)
    {
        var at = aFrom;
        if (at.Line >= lines.Count)
        {
            return at;
        }

        var line = lines[at.Line];
        var cindex = GetCharacterIndex(at);

        if (cindex >= line.Count)
        {
            return at;
        }

        var prevspace = char.IsWhiteSpace((char)line[cindex].Rune.Value);
        var cstartColor = line[cindex].Color;

        while (cindex < line.Count)
        {
            var rune = line[cindex].Rune;
            var isSpace = char.IsWhiteSpace((char)rune.Value);

            // Stop if color changes
            if (cstartColor != line[cindex].Color)
            {
                break;
            }

            // If whitespace state changes, handle trailing whitespace and break
            if (prevspace != isSpace)
            {
                if (isSpace)
                {
                    while (cindex < line.Count && char.IsWhiteSpace((char)line[cindex].Rune.Value))
                    {
                        ++cindex;
                    }
                }

                break;
            }

            ++cindex; // advance by one rune/glyph
        }

        return new Coordinate(aFrom.Line, GetCharacterColumn(aFrom.Line, cindex));
    }

    public int GetCharacterIndex(Coordinate coord)
    {
        if (coord.Line >= lines.Count)
        {
            return -1;
        }

        var line = lines[coord.Line];
        var visualCol = 0;

        for (var i = 0; i < line.Count; i++)
        {
            var glyph = line[i];
            var charWidth = GlyphHelper.GetGlyphDisplayWidth(glyph);

            if (visualCol + charWidth > coord.Column)
            {
                return i;
            }

            visualCol += charWidth;
        }

        return line.Count;
    }

    public int GetCharacterColumn(int lineNum, int index)
    {
        if (lineNum >= lines.Count)
        {
            return 0;
        }

        var line = lines[lineNum];
        var visualCol = 0;

        for (var i = 0; i < index && i < line.Count; i++)
        {
            visualCol += GlyphHelper.GetGlyphDisplayWidth(line[i]);
        }

        return visualCol;
    }


    public bool IsOnWordBoundary(Coordinate at)
    {
        if (at.Line >= lines.Count || at.Column == 0)
        {
            return true;
        }

        var line = lines[at.Line];
        var characterIndex = GetCharacterIndex(at);
        if (characterIndex >= line.Count)
        {
            return true;
        }

        if (Colorizer.Enabled)
        {
            return line[characterIndex].Color != line[characterIndex - 1].Color;
        }

        return line[characterIndex].IsWhiteSpace() != line[characterIndex - 1].IsWhiteSpace();
    }

    public float TextDistanceToLineStart(Coordinate position)
    {
        var line = Buffer.GetLine(position.Line);
        var spaceSize = ImGui.CalcTextSize(" ").X;
        var distance = 0.0f;

        // var colIndex = Buffer.GetCharacterIndex(position);
        var i = 0;
        while (i < position.Column && i < line.Count)
        {
            var glyph = line[i];
            var rune = glyph.Rune;

            if (rune.Value == '\t')
            {
                var tabWidth = Style.TabSize * spaceSize;
                distance = (float)(Math.Floor(distance / tabWidth) + 1) * tabWidth;
                i++;
            }
            else if (rune.Value == ' ')
            {
                distance += spaceSize;
                i++;
            }
            else
            {
                // collect a run of same-color, non-whitespace glyphs to match renderer
                var runStart = i;
                var runText = "";

                while (i < position.Column && i < line.Count)
                {
                    var r = line[i].Rune;
                    if (r.Value == '\t' || r.Value == ' ')
                    {
                        break;
                    }

                    runText += r.ToString();
                    i++;
                }

                distance += ImGui.CalcTextSize(runText).X;
            }
        }

        return distance;
    }

    public float GetLongestRenderedLineWidth()
    {
        var maxWidth = 0f;
        var spaceSize = ImGui.CalcTextSize(" ").X;
        var tabSize = Style.TabSize * spaceSize;

        foreach (var line in lines)
        {
            var width = line.GetRenderedWidth(spaceSize, tabSize);
            if (width > maxWidth)
            {
                maxWidth = width;
            }
        }

        return maxWidth;
    }
}
