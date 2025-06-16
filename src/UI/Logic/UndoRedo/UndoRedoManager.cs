using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public class UndoRedoManager : IUndoRedoManager
{
    public List<UndoRedoItem> UndoList { get; set; } = new();
    public List<UndoRedoItem> RedoList { get; set; } = new();
    private const int MaxUndoItems = 100;

    public void Do(UndoRedoItem action)
    {
        UndoList.Add(action);
        if (UndoList.Count > MaxUndoItems)
        {
            UndoList.RemoveAt(0);
        }

        RedoList.Clear();
    }

    public UndoRedoItem? Undo()
    {
        if (UndoList.Count > 0)
        {
            var action = UndoList[^1];
            UndoList.RemoveAt(UndoList.Count - 1);
            RedoList.Add(action);
            return action;
        }

        return null;
    }

    public UndoRedoItem? Redo()
    {
        if (RedoList.Count > 0)
        {
            var action = RedoList[^1];
            RedoList.RemoveAt(RedoList.Count - 1);
            UndoList.Add(action);
            return action;
        }

        return null;
    }

    public bool CanUndo => UndoList.Count > 0;
    public bool CanRedo => RedoList.Count > 0;
}
