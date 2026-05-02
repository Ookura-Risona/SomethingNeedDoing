using System.Text;

namespace DalamudCodeEditor;

public struct Glyph(Rune rune, PaletteIndex color = PaletteIndex.Default)
{
    public readonly Rune Rune = rune;

    public PaletteIndex Color = color;

    public bool Comment = false;

    public Glyph(char character, PaletteIndex color = PaletteIndex.Default) : this(new Rune(character), color)
    {
    }

    public override string ToString()
    {
        return Rune.ToString();
    }

    public bool IsLetter()
    {
        // Include underscores to group words
        return Rune.IsLetter(rune) || rune.Value == '_';
    }

    public bool IsNumber()
    {
        return Rune.IsNumber(rune);
    }

    public bool IsWhiteSpace()
    {
        return char.IsWhiteSpace((char)Rune.Value);
    }

    public bool IsSpecial()
    {
        return !IsLetter() && !IsNumber() && !IsWhiteSpace();
    }

    public bool IsTab()
    {
        return Rune.Value == '\t';
    }

    public bool IsGroupable(Glyph glyph)
    {
        if (IsLetter())
        {
            return glyph.IsLetter();
        }

        if (IsNumber())
        {
            return glyph.IsNumber();
        }

        return Rune.Value switch
        {
            (int)')' => glyph.Rune.Value == ')' || glyph.Rune.Value == '(',
            (int)']' => glyph.Rune.Value == ']' || glyph.Rune.Value == '{',
            (int)'}' => glyph.Rune.Value == '}' || glyph.Rune.Value == '[',
            _ => Rune.Value == glyph.Rune.Value,
        };
    }
}
