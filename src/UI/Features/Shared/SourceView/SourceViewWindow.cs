using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public class SourceViewWindow : Window
{
    private readonly SourceViewViewModel _vm;
    
    public SourceViewWindow(SourceViewViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        Width = 1000;
        Height = 700;   
        MinWidth = 700;
        MinHeight = 400;
        CanResize = true;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var textBox = new TextBox
        {
            Margin = new Thickness(0, 0, 10, 0),
            [!TextBox.TextProperty] = new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        };
        vm.SourceViewTextBox = textBox;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);   
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var labelCursorPosition = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.LineAndColumnInfo)).WithAlignmentTop();

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(textBox, 0);
        grid.Add(labelCursorPosition, 1);
        grid.Add(buttonPanel, 1);

        Content = grid;
        
        Activated += delegate { textBox.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
