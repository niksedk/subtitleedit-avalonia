using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAdjustDuration
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var header = new Label
        {
            Content = "Adjust durations",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var checkBoxRemoveAll = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveAll));
        var checkBoxRemoveItalic = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveItalic));
        var checkBoxRemoveBold = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveBold));
        var checkBoxRemoveUnderline = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveUnderline));
        var checkBoxRemoveFontTags = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveFontTags));
        var checkBoxRemoveAlignmentTags = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveAlignmentTags));
        var checkBoxRemoveColors = UiUtil.MakeCheckBox(vm, nameof(vm.FormattingRemoveColors));

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                header,
                checkBoxRemoveAll,
                checkBoxRemoveItalic,
                checkBoxRemoveBold,
                checkBoxRemoveUnderline,
                checkBoxRemoveFontTags,
                checkBoxRemoveAlignmentTags,
                checkBoxRemoveColors,
            }
        };

        return panel;
    }
}
