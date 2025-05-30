using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public interface IUndoRedoManager
{
    List<UndoRedoItem> UndoList { get; set; }
    List<UndoRedoItem> RedoList { get; set; }
    void Do(UndoRedoItem action);
    UndoRedoItem? Undo();
    UndoRedoItem? Redo();
    bool CanUndo { get; }
    bool CanRedo { get; }
}