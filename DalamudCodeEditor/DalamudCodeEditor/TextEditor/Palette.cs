using System.Numerics;

namespace DalamudCodeEditor.TextEditor;

public class Palette
{
    private readonly Vector4[] _colors;

    public Vector4 Default
    {
        get => this[PaletteIndex.Default];
    }

    public Vector4 Background
    {
        get => this[PaletteIndex.Background];
    }

    public Vector4 Cursor
    {
        get => this[PaletteIndex.Cursor];
    }

    public Vector4 Selection
    {
        get => this[PaletteIndex.Selection];
    }

    public Vector4 LineNumber
    {
        get => this[PaletteIndex.LineNumber];
    }

    public Vector4 CurrentLineFill
    {
        get => this[PaletteIndex.CurrentLineFill];
    }

    public Vector4 CurrentLineFillInactive
    {
        get => this[PaletteIndex.CurrentLineFillInactive];
    }

    public Vector4 CurrentLineEdge
    {
        get => this[PaletteIndex.CurrentLineEdge];
    }

    public Vector4 Keyword
    {
        get => this[PaletteIndex.Keyword];
    }

    public Vector4 Variable
    {
        get => this[PaletteIndex.Variable];
    }

    public Vector4 Function
    {
        get => this[PaletteIndex.Function];
    }

    public Vector4 Number
    {
        get => this[PaletteIndex.Number];
    }

    public Vector4 String
    {
        get => this[PaletteIndex.String];
    }

    public Vector4 OtherLiteral
    {
        get => this[PaletteIndex.OtherLiteral];
    }

    public Vector4 Operator
    {
        get => this[PaletteIndex.Operator];
    }

    public Vector4 Punctuation
    {
        get => this[PaletteIndex.Punctuation];
    }

    public Vector4 Comment
    {
        get => this[PaletteIndex.Comment];
    }


    public Palette()
    {
        _colors = new Vector4[(int)PaletteIndex.Max];
        SetToDefault();
        EnableHighlighting();
    }

    public Vector4 this[PaletteIndex index]
    {
        get => _colors[(int)index];
        set => _colors[(int)index] = value;
    }

    public void SetToDefault()
    {
        var white = new Vector4(0.9f, 0.9f, 0.9f, 1f);
        var grey = new Vector4(0.3f, 0.3f, 0.3f, 1f);
        var background = new Vector4(0.12f, 0.12f, 0.12f, 1f);


        Set(PaletteIndex.Default, white);

        // UI Elements
        Set(PaletteIndex.Background, background);
        Set(PaletteIndex.Cursor, new Vector4(1f, 1f, 1f, 1f));
        Set(PaletteIndex.Selection, new Vector4(0.25f, 0.35f, 0.5f, 1f));
        Set(PaletteIndex.LineNumber, new Vector4(0.5f, 0.5f, 0.5f, 1f));
        Set(PaletteIndex.CurrentLineFill, new Vector4(0.16f, 0.16f, 0.16f, 1f));
        Set(PaletteIndex.CurrentLineFillInactive, new Vector4(0.14f, 0.14f, 0.14f, 1f));
        Set(PaletteIndex.CurrentLineEdge, new Vector4(0.3f, 0.3f, 0.3f, 1f));

        // Syntax Elements
        Set(PaletteIndex.Keyword, white);
        Set(PaletteIndex.Variable, white);
        Set(PaletteIndex.Function, white);

        Set(PaletteIndex.Number, white);
        Set(PaletteIndex.String, white);
        Set(PaletteIndex.OtherLiteral, white);

        Set(PaletteIndex.Operator, white);
        Set(PaletteIndex.Punctuation, white);
        Set(PaletteIndex.Comment, grey);
    }

    public void EnableHighlighting()
    {
        Set(PaletteIndex.Keyword, new Vector4(0.85f, 0.45f, 0.0f, 1f)); // Orange
        Set(PaletteIndex.Variable, new Vector4(0.9f, 0.9f, 0.7f, 1f)); // Pale yellow
        Set(PaletteIndex.Function, new Vector4(0.4f, 0.7f, 1.0f, 1f)); // Light blue
        Set(PaletteIndex.Number, new Vector4(0.9f, 0.6f, 0.6f, 1f)); // Soft red/pink
        Set(PaletteIndex.String, new Vector4(0.6f, 0.9f, 0.6f, 1f)); // Soft green
        Set(PaletteIndex.OtherLiteral, new Vector4(0.7f, 0.7f, 0.7f, 1f)); // Light grey
        Set(PaletteIndex.Operator, new Vector4(0.8f, 0.8f, 0.8f, 1f)); // Light grey
        Set(PaletteIndex.Punctuation, new Vector4(0.8f, 0.8f, 0.8f, 1f)); // Light grey
        Set(PaletteIndex.Comment, new Vector4(0.5f, 0.5f, 0.5f, 1f)); // Medium grey
    }

    private void Set(PaletteIndex index, Vector4 color)
    {
        _colors[(int)index] = color;
    }
}
