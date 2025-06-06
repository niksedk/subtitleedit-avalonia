using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public class ChangeCasingWindow : Window
{
    private ChangeCasingViewModel _vm;
    
    public ChangeCasingWindow(ChangeCasingViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.Tools.ChangeCasing.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300; 
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

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

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);   
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );
        
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
            Margin = UiUtil.MakeWindowMargin(),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxNormalCasing, 0, 0);
        grid.Add(checkBoxNormalCasingFixNames, 1, 0);
        grid.Add(checkBoxNormalCasingOnlyUpper, 2, 0);
        grid.Add(checkBoxFixNamesOnly, 3, 0);
        grid.Add(checkBoxAllUppercase, 4, 0);
        grid.Add(checkBoxAllLowercase, 5, 0);
        grid.Add(buttonPanel, 6, 0);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
