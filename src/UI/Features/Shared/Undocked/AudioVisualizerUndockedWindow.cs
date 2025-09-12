using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class AudioVisualizerUndockedWindow : Window
{
    private readonly AudioVisualizerUndockedViewModel _vm;

    public AudioVisualizerUndockedWindow(AudioVisualizerUndockedViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this);
        Title = Se.Language.General.AudioVisualizer; 
        MinWidth = 400;
        MinHeight = 100;
        Width = 800;
        Height = 200;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        Loaded += vm.Onloaded;
        KeyDown += vm.OnKeyDown;
        Closing += vm.OnClosing;
    }
}
