using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

public class ShowImageWindow : Window
{
    private readonly ShowImageViewModel _vm;
    
    public ShowImageWindow(ShowImageViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        Width = 1000;
        Height = 760;
        MinWidth = 400;
        MinHeight = 300;

        CanResize = true;
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
        };

        var flyout = new MenuFlyout();
        image.ContextFlyout = flyout;

        var menuItemDelete = new MenuItem
        {
            Header = Se.Language.General.CopyImageToClipboard,
            DataContext = vm,
            Command = vm.CopyImageToClipboardCommand,
        };
        flyout.Items.Add(menuItemDelete);

        var menuItemClear = new MenuItem
        {
            Header = Se.Language.General.SaveImageAsDotDotDot,
            DataContext = vm,
            Command = vm.SaveImageAsCommand,
        };
        flyout.Items.Add(menuItemClear);


        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk);
        
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
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(image, 0);
        grid.Add(buttonPanel, 1);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
