using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class VideoPlayerUndockedWindow : Window
{
    private readonly VideoPlayerUndockedViewModel _vm;

    public VideoPlayerUndockedWindow(VideoPlayerUndockedViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.VideoPlayer;
        MinWidth = 400;
        MinHeight = 200;
        Width = 800;
        Height = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        Loaded += vm.Onloaded;
        KeyDown += vm.OnKeyDown;
        Closing += vm.OnClosing;
    }
}
