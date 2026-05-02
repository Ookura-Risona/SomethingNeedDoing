namespace DalamudCodeEditor;

public readonly struct Token(int start, int end, PaletteIndex color)
{
    public int Start { get; } = start;

    public int End { get; } = end;

    public PaletteIndex Color { get; } = color;
}
