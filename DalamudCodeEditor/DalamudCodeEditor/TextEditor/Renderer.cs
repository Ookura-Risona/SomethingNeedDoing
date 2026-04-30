using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Renderer(Editor editor) : EditorComponent(editor)
{
    public bool IsRendering { get; private set; } = false;

    public float GutterWidth { get; private set; } = 20f;

    public string TextRun = string.Empty;


    public float CharacterWidth
    {
        get => ImGui.CalcTextSize("#").X;
    }

    public float LineHeight
    {
        get => ImGui.GetTextLineHeightWithSpacing() * Style.LineSpacing;
    }

    public Vector2 ContentSize
    {
        get => ImGui.GetWindowContentRegionMax();
    }

    public void SetGutterWidth(float width)
    {
        GutterWidth = width;
    }

    public void Render()
    {
        var contentSize = ImGui.GetWindowContentRegionMax();
        var drawList = ImGui.GetWindowDrawList();
        var longest = GutterWidth + Buffer.GetLongestRenderedLineWidth() + 2; // 2 = padding

        var (selectionStart, selectionEnd) = Selection.GetOrderedPositions();

        Scroll.ScrollToTop();

        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var scrollX = ImGui.GetScrollX();
        var scrollY = ImGui.GetScrollY();

        var firstLine = (int)Math.Floor(scrollY / Renderer.LineHeight);
        var totalLines = Buffer.LineCount;
        var lastLine = Math.Min(totalLines - 1, firstLine + (int)Math.Floor((scrollY + contentSize.Y) / LineHeight));

        var buf = Style.ShowLineNumbers ? $" {totalLines} " : "";
        SetGutterWidth(ImGui.CalcTextSize(buf).X);

        if (totalLines == 0)
        {
            ImGui.Dummy(new Vector2(GutterWidth + 2, LineHeight));
            return;
        }

        var spaceWidth = ImGui.CalcTextSize(" ").X;

        for (var lineNo = firstLine; lineNo <= lastLine; lineNo++)
        {
            var lineStart = new Vector2(cursorScreenPos.X, cursorScreenPos.Y + lineNo * LineHeight);
            var textStart = new Vector2(lineStart.X + GutterWidth, lineStart.Y);

            var line = Buffer.GetLine(lineNo);

            // Draw selection
            var selectionStartX = -1f;
            var selectionEndX = -1f;

            var lineStartCoord = new Coordinate(lineNo, 0);
            var lineEndCoord = new Coordinate(lineNo, Buffer.GetLineMaxColumn(lineNo));

            if (selectionStart <= lineEndCoord)
            {
                selectionStartX = selectionStart > lineStartCoord
                    ? Buffer.TextDistanceToLineStart(selectionStart)
                    : 0f;
            }

            if (selectionEnd > lineStartCoord)
            {
                selectionEndX = Buffer.TextDistanceToLineStart(selectionEnd < lineEndCoord
                    ? selectionEnd
                    : lineEndCoord);
            }

            if (selectionEnd.Line > lineNo)
            {
                selectionEndX += CharacterWidth;
            }

            if (selectionStartX >= 0 && selectionEndX >= 0 && selectionStartX < selectionEndX)
            {
                var vstart = new Vector2(lineStart.X + GutterWidth + selectionStartX, lineStart.Y);
                var vend = new Vector2(lineStart.X + GutterWidth + selectionEndX, lineStart.Y + LineHeight);
                drawList.AddRectFilled(vstart, vend, Palette.Selection.GetU32());
            }

            // Draw line number
            if (Style.ShowLineNumbers)
            {
                buf = $"{lineNo + 1}  ";
                var numWidth = ImGui.CalcTextSize(buf).X;
                drawList.AddText(
                    new Vector2(lineStart.X + GutterWidth - numWidth, lineStart.Y),
                    Palette.LineNumber.GetU32(),
                    buf);
            }

            // Draw current line highlight & cursor
            if (State.CursorPosition.Line == lineNo && ImGui.IsWindowFocused())
            {
                if (!Selection.HasSelection)
                {
                    var lineEnd = new Vector2(lineStart.X + contentSize.X + scrollX, lineStart.Y + LineHeight);
                    drawList.AddRectFilled(lineStart, lineEnd,
                        Palette[PaletteIndex.CurrentLineFill].GetU32());
                    drawList.AddRect(lineStart, lineEnd, Palette.CurrentLineEdge.GetU32(), 1f);
                }

                var elapsed = DateTime.Now.Ticks - editor.StartTime;
                if (elapsed > 400)
                {
                    var cx = Buffer.TextDistanceToLineStart(State.CursorPosition);
                    var cursorStart = lineStart with { X = textStart.X + cx };
                    var cursorEnd = new Vector2(cursorStart.X + 1f, lineStart.Y + LineHeight);
                    drawList.AddRectFilled(cursorStart, cursorEnd, Palette[PaletteIndex.Cursor].GetU32());

                    if (elapsed > 800)
                    {
                        editor.StartTime = DateTime.Now.Ticks;
                    }
                }
            }

            // Draw glyphs
            var prevColor = Palette[PaletteIndex.Default].GetU32();
            var offset = Vector2.Zero;
            var run = "";
            for (var i = 0; i < line.Count;)
            {
                var glyph = line[i];
                var rune = glyph.Rune;
                var color = Colorizer.GetGlyphColor(glyph);

                if ((color != prevColor || rune.Value == '\t' || rune.Value == ' ') && run.Length > 0)
                {
                    drawList.AddText(textStart + offset, prevColor, run);
                    offset.X += ImGui.CalcTextSize(run).X;
                    run = "";
                }

                prevColor = color;

                if (rune.Value == '\t')
                {
                    var oldX = offset.X;
                    var tabWidth = Style.TabSize * spaceWidth;
                    offset.X = (float)(1.0 + Math.Floor((1.0 + offset.X) / tabWidth)) * tabWidth;
                    i++;

                    if (Style.ShowWhitespace)
                    {
                        var s = ImGui.GetFontSize();
                        var x1 = textStart.X + oldX + 1.0f;
                        var x2 = textStart.X + offset.X - 1.0f;
                        var y = textStart.Y + s * 0.5f;
                        drawList.AddLine(new Vector2(x1, y), new Vector2(x2, y), 0x90909090);
                        drawList.AddLine(new Vector2(x2, y), new Vector2(x2 - s * 0.2f, y - s * 0.2f), 0x90909090);
                        drawList.AddLine(new Vector2(x2, y), new Vector2(x2 - s * 0.2f, y + s * 0.2f), 0x90909090);
                    }
                }
                else if (rune.Value == ' ')
                {
                    if (Style.ShowWhitespace)
                    {
                        var s = ImGui.GetFontSize();
                        var x = textStart.X + offset.X + spaceWidth * 0.5f;
                        var y = textStart.Y + s * 0.5f;
                        drawList.AddCircleFilled(new Vector2(x, y), 1.5f, 0x80808080, 4);
                    }

                    offset.X += spaceWidth;
                    i++;
                }
                else
                {
                    run += rune.ToString();
                    i++;
                }
            }

            if (run.Length > 0)
            {
                drawList.AddText(textStart + offset, prevColor, run);
            }
        }

        ImGui.Dummy(new Vector2(longest, totalLines * LineHeight));
    }

    public void Start()
    {
        IsRendering = true;
    }

    public void End()
    {
        IsRendering = false;
    }

    public int GetPageSize()
    {
        return (int)Math.Floor((ImGui.GetWindowHeight() - 20.0f) / LineHeight);
    }
}
