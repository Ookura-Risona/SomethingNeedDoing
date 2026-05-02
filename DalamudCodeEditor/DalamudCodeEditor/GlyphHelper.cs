namespace DalamudCodeEditor;

public static class GlyphHelper
{
    public static int GetGlyphDisplayWidth(char c)
    {
        return GetGlyphDisplayWidth(new Glyph(c));
    }

    public static int GetGlyphDisplayWidth(Glyph glyph)
    {
        var codePoint = glyph.Rune.Value;

        if (
            codePoint is >= 0x1100 and <= 0x115F || // Hangul Jamo
            codePoint is >= 0x2E80 and <= 0xA4CF || // CJK
            codePoint is >= 0xAC00 and <= 0xD7A3 || // Hangul
            codePoint is >= 0xF900 and <= 0xFAFF || // Compatibility Ideographs
            codePoint is >= 0xFE10 and <= 0xFE19 || // Vertical forms
            codePoint is >= 0x1F300 and <= 0x1F64F || // Emoji
            codePoint is >= 0x1F900 and <= 0x1F9FF || // Emoji
            codePoint is >= 0x20000 and <= 0x2FFFD || // CJK Extension B-D
            codePoint is >= 0x30000 and <= 0x3FFFD // CJK Extension E-G
        )
        {
            return 2;
        }

        return 1;
    }
}
