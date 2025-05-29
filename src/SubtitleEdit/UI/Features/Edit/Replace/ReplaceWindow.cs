using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

public class ReplaceWindow : Window
{
    private ReplaceViewModel _vm;

    public ReplaceWindow(ReplaceViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Replace";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var textBoxFind = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            Margin = new Thickness(0, 0, 0, 3),
            Watermark = "Search text...",
            [!TextBox.TextProperty] = new Binding(nameof(vm.SearchText)) { Mode = BindingMode.TwoWay }
        };

        var checkBoxWholeWord = new CheckBox
        {
            Content = "Whole word",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.WholeWord)) { Mode = BindingMode.TwoWay }
        };

        var panelFind = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                textBoxFind,
                checkBoxWholeWord
            }
        };

        var labelReplaceWith = new TextBlock
        {
            Text = "Replace with:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 3)
        };

        var textBoxReplace = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
            Margin = new Thickness(0, 0, 0, 3),
            Watermark = "Replace text...",
            [!TextBox.TextProperty] = new Binding(nameof(vm.ReplaceText)) { Mode = BindingMode.TwoWay }
        };

        var panelReplace = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                labelReplaceWith,
                textBoxReplace
            }
        };

        var radioButtonNormal = new RadioButton
        {
            Content = "Normal",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeNormal)) { Mode = BindingMode.TwoWay }
        };

        var radioButtonCaseInsensitive = new RadioButton
        {
            Content = "Case insensitive",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeCanseInsensitive)) { Mode = BindingMode.TwoWay }
        };

        var radioButtonRegularExpression = new RadioButton
        {
            Content = "Regular expression",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 3),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FindTypeRegularExpression)) { Mode = BindingMode.TwoWay }
        };

        var panelFindTypes = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Children =
            {
                radioButtonNormal,
                radioButtonCaseInsensitive,
                radioButtonRegularExpression
            }
        };

        var buttonFindNext = UiUtil.MakeButton("Find Next", vm.FindNextCommand)
            .WithLeftAlignment()
            .WithMinwidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonReplaceNext = UiUtil.MakeButton("Replace next", vm.ReplaceCommand)
            .WithLeftAlignment()
            .WithMinwidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonReplaceAll = UiUtil.MakeButton("Replace all", vm.ReplaceAllCommand)
            .WithLeftAlignment()
            .WithMinwidth(150)
            .WithMargin(0, 0, 0, 10);
        var buttonCount = UiUtil.MakeButton("Count", vm.CountCommand)
            .WithLeftAlignment()
            .WithMinwidth(150)
            .WithMargin(0, 0, 0, 10);

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 50, 0),
            Children =
            {
                buttonFindNext,
                buttonReplaceNext,
                buttonReplaceAll,
                buttonCount
            }
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelFind, 0, 0);
        grid.Add(panelReplace, 1, 0);
        grid.Add(panelFindTypes, 2, 0);
        grid.Add(panelButtons, 0, 1, 3, 1);

        Content = grid;

        Activated += delegate { textBoxFind.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
