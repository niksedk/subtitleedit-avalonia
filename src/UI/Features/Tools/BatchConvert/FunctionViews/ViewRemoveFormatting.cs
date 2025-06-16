using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewRemoveFormatting
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = "Remove formatting",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var checkBoxRemoveAll = UiUtil.MakeCheckBox("Remove all", vm, nameof(vm.FormattingRemoveAll));
        var checkBoxRemoveItalic = UiUtil.MakeCheckBox("Remove italic", vm, nameof(vm.FormattingRemoveItalic));
        var checkBoxRemoveBold = UiUtil.MakeCheckBox("Remove bold", vm, nameof(vm.FormattingRemoveBold));
        var checkBoxRemoveUnderline = UiUtil.MakeCheckBox("Remove underline", vm, nameof(vm.FormattingRemoveUnderline));
        var checkBoxRemoveFontTags = UiUtil.MakeCheckBox("Remove font tags", vm, nameof(vm.FormattingRemoveFontTags));
        var checkBoxRemoveAlignmentTags = UiUtil.MakeCheckBox("Remove alignment tags", vm, nameof(vm.FormattingRemoveAlignmentTags));
        var checkBoxRemoveColors = UiUtil.MakeCheckBox("Remove color tags", vm, nameof(vm.FormattingRemoveColors));

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                labelHeader,
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
