using DalamudCodeEditor.TextEditor;

namespace DalamudCodeEditor;

public class Coordinate(int line = 0, int column = 0)
{
    public int Line = line;

    public int Column = column;

    private static Coordinate Invalid()
    {
        return new Coordinate(-1, -1);
    }

    public static bool operator ==(Coordinate a, Coordinate o)
    {
        return a.Line == o.Line && a.Column == o.Column;
    }

    public static bool operator !=(Coordinate a, Coordinate o)
    {
        return a.Line != o.Line || a.Column != o.Column;
    }

    public static bool operator <(Coordinate a, Coordinate o)
    {
        if (a.Line != o.Line)
        {
            return a.Line < o.Line;
        }

        return a.Column < o.Column;
    }

    public static bool operator >(Coordinate a, Coordinate o)
    {
        if (a.Line != o.Line)
        {
            return a.Line > o.Line;
        }

        return a.Column > o.Column;
    }

    public static bool operator <=(Coordinate a, Coordinate o)
    {
        if (a.Line != o.Line)
        {
            return a.Line < o.Line;
        }

        return a.Column <= o.Column;
    }

    public static bool operator >=(Coordinate a, Coordinate o)
    {
        if (a.Line != o.Line)
        {
            return a.Line > o.Line;
        }

        return a.Column >= o.Column;
    }

    public override bool Equals(object? obj)
    {
        return obj is Coordinate o && Line == o.Line && Column == o.Column;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Line, Column);
    }

    public Coordinate Sanitized(Editor editor)
    {
        var sLine = Line;
        var sColumn = Column;
        var lineCount = editor.Buffer.LineCount;

        if (sLine >= lineCount)
        {
            if (lineCount == 0)
            {
                sLine = 0;
                sColumn = 0;
            }
            else
            {
                sLine = lineCount - 1;
                sColumn = editor.Buffer.GetLineMaxColumn(sLine);
            }

            return new Coordinate(sLine, sColumn);
        }

        sColumn = lineCount == 0 ? 0 : Math.Min(sColumn, editor.Buffer.GetLineMaxColumn(sLine));
        return new Coordinate(sLine, sColumn);
    }


    public Coordinate WithLine(int line)
    {
        return new Coordinate(line, Column);
    }

    public Coordinate WithColumn(int column)
    {
        return new Coordinate(Line, column);
    }

    public Coordinate ToHome()
    {
        return WithColumn(0);
    }

    public Coordinate ToEnd(Editor editor)
    {
        return WithColumn(editor.Buffer.GetLineMaxColumn(Line));
    }

    public Coordinate ToFirstLine()
    {
        return WithLine(0);
    }

    public Coordinate ToLastLine(Editor editor)
    {
        return WithLine(editor.Buffer.LineCount - 1);
    }


    public bool IsOnFirstLine()
    {
        return Line == 0;
    }

    public bool IsOnLastLine(Editor editor)
    {
        return Line == editor.Buffer.LineCount - 1;
    }

    public bool IsAtStartOfLine()
    {
        return Column == 0;
    }

    public bool IsAtEndOfLine(Editor editor)
    {
        return Column == editor.Buffer.GetLineMaxColumn(Line);
    }

    public bool IsAtStartOfFile()
    {
        return IsOnFirstLine() && IsAtStartOfLine();
    }

    public bool IsAtEndOfFile(Editor editor)
    {
        return IsOnLastLine(editor) && IsAtEndOfLine(editor);
    }
}
