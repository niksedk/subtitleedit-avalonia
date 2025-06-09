using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewDeleteLines
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var header = new Label
        {
            Content = "Delete lines",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var labelDeleteContains = UiUtil.MakeLabel("Delete lines containing text");
        var textBoxDeleteContains = UiUtil.MakeTextBox(400, vm, nameof(vm.DeleteLinesContains));
        var panelDeleteContains = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteContains, textBoxDeleteContains },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };  

        var labelDeleteFirstLines = UiUtil.MakeLabel("Delete first lines");
        var numericUpDownDeleteFirstLines = UiUtil.MakeNumericUpDownInt(0, 100, 150, vm, nameof(vm.DeleteXFirstLines));
        var panelDeleteFirstLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteFirstLines, numericUpDownDeleteFirstLines },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelDeleteLastLines = UiUtil.MakeLabel("Delete last lines");
        var numericUpDownDeleteLastLines = UiUtil.MakeNumericUpDownInt(0, 100, 150, vm, nameof(vm.DeleteXLastLines));
        var panelDeleteLastLines = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Children = { labelDeleteLastLines, numericUpDownDeleteLastLines },
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children = 
            { 
                header,
                panelDeleteContains,
                panelDeleteFirstLines,
                panelDeleteLastLines,
            }
        };

        return panel;
    }
}
