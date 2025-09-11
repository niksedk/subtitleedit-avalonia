using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Edit.Find;

public partial class FindViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private bool _findTypeNormal;
    [ObservableProperty] private bool _findTypeCanseInsensitive;
    [ObservableProperty] private bool _findTypeRegularExpression;
    [ObservableProperty] private string _countResult;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private IFindService? _findService;
    private List<string> _subs = new List<string>();

    public FindViewModel()
    {
        SearchText = string.Empty;
        CountResult = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        WholeWord = Se.Settings.Edit.Find.FindWholeWords;

        if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseInsensitive.ToString())
        {
            FindTypeCanseInsensitive = true;
        }
        else if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseSensitive.ToString())
        {
            FindTypeNormal = true;
        }
        else
        {
            FindTypeRegularExpression = true;
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Edit.Find.FindWholeWords = WholeWord;
        Se.Settings.Edit.Find.FindSearchType.ToString();
    }

    [RelayCommand]
    private void FindPrevious()
    {
        CountResult = string.Empty;
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void FindNext()
    {
        CountResult = string.Empty;
        OkPressed = true;
        SaveSettings();
        Window?.Close();
    }

    [RelayCommand]
    private void Count()
    {
        CountResult = string.Empty;
        if (_findService == null || string.IsNullOrEmpty(SearchText))
        {
            return;
        }

        _findService.Initialize(
            _subs,
            0,
            WholeWord,
            FindTypeNormal ? FindMode.CaseSensitive : FindTypeCanseInsensitive ? FindMode.CaseInsensitive : FindMode.RegularExpression);

        var count = _findService.Count(SearchText);

        if (count <= 0)
        {
            CountResult = Se.Language.General.FoundNoMatches;
        }
        else if (count == 1)
        {
            CountResult = Se.Language.General.FoundOneMatch;
        }
        else
        {
            CountResult = string.Format(Se.Language.General.FoundXMatches, count);
        }

        SaveSettings();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void FindTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            FindNext();
        }
    }

    internal void Initialize(IFindService findService, List<string> subs)
    {
        _findService = findService;
        _subs = subs;
    }
}