using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Common;

public class DownloadFfmpegWindow : Window
{
    public DownloadFfmpegWindow(DownloadFfmpegViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Downloading ffmpeg";
        Width = 400;
        Height = 250;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = "Downloading ffmpeg",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.Progress)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var buttonCancel = UiUtil.MakeButton("Cancel", vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar( buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(20),
            Children =
            {
                titleText,
                progressSlider,
                statusText,
                buttonBar,
            }
        };

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}