using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public abstract class DirtyTrackable(Editor editor) : EditorComponent(editor)
{
    private bool isDirty = false;

    private double lastDirtyTime = 0;

    public double TimeSinceDirty
    {
        get => isDirty ? ImGui.GetTime() - lastDirtyTime : 0;
    }

    public bool IsDirty
    {
        get => isDirty;
    }

    public void MarkClean()
    {
        isDirty = false;
    }

    public void MarkDirty()
    {
        isDirty = true;
        lastDirtyTime = ImGui.GetTime();
    }
}
