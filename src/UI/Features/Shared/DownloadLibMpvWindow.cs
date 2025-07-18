using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared;

public class DownloadLibMpvWindow : Window
{
    private readonly DownloadLibMpvViewModel _vm;

    public DownloadLibMpvWindow(DownloadLibMpvViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "Downloading libmpv";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = "Downloading libmpv",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            MinWidth = 400,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(DownloadLibMpvViewModel.Progress)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(DownloadLibMpvViewModel.StatusText)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                progressSlider,
                statusText,
                buttonBar,
            }
        };

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        }; 
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}