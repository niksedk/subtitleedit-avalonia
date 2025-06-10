using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeCasing
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = "Change casing",
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

         var checkBoxNormalCasingOnlyUpper = new CheckBox
        {
            Content = "Only fix all uppercase lines)",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 15),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.NormalCasingOnlyUpper)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxFixNamesOnly = new RadioButton
        {
            Content = "Fix names only",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FixNamesOnly)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxAllUppercase = new RadioButton
        {
            Content = "All uppercase",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AllUppercase)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxAllLowercase = new RadioButton
        {
            Content = "All lowercase",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AllLowercase)) { Mode = BindingMode.TwoWay },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        };

        grid.Add(labelHeader, 0, 0);
        grid.Add(checkBoxNormalCasing, 1, 0);
        grid.Add(checkBoxNormalCasingFixNames, 2, 0);
        grid.Add(checkBoxNormalCasingOnlyUpper, 3, 0);
        grid.Add(checkBoxFixNamesOnly, 4, 0);
        grid.Add(checkBoxAllUppercase, 5, 0);
        grid.Add(checkBoxAllLowercase, 6, 0);
        
        return grid;
    }
}
