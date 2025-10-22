using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

namespace Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;

public partial class FixNetflixErrorsViewModel : ObservableObject
{
    public class LanguageItem
    {
        public string Code { get; }
        public string Name { get; }

        public LanguageItem(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public static List<LanguageItem> GetAll()
        {
            return Iso639Dash2LanguageCode.List
                .Select(p => new LanguageItem(p.TwoLetterCode, p.EnglishName))
                .OrderBy(p => p.Name)
                .ToList();
        }
    }

    [ObservableProperty] private DisplayFile? _selectedFile;
    [ObservableProperty] private ObservableCollection<LanguageItem> _languages;
    [ObservableProperty] private LanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<FixNetflixErrorsItem> _fixes;
    [ObservableProperty] private FixNetflixErrorsItem? _selectedFix;
    [ObservableProperty] private string _fixText;
    [ObservableProperty] private bool _fixTextEnabled;

    // New: selectable list of Netflix checks
    [ObservableProperty] private ObservableCollection<NetflixCheckDisplayItem> _checks = new();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; }

    private Subtitle _subtitle;
    private readonly Timer _timer;
    private bool _dirty;
    private readonly List<Paragraph> _edited;

    private readonly IWindowService _windowService;

    public FixNetflixErrorsViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Languages = new ObservableCollection<LanguageItem>(LanguageItem.GetAll());
        Fixes = new ObservableCollection<FixNetflixErrorsItem>();
        FixText = string.Empty;
        _edited = new List<Paragraph>();
        _timer = new Timer(500);
        _timer.Elapsed += TimerElapsed;
        FixedSubtitle = new Subtitle();
        _subtitle = new Subtitle();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        LoadSettings();
        LoadChecks();
        SetDirty();
    }

    private void LoadChecks()
    {
        Checks.Clear();
        foreach (var checker in NetflixQualityController.GetAllCheckers())
        {
            var name = GetFriendlyCheckerName(checker.GetType().Name);
            Checks.Add(new NetflixCheckDisplayItem(checker, name, true));
        }
    }

    private static string GetFriendlyCheckerName(string typeName)
    {
        // Remove common prefix
        if (typeName.StartsWith("NetflixCheck", StringComparison.Ordinal))
        {
            typeName = typeName.Substring("NetflixCheck".Length);
        }

        // Insert spaces before capitals
        var chars = new List<char>();
        for (int i = 0; i < typeName.Length; i++)
        {
            var c = typeName[i];
            if (i > 0 && char.IsUpper(c) && (char.IsLower(typeName[i - 1]) || (i + 1 < typeName.Length && char.IsLower(typeName[i + 1]))))
            {
                chars.Add(' ');
            }
            chars.Add(c);
        }
        return new string(chars.ToArray());
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        FixedSubtitle = new Subtitle(_subtitle, false);
        FixedSubtitle.Paragraphs.Clear();
        for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
        {
            var p = _subtitle.Paragraphs[index];
            var fixedParagraph = Fixes.FirstOrDefault(ri => ri.Index == index);
            if (fixedParagraph != null)
            {
                p.Text = fixedParagraph.After;
            }

            FixedSubtitle.Paragraphs.Add(p);
        }

        FixedSubtitle.RemoveEmptyLines();

        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void ChecksSelectAll()
    {
        foreach (var c in Checks)
        {
            c.IsSelected = true;
        }
        SetDirty();
    }

    [RelayCommand]
    private void ChecksInverseSelection()
    {
        foreach (var c in Checks)
        {
            c.IsSelected = !c.IsSelected;
        }
        SetDirty();
    }

    private void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();

        try
        {
            if (_dirty)
            {
                _dirty = false;
                Dispatcher.UIThread.Invoke(GeneratePreview);
            }
        }
        catch
        {
            return;
        }

        _timer.Start();
    }

    private void GeneratePreview()
    {
        // Build selected checks list
        var selectedChecks = Checks.Where(c => c.IsSelected).Select(c => c.Checker).ToList();
        Fixes.Clear();

        if (_subtitle.Paragraphs.Count == 0 || selectedChecks.Count == 0)
        {
            return;
        }

        var controller = new NetflixQualityController
        {
            Language = SelectedLanguage?.Code ?? "en",
            FrameRate = Configuration.Settings.General.CurrentFrameRate,
        };

        controller.RunChecks(_subtitle, selectedChecks);

        // Map paragraph to proposed text changes (ignore pure timing-only changes for now)
        var fixMap = new Dictionary<int, (string Before, string After, Paragraph P)>();
        foreach (var r in controller.Records)
        {
            if (r.OriginalParagraph == null)
            {
                continue;
            }

            var idx = _subtitle.Paragraphs.IndexOf(r.OriginalParagraph);
            if (idx < 0)
            {
                continue;
            }

            var before = r.OriginalParagraph.Text;
            var after = r.FixedParagraph?.Text;
            if (!string.IsNullOrEmpty(after) && !string.Equals(before, after, StringComparison.Ordinal))
            {
                // If multiple fixes affect the same paragraph, keep last suggestion
                fixMap[idx] = (before, after, r.OriginalParagraph);
            }
        }

        if (fixMap.Count == 0)
        {
            return;
        }

        foreach (var kvp in fixMap.OrderBy(k => k.Key))
        {
            var index = kvp.Key;
            var (before, after, p) = kvp.Value;
            var item = new FixNetflixErrorsItem(true, index, before, after, p);
            Fixes.Add(item);
        }
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        SetDirty();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(_subtitle) ?? "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == languageCode) ??
            Languages.FirstOrDefault(l => l.Code == "en");
        
        _timer.Start();

        if (Checks.Count == 0)
        {
            LoadChecks();
        }

        SetDirty();
    }

    public void SetDirty()
    {
        _dirty = true;
    }
}