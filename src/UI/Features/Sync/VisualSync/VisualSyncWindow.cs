using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls.AudioVisualizerControl;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public class VisualSyncWindow : Window
{
    private readonly VisualSyncViewModel _vm;
    
    public VisualSyncWindow(VisualSyncViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.Sync.VisualSync;
        CanResize = true;
        Width = 1000;
        Height = 700;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelVideoInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoInfo));
        var panelVideo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                labelVideoInfo
            }
        };

        vm.VideoPlayerControlLeft = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlLeft.FullScreenIsVisible = false;
        vm.VideoPlayerControlRight = InitVideoPlayer.MakeVideoPlayer();
        vm.VideoPlayerControlRight.FullScreenIsVisible = false;

        vm.AudioVisualizerLeft = new AudioVisualizer { Height = 80, Width = double.NaN };
        vm.AudioVisualizerRight = new AudioVisualizer{ Height = 80, Width = double.NaN };

        var comboBoxLeft = UiUtil.MakeComboBox(vm.Paragraphs, vm, nameof(vm.SelectedParagraphLeft));
        var comboBoxRight = UiUtil.MakeComboBox(vm.Paragraphs, vm, nameof(vm.SelectedParagraphRight));

        var buttonSync = UiUtil.MakeButton(Se.Language.Sync.Sync, vm.SyncCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);   
        var buttonPanel = UiUtil.MakeButtonBar(buttonSync, buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelVideo, 0, 0, 1, 2);
        grid.Add(vm.VideoPlayerControlLeft, 1);
        grid.Add(vm.VideoPlayerControlRight, 1,1);
        grid.Add(vm.AudioVisualizerLeft, 2);
        grid.Add(vm.AudioVisualizerRight, 2, 2);
        grid.Add(comboBoxRight, 3);
        grid.Add(comboBoxLeft, 3,1);
        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
