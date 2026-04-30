using System.Text;
using System.Text.RegularExpressions;

namespace DalamudCodeEditor.TextEditor;

public class Colorizer(Editor editor) : EditorComponent(editor)
{
    public bool Enabled { get; private set; } = true;

    private bool _checkComments = false;

    private int _minLineToColorize = int.MaxValue;

    private int _maxLineToColorize = int.MinValue;


    public void Colorize(int startLine = 0, int lineCount = -1)
    {
        var totalLines = Buffer.LineCount;
        var endLine = lineCount == -1
            ? totalLines
            : Math.Min(totalLines, startLine + lineCount);

        _minLineToColorize = Math.Min(_minLineToColorize, startLine);
        _maxLineToColorize = Math.Max(_maxLineToColorize, endLine);

        _minLineToColorize = Math.Max(0, _minLineToColorize);
        _maxLineToColorize = Math.Max(_minLineToColorize, _maxLineToColorize);

        _checkComments = true;
    }

    private void ColorizeLineRange(int fromLine, int toLine)
    {
        var lines = Buffer.GetLines();
        if (lines.Count == 0 || fromLine >= toLine)
        {
            return;
        }

        toLine = Math.Min(toLine, lines.Count);

        for (var lineIndex = fromLine; lineIndex < toLine; lineIndex++)
        {
            var line = lines[lineIndex];
            if (line.Count == 0)
            {
                continue;
            }

            var lineStringBuilder = new StringBuilder(line.Count);
            for (var i = 0; i < line.Count; i++)
            {
                lineStringBuilder.Append(line[i].Rune);
                var glyph = line[i];
                glyph.Color = PaletteIndex.Default;
                line[i] = glyph;
            }

            var lineText = lineStringBuilder.ToString();

            if (editor.Language.TokenizeLine != null)
            {
                foreach (var token in editor.Language.TokenizeLine(lineText))
                {
                    for (var i = token.Start; i < token.End && i < line.Count; i++)
                    {
                        var glyph = line[i];
                        glyph.Color = token.Color;
                        line[i] = glyph;
                    }
                }
            }
            else
            {
                foreach (var (regex, color) in editor.Language.RegexTokens)
                {
                    foreach (Match match in regex.Matches(lineText))
                    {
                        for (var i = match.Index; i < match.Index + match.Length && i < line.Count; i++)
                        {
                            var glyph = line[i];
                            glyph.Color = color;
                            line[i] = glyph;
                        }
                    }
                }

                for (var i = 0; i < line.Count;)
                {
                    var glyph = line[i];

                    if (glyph.IsLetter())
                    {
                        var wordStart = i;
                        var wordEnd = i;

                        while (wordEnd < line.Count && glyph.IsLetter() || glyph.IsNumber())
                        {
                            wordEnd++;
                        }

                        var word = lineText.Substring(wordStart, wordEnd - wordStart);
                        var color = PaletteIndex.Default;

                        if (editor.Language.Keywords.Contains(word))
                        {
                            color = PaletteIndex.Keyword;
                        }
                        else if (editor.Language.Identifiers.TryGetValue(word, out var type))
                        {
                            color = PaletteIndex.Function;
                        }

                        for (var j = wordStart; j < wordEnd && j < line.Count; j++)
                        {
                            var subGlyph = line[j];
                            subGlyph.Color = color;
                            line[j] = subGlyph;
                        }

                        i = wordEnd;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }
    }

    internal void ProcessColorizationQueue()
    {
        if (Buffer.GetLines().Count == 0 || !Enabled)
        {
            return;
        }

        if (_minLineToColorize <= _maxLineToColorize)
        {
            var increment = editor.Language.TokenizeLine == null ? 50 : 500;
            var currentProcessingEndLine = Math.Min(_minLineToColorize + increment, _maxLineToColorize);

            ColorizeLineRange(_minLineToColorize, currentProcessingEndLine);

            if (_checkComments)
            {
                ApplyCommentAndStringHighlighting(0, Buffer.LineCount);
                _checkComments = false;
            }

            _minLineToColorize = currentProcessingEndLine;

            if (_minLineToColorize >= _maxLineToColorize)
            {
                ResetColorizationRange();
            }
        }
    }


    private void ApplyCommentAndStringHighlighting(int startLine, int endLine)
    {
        var lines = Buffer.GetLines();
        if (lines.Count == 0)
        {
            return;
        }

        var inMultiLineComment = false;
        var inString = false;
        var stringDelimiter = new Rune('\0');


        if (startLine > 0)
        {
        }

        for (var lineIndex = startLine; lineIndex < Math.Min(endLine, lines.Count); lineIndex++)
        {
            var line = lines[lineIndex];
            var currentLineIsSingleLineComment = false;


            for (var i = 0; i < line.Count; i++)
            {
                var glyph = line[i];
                glyph.Comment = false;
                line[i] = glyph;
            }

            for (var charIndex = 0; charIndex < line.Count; charIndex++)
            {
                var currentGlyph = line[charIndex];
                var character = currentGlyph.Rune;

                if (!inMultiLineComment && (character.Value == '"' || character.Value == '\''))
                {
                    if (!inString)
                    {
                        inString = true;
                        stringDelimiter = character;
                    }
                    else if (character == stringDelimiter)
                    {
                        if (charIndex + 1 < line.Count && line[charIndex + 1].Rune == stringDelimiter)
                        {
                            charIndex++;
                            currentGlyph = line[charIndex];
                        }
                        else
                        {
                            inString = false;
                            stringDelimiter = new Rune('\0');
                        }
                    }
                }
                else if (inString && character.Value == '\\' && charIndex + 1 < line.Count)
                {
                    charIndex++;
                    currentGlyph = line[charIndex];
                }

                if (!inString)
                {
                    if (!inMultiLineComment && editor.Language.CommentStart.Length > 0 &&
                        charIndex + editor.Language.CommentStart.Length <= line.Count &&
                        CompareStringWithGlyphs(editor.Language.CommentStart, line, charIndex))
                    {
                        inMultiLineComment = true;
                        charIndex += editor.Language.CommentStart.Length - 1;
                        continue;
                    }
                    else if (inMultiLineComment && editor.Language.CommentEnd.Length > 0 &&
                             charIndex + editor.Language.CommentEnd.Length <= line.Count &&
                             CompareStringWithGlyphs(editor.Language.CommentEnd, line, charIndex))
                    {
                        inMultiLineComment = false;
                        charIndex += editor.Language.CommentEnd.Length - 1;
                        currentGlyph.Comment = true;
                        line[charIndex] = currentGlyph;
                        continue;
                    }

                    if (!currentLineIsSingleLineComment && editor.Language.SingleLineComment.Length > 0 &&
                        charIndex + editor.Language.SingleLineComment.Length <= line.Count &&
                        CompareStringWithGlyphs(editor.Language.SingleLineComment, line, charIndex))
                    {
                        currentLineIsSingleLineComment = true;
                    }
                }

                if (inMultiLineComment || currentLineIsSingleLineComment)
                {
                    currentGlyph.Comment = true;
                }

                line[charIndex] = currentGlyph;
            }
        }
    }

    private bool CompareStringWithGlyphs(string searchString, Line line, int startIndex)
    {
        var runes = searchString.EnumerateRunes().ToArray();

        if (startIndex + runes.Length > line.Count)
        {
            return false;
        }

        for (var i = 0; i < runes.Length; i++)
        {
            if (runes[i].Value != line[startIndex + i].Rune.Value)
            {
                return false;
            }
        }

        return true;
    }


    public void ResetColorizationRange()
    {
        _minLineToColorize = int.MaxValue;
        _maxLineToColorize = int.MinValue;
        _checkComments = false;
    }


    public uint GetGlyphColor(Glyph glyph)
    {
        if (!Enabled)
        {
            return Palette.Default.GetU32();
        }

        if (glyph.Comment)
        {
            return Palette.Comment.GetU32();
        }

        return Palette[glyph.Color].GetU32();
    }

    public void SetEnabled(bool value)
    {
        Enabled = value;
    }
}
