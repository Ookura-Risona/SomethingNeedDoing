namespace DalamudCodeEditor.TextEditor;

public class UndoManager(Editor editor) : EditorComponent(editor)
{
    private List<UndoRecord> buffer = [];

    private int bufferIndex = 0;

    public bool CanUndo()
    {
        return !editor.IsReadOnly && bufferIndex > 0 && buffer.Count > 0;
    }

    public bool CanRedo()
    {
        return !editor.IsReadOnly && bufferIndex < buffer.Count && buffer.Count > 0;
    }

    public void Undo()
    {
        if (CanUndo())
        {
            bufferIndex--;
            buffer[bufferIndex].Undo(editor);
        }
    }

    public void Redo()
    {
        if (CanRedo())
        {
            buffer[bufferIndex].Redo(editor);
            ++bufferIndex;
        }
    }

    public void AddUndo(UndoRecord aValue)
    {
        if (bufferIndex < buffer.Count)
        {
            buffer.RemoveRange(bufferIndex, buffer.Count - bufferIndex);
        }

        buffer.Add(aValue);
        ++bufferIndex;
    }

    public void Clear()
    {
        buffer.Clear();
        bufferIndex = 0;
    }

    public void Create(Action change)
    {
        var record = UndoRecord.Create(editor, change);
        if (record.BeforeText == record.AfterText)
        {
            return;
        }

        AddUndo(record);
    }
}
