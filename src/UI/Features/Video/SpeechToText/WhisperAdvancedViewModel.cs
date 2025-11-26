using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class WhisperAdvancedViewModel : ObservableObject
{
    [ObservableProperty] private string _parameters;
    [ObservableProperty] private string _helpText;
    [ObservableProperty] private bool _isVadCppEnabled;

    public Window? Window { get; set; }
    public List<IWhisperEngine> Engines { get; set; }

    public bool OkPressed { get; private set; }

    public WhisperAdvancedViewModel(IWindowService windowService)
    {
        Parameters = string.Empty;
        HelpText = string.Empty;
        Engines = new List<IWhisperEngine>();
    }

    public void RefreshVadCpp(IWhisperEngine engine)
    {
        IsVadCppEnabled = engine.Name == WhisperEngineCpp.StaticName;
    }

    [RelayCommand]
    private async Task EngineClicked(IWhisperEngine engine)
    {
        var helpText = await engine.GetHelpText();
        HelpText = engine.Name + Environment.NewLine + Environment.NewLine + helpText;
        RefreshVadCpp(engine);
    }

    [RelayCommand]
    private void EnableVadCpp()
    {
        var fileName = GetVadCppFile();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        Parameters = $"--vad --vad-model '{fileName}'";
    }

    private static string? GetVadCppFile()
    {
        var searchPaths = new List<string>
        {
            Path.Combine(Se.WhisperFolder, "Cpp", "Models"),
            Path.Combine(Se.WhisperFolder, "Cpp", "models"),
            Path.Combine(Se.WhisperFolder, "Cpp"),
        };

        foreach (var searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                continue;
            }

            var files = Directory.GetFiles(searchPath, "ggml-silero-v*.bin", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return files[0];
            }
        }

        return null;
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