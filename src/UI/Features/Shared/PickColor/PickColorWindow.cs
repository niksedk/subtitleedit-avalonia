using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickColor;

public class PickColorWindow : Window
{

    private readonly PickColorViewModel _vm;

    public PickColorWindow(PickColorViewModel vm)
    {
        UiUtil.InitializeWindow(this);
        Title = Se.Language.Tools.ColorPickerTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;   
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var colorView = new ColorView
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10),
        };
        vm.ColorView = colorView;
        colorView.Bind(ColorView.ColorProperty, new Binding
        {
            Path = nameof(vm.SelectedColor),
        });

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(colorView, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
