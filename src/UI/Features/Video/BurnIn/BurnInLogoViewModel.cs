using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInLogoViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BurnInEffectItem> _effects;

    [ObservableProperty] private ObservableCollection<BurnInEffectItem> _selectedEffects;

    public Window? Window { get; set; }
    public BurnInLogo BurnInLogo { get; set; }
    public string VideoFileName { get; set; }
    public int VideoWidth { get; set; }
    public int VideoHeight { get; set; }
    public bool OkPressed { get; private set; }
    public VideoPlayerControl? VideoPlayerControl { get; set; }
    public double PositionInSeconds { get; set; }
    public List<CheckBox> CheckBoxes { get; set; }

    public BurnInLogoViewModel()
    {
        BurnInLogo = new BurnInLogo();
        VideoFileName = string.Empty;
        Effects = new ObservableCollection<BurnInEffectItem>(BurnInEffectItem.List());
        SelectedEffects = new ObservableCollection<BurnInEffectItem>();
        CheckBoxes = new List<CheckBox>();
    }

    [RelayCommand]
    private void Ok()
    {
        PositionInSeconds = VideoPlayerControl?.Position ?? 0;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void AddSelectedEffect(BurnInEffectItem effect)
    {
        if (!SelectedEffects.Contains(effect))
        {
            SelectedEffects.Add(effect);
        }
    }

    public void RemoveSelectedEffect(BurnInEffectItem effect)
    {
        SelectedEffects.Remove(effect);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void OnLoaded()
    {
    }

    internal void Initialize(string videoFileName, int videoWidth, int videoHeight)
    {
        VideoFileName = videoFileName;
        VideoWidth = videoWidth;
        VideoHeight = videoHeight;
    }
}