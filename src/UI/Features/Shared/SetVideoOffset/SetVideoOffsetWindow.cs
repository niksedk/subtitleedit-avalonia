using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

public class SetVideoOffsetWindow : Window
{
    private readonly SetVideoOffset.SetVideoOffsetViewModel _vm;

    public SetVideoOffsetWindow(SetVideoOffset.SetVideoOffsetViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.SetVideoOffset;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDownOffset = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.TimeOffset))
            {
                Mode = BindingMode.TwoWay,
            }
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
                    Content = Se.Language.General.VideoOffset,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                timeCodeUpDownOffset,
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

        Activated += delegate { timeCodeUpDownOffset.Focus(); };
        KeyDown += (sender, args) => vm.OnKeyDown(args);
    }
}