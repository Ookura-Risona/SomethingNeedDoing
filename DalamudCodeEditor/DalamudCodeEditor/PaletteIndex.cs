namespace DalamudCodeEditor;

/// <summary>
///     Represents the type of token or editor element being colorized.
///     The order matters for indexing into the highlight palette.
/// </summary>
public enum PaletteIndex
{
    Default = 0,

    Background,

    Cursor,

    Selection,

    LineNumber,

    CurrentLineFill,

    CurrentLineFillInactive,

    CurrentLineEdge,

    Keyword,

    Variable,

    Function,

    Number,

    String,

    OtherLiteral,

    Operator,

    Punctuation,

    Comment,

    Max,
}
