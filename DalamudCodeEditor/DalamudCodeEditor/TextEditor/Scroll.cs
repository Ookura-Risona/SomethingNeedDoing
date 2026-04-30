using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Scroll(Editor editor) : EditorComponent(editor)
{
    public bool ShouldScrollToCursor { get; private set; } = false;

    public bool ShouldScrollToTop { get; private set; } = false;

    public void RequestScrollToCursor()
    {
        ShouldScrollToCursor = true;
    }

    public void ScrollToCursor()
    {
        if (!ShouldScrollToCursor)
        {
            return;
        }

        Cursor.EnsureVisible();
        ImGui.SetWindowFocus();
        ShouldScrollToCursor = false;
    }

    public void RequestScrollToTop()
    {
        ShouldScrollToTop = true;
    }

    public void ScrollToTop()
    {
        if (!ShouldScrollToTop)
        {
            return;
        }

        ShouldScrollToTop = false;
        ImGui.SetScrollY(0f);
    }
}
