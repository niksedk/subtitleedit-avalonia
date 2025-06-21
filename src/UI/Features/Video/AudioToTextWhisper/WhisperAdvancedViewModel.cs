using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public partial class WhisperAdvancedViewModel : ObservableObject
{
    [ObservableProperty] private string _parameters;
    [ObservableProperty] private string _helpText;

    public Window? Window { get; set; }
    public List<IWhisperEngine> Engines { get; set; }

    public bool OkPressed { get; private set; }

    public WhisperAdvancedViewModel(IWindowService windowService)
    {
        Parameters = string.Empty;
        HelpText = string.Empty;
        Engines = new List<IWhisperEngine>();
    }

    [RelayCommand]
    private async Task EngineClicked(IWhisperEngine engine)
    {
        var helpText = await engine.GetHelpText();
        HelpText = engine.Name + Environment.NewLine + Environment.NewLine + helpText;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}