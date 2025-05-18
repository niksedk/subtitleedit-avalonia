using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class DownloadWhisperEngineWindow : Window
{
    private readonly DownloadWhisperEngineViewModel _vm;

    public DownloadWhisperEngineWindow(DownloadWhisperEngineViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "Downloading Whisper engine";
        Width = 400;
        Height = 190;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = "Downloading Whisper engine",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
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
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.Progress)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

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

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        }; 
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}