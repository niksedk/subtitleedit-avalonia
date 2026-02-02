using Avalonia;
using Avalonia.Threading;
using AvaloniaEdit;

namespace Nikse.SubtitleEdit.Logic;

public static class TextEditorExtensions
{
    public static readonly AttachedProperty<int> ScrollToLineProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, int>("ScrollToLine", typeof(TextEditorExtensions), 0);

    static TextEditorExtensions()
    {
        ScrollToLineProperty.Changed.AddClassHandler<TextEditor, int>((x, e) => OnScrollToLineChanged(x, e));
    }

    private static void OnScrollToLineChanged(TextEditor editor, AvaloniaPropertyChangedEventArgs<int> e)
    {
        if (editor != null && e.NewValue.Value > 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (editor.Document != null && e.NewValue.Value > 0 && e.NewValue.Value <= editor.Document.LineCount)
                {
                    var line = editor.Document.GetLineByNumber(e.NewValue.Value);
                    editor.CaretOffset = line.Offset;
                    editor.ScrollToLine(e.NewValue.Value);
                }
            }, DispatcherPriority.Background);
        }
    }

    public static void SetScrollToLine(TextEditor element, int value) => element.SetValue(ScrollToLineProperty, value);
    public static int GetScrollToLine(TextEditor element) => element.GetValue(ScrollToLineProperty);
}
