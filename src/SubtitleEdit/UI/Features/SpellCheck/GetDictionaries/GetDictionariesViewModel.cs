using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public partial class GetDictionariesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SpellCheckDictionaryDisplay> _dictionaries;
    [ObservableProperty] private SpellCheckDictionaryDisplay? selectedDictionary;
    [ObservableProperty] private string _description;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isDownloadEnabled;
    [ObservableProperty] private bool _isProgressVisible;

    public GetDictionariesWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly ISpellCheckDictionaryDownloadService _spellCheckDictionaryDownloadService;

    public GetDictionariesViewModel(ISpellCheckDictionaryDownloadService spellCheckDictionaryDownloadService)
    {
        _spellCheckDictionaryDownloadService = spellCheckDictionaryDownloadService;

        Dictionaries = new ObservableCollection<SpellCheckDictionaryDisplay>();
        SelectedDictionary = null;
        Description = string.Empty;
        IsDownloadEnabled = true;
        IsProgressVisible = false;
        StatusText = string.Empty;

        LoadDictionaries();
    }

    private void LoadDictionaries()
    {
        var uri = new Uri("avares://Nikse.SubtitleEdit/Assets/HunspellDictionaries.json");
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);

        var jsonContent = reader.ReadToEndAsync().Result;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var dictionaries = JsonSerializer.Deserialize<List<SpellCheckDictionaryDisplay>>(jsonContent, options);
        foreach (var dictionary in dictionaries ?? new List<SpellCheckDictionaryDisplay>())
        {
            Dictionaries.Add(dictionary);
        }

        var englishName = CultureInfo.CurrentCulture.EnglishName;
        if (englishName.Contains('(') && englishName.Contains(')'))
        {
            var start = englishName.IndexOf('(') + 1;
            var end = englishName.IndexOf(')');
            englishName = englishName.Substring(start, end - start).Trim();
        }

        var selected = Dictionaries.FirstOrDefault(d => d.EnglishName.Contains(englishName, StringComparison.OrdinalIgnoreCase) ||
                                                        d.NativeName.Contains(englishName, StringComparison.OrdinalIgnoreCase));
        if (selected == null)
        {
            if (LanguageHelper.CountryToLanguage.TryGetValue(englishName.ToLower(), out var languageName))
            {
                selected = Dictionaries.FirstOrDefault(d => d.EnglishName.Equals(languageName, StringComparison.OrdinalIgnoreCase) ||
                                                            d.NativeName.Equals(languageName, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (selected != null)
        {
            SelectedDictionary = selected;
            Description = selected.Description;
        }
        else
        {
            SelectedDictionary = Dictionaries.FirstOrDefault();
            Description = string.Empty;
        }
    }

    [RelayCommand]
    private void Download()
    {
        IsDownloadEnabled = false;
        IsProgressVisible = true;
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        if (!Directory.Exists(Se.DictionariesFolder))
        {
            Directory.CreateDirectory(Se.DictionariesFolder);
        }

        var dirInfo = new DirectoryInfo(Se.DictionariesFolder);
        await Window!.Launcher.LaunchDirectoryInfoAsync(dirInfo);
    }

    [RelayCommand]
    private void Ok()
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