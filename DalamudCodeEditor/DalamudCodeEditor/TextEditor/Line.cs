using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Line : Collection<Glyph>
{
    public Line() : base(new List<Glyph>())
    {
    }

    public void AddRange(IEnumerable<Glyph> glyphs)
    {
        foreach (var g in glyphs)
        {
            Add(g);
        }
    }

    public void RemoveRange(int index, int count)
    {
        for (var i = 0; i < count; i++)
        {
            // Remove at the same index since it shuffles back with each removal
            RemoveAt(index);
        }
    }

    public List<Glyph> GetGroupedGlyphsBeforeCursor(Cursor cursor)
    {
        var position = cursor.GetPosition();
        if (position.Column == 0)
        {
            return [];
        }


        var target = GetGlyphBeforeCursor(cursor);
        if (target == null)
        {
            return [];
        }

        List<Glyph> run = [];

        for (var i = position.Column - 1; i >= 0; i--)
        {
            var glyph = this[i];
            if (!target.Value.IsGroupable(glyph))
            {
                break;
            }

            run.Insert(0, glyph);
        }

        return run;
    }

    public List<Glyph> GetGroupedGlyphsAfterCursor(Cursor cursor)
    {
        var position = cursor.GetPosition();
        var line = this;
        if (position.Column >= line.Count)
        {
            return [];
        }

        var target = GetGlyphUnderCursor(cursor);
        if (target == null)
        {
            return [];
        }

        List<Glyph> run = [];

        for (var i = position.Column; i < line.Count; i++)
        {
            var glyph = line[i];
            if (!target.Value.IsGroupable(glyph))
            {
                break;
            }

            run.Add(glyph);
        }

        return run;
    }

    public Glyph? GetGlyphBeforeCursor(Cursor cursor)
    {
        var column = cursor.GetPosition().Column;
        if (column == 0)
        {
            return null;
        }

        return this[column - 1];
    }

    public Glyph? GetGlyphUnderCursor(Cursor cursor)
    {
        return this[cursor.GetPosition().Column];
    }

    public List<Glyph> GetGraphemeClusterBeforeCursor(Cursor cursor)
    {
        var column = cursor.GetPosition().Column;
        if (column == 0)
        {
            return [];
        }

        var slice = this.Take(column).ToList();
        var text = string.Concat(slice.Select(g => g.Rune.ToString()));

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        var clusterStart = 0;
        var clusterText = "";

        while (enumerator.MoveNext())
        {
            clusterStart = enumerator.ElementIndex;
            clusterText = enumerator.GetTextElement(); // save last
        }

        var cluster = new List<Glyph>();
        for (var i = column - 1; i >= 0; i--)
        {
            cluster.Insert(0, this[i]);
            var test = string.Concat(cluster.Select(g => g.Rune.ToString()));
            if (test == clusterText)
            {
                return cluster;
            }
        }

        return [this[column - 1]];
    }

    public float GetRenderedWidth(float spaceSize, float tabSize)
    {
        var width = 0f;
        var i = 0;

        while (i < Count)
        {
            var glyph = this[i];

            if (glyph.IsTab())
            {
                width = (float)(Math.Floor(width / tabSize) + 1) * tabSize;
                i++;
            }
            else if (glyph.Rune.Value == ' ')
            {
                width += spaceSize;
                i++;
            }
            else
            {
                var run = "";

                while (i < Count)
                {
                    var r = this[i].Rune;
                    if (r.Value == '\t' || r.Value == ' ')
                    {
                        break;
                    }

                    run += r.ToString();
                    i++;
                }

                width += ImGui.CalcTextSize(run).X;
            }
        }

        return width;
    }

    public int GetColumnAtX(float x, float spaceSize, float tabSize)
    {
        var width = 0f;
        var col = 0;
        var i = 0;

        while (i < Count)
        {
            var rune = this[i].Rune;

            if (rune.Value == '\t')
            {
                var nextTabStop = (float)(Math.Floor(width / tabSize) + 1) * tabSize;

                if (x < nextTabStop)
                {
                    return col;
                }

                width = nextTabStop;
                col++;
                i++;
            }
            else if (rune.Value == ' ')
            {
                if (x < width + spaceSize)
                {
                    return col;
                }

                width += spaceSize;
                col++;
                i++;
            }
            else
            {
                var runSb = new StringBuilder();
                var runStartIndex = i;

                while (i < Count)
                {
                    var r = this[i].Rune;
                    if (r.Value is '\t' or ' ')
                    {
                        break;
                    }

                    runSb.Append(r.ToString());
                    i++;
                    col++;
                }

                var runText = runSb.ToString();
                var runWidth = ImGui.CalcTextSize(runText).X;

                if (x < width + runWidth)
                {
                    var runAccumWidth = width;

                    for (var j = runStartIndex; j < i; j++)
                    {
                        var glyphText = this[j].Rune.ToString();
                        var glyphWidth = ImGui.CalcTextSize(glyphText).X;

                        if (x < runAccumWidth + glyphWidth / 2)
                        {
                            return j;
                        }

                        runAccumWidth += glyphWidth;
                    }

                    return i;
                }

                width += runWidth;
            }
        }

        return Count;
    }

    public override string ToString()
    {
        return string.Concat(this.Select(g => g.ToString()));
    }
}
