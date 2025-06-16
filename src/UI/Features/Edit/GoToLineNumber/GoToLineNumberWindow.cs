using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;

public class GoToLineNumberWindow : Window
{
    private readonly GoToLineNumberViewModel _vm;

    public GoToLineNumberWindow(GoToLineNumberViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Go to line number";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        vm.UpDown = new NumericUpDown
        {
            VerticalAlignment = VerticalAlignment.Center,
            Width = 150,
            VerticalContentAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(_vm.LineNumber))
            {
                Mode = BindingMode.TwoWay,
            },
            [!NumericUpDown.MaximumProperty] = new Binding(nameof(_vm.MaxLineNumber))
            {
                Mode = BindingMode.OneWay,
            },
            Minimum = 1,
            Increment = 1,          // Only step in whole numbers
            FormatString = "F0",    // Show 0 decimal places
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                new Label
                {
                    Content = "Go to line number",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                vm.UpDown,
            }
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand), 
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                panel,
                buttonPanel,
            }
        };

        Content = contentPanel;

        Activated += delegate { vm.Activated(); }; 
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}