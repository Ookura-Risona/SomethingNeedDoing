using Dalamud.Bindings.ImGui;
using DalamudCodeEditor.TextEditor;

public readonly struct KeyBinding
{
    public ImGuiKey Key { get; }

    public KeyState Ctrl { get; }

    public KeyState Shift { get; }

    public KeyState Alt { get; }

    public KeyBinding(ImGuiKey key, KeyState ctrl = KeyState.Up, KeyState shift = KeyState.Up, KeyState alt = KeyState.Up)
    {
        Key = key;
        Ctrl = ctrl;
        Shift = shift;
        Alt = alt;
    }

    public KeyBinding WithCtrl(KeyState state)
    {
        return new KeyBinding(Key, state, Shift, Alt);
    }

    public KeyBinding WithShift(KeyState state)
    {
        return new KeyBinding(Key, Ctrl, state, Alt);
    }

    public KeyBinding WithAlt(KeyState state)
    {
        return new KeyBinding(Key, Ctrl, Shift, state);
    }

    public KeyBinding CtrlDown()
    {
        return WithCtrl(KeyState.Down);
    }

    public KeyBinding CtrlIgnored()
    {
        return WithCtrl(KeyState.Ignored);
    }

    public KeyBinding ShiftDown()
    {
        return WithShift(KeyState.Down);
    }

    public KeyBinding ShiftIgnored()
    {
        return WithShift(KeyState.Ignored);
    }

    public KeyBinding AltDown()
    {
        return WithAlt(KeyState.Down);
    }

    public KeyBinding AltIgnored()
    {
        return WithAlt(KeyState.Ignored);
    }

    public bool Matches(bool ctrl, bool shift, bool alt)
    {
        return MatchesKeyState(Ctrl, ctrl) &&
               MatchesKeyState(Shift, shift) &&
               MatchesKeyState(Alt, alt);
    }

    private static bool MatchesKeyState(KeyState bindingState, bool actualPressed)
    {
        return bindingState switch
        {
            KeyState.Ignored => true,
            KeyState.Down => actualPressed,
            KeyState.Up => !actualPressed,
            _ => false,
        };
    }
}
