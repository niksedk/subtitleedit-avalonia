using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewMultipleReplace
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = "Multiple replace",
            VerticalAlignment = VerticalAlignment.Center,
        };
        
         var checkBoxNormalCasing = new RadioButton
        {
            Content = "Normal casing",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.NormalCasing)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxNormalCasingFixNames = new CheckBox
        {
            Content = "Fix names",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 5),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.NormalCasingFixNames)) { Mode = BindingMode.TwoWay },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0);
        grid.Add(checkBoxNormalCasing, 1, 0);
        grid.Add(checkBoxNormalCasingFixNames, 2, 0);

        return grid;
    }
}
