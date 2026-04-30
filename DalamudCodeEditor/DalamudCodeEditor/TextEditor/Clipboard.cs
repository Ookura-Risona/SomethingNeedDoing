using System.Text;
using Dalamud.Bindings.ImGui;

namespace DalamudCodeEditor.TextEditor;

public class Clipboard(Editor editor) : EditorComponent(editor)
{
    public void Copy()
    {
        if (Selection.HasSelection)
        {
            ImGui.SetClipboardText(Selection.Text);
        }
        else
        {
            if (Buffer.GetLines().Count != 0)
            {
                var line = Buffer.GetLines()[Cursor.GetPosition().Line];
                var str = new StringBuilder();
                foreach (var g in line)
                {
                    str.Append(g.Rune);
                }

                ImGui.SetClipboardText(str.ToString());
            }
        }
    }

    public void Cut()
    {
        if (editor.IsReadOnly)
        {
            Copy();
            return;
        }

        if (Selection.HasSelection)
        {
            UndoManager.Create(() =>
            {
                Copy();
                Buffer.DeleteSelection();
            });
        }
    }

    public unsafe void Paste()
    {
        if (editor.IsReadOnly)
        {
            return;
        }

        var clipText = ImGui.GetClipboardText();
        if (!string.IsNullOrEmpty(clipText))
        {
            UndoManager.Create(() =>
            {
                if (Selection.HasSelection)
                {
                    Buffer.DeleteSelection();
                }

                Buffer.InsertText(clipText);
            });
        }
    }
}
