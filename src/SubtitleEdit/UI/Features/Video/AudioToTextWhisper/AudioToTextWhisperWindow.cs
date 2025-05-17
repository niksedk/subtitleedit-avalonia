using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class AudioToTextWhisperWindow : Window
{
    private StackPanel _contentPanel;
    private AudioToTextWhisperViewModel _vm;
    
    public AudioToTextWhisperWindow(AudioToTextWhisperViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Audio to text - Whisper";
        Width = 300;
        Height = 160;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var languagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                new Label
                {
                    Content = "Language",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new ComboBox
                {
                    ItemsSource = vm.Languages,
                    SelectedValue = vm.SelectedLanguage,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10),
            Children =
            {
                UiUtil.MakeButton("OK",  vm.OkCommand),
                UiUtil.MakeButton("Cancel",  vm.CancelCommand),
            }
        };
        
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = new Thickness(10),
            Children =
            {
                languagePanel,
                buttonPanel,                
            }
        };

        Content = _contentPanel;
        
        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
