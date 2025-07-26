using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ImageBasedPreviewWindow : Window
{
    private readonly ImageBasedPreviewViewModel _vm;

    public ImageBasedPreviewWindow(ImageBasedPreviewViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)) { Mode = BindingMode.TwoWay });
        Width = 1060;
        Height = 700;
        CanResize = true;
        MinWidth = 720;
        MinHeight = 480;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.BitmapPreview)) { Mode = BindingMode.TwoWay },
        };
        vm.ImagePreview = image;

        Content = image;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}