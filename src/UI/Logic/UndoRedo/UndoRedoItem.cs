using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Logic.UndoRedo;

public class UndoRedoItem
{
    public string Description { get; set; }
    public SubtitleLineViewModel[] Subtitles { get; set; }
    public string SubtitleFileName { get; set; }
    public int[] SelectedLines { get; set; }
    public int CaretIndex { get; set; }
    public int SelectionLength { get; set; }
    public DateTime Created { get; set; }

    public UndoRedoItem(
        string description, 
        SubtitleLineViewModel[] subtitles, 
        string? subtitleFileName, 
        int[] selectedLines, 
        int caretIndex, 
        int selectionLength)
    {
        Description = description;
        Subtitles = subtitles;
        SubtitleFileName = subtitleFileName ?? string.Empty;
        SelectedLines = selectedLines;
        CaretIndex = caretIndex;
        SelectionLength = selectionLength;
        Created = DateTime.Now;
    }
}
