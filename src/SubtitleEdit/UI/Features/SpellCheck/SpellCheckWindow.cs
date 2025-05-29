using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckWindow : Window
{
    private SpellCheckViewModel _vm;
    
    public SpellCheckWindow(SpellCheckViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Spell check";
        Width = 900;
        Height = 740;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelLine = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!Label.ContentProperty] = new Binding(nameof(SpellCheckViewModel.LineText), BindingMode.OneWay)
        };

        var panelWholeText = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Children =
            {
                new TextBox
                {
                    [!TextBox.TextProperty] = new Binding(nameof(SpellCheckViewModel.WholeText), BindingMode.TwoWay),
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 400,
                    MinHeight = 30
                }
            }
        };

        var labelWordNotFound = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = "Word not found:",
        };

        var textBoxWord = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(SpellCheckViewModel.Word), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Center,
            Width = double.NaN,
        };

        var panelButtons = MakeButtons(vm);

        var panelSuggestions = MakeSuggestions(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);

       
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelLine, 0, 0);
        grid.Add(panelWholeText, 1, 0);
        grid.Add(labelWordNotFound, 2, 0);
        grid.Add(textBoxWord, 3, 0);
        grid.Add(panelButtons, 4, 0);

        grid.Add(panelSuggestions, 0, 1, 1, 6);

        Content = grid;
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    private Grid MakeButtons(SpellCheckViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };

        return grid;
    }

    private Grid MakeSuggestions(SpellCheckViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
