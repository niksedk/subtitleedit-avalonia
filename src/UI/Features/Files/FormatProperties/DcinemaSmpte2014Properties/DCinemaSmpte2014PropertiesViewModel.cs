using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaSmpte2014Properties;

public partial class DCinemaSmpte2014PropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _languages = null!;
    [ObservableProperty] private ObservableCollection<string> _timeCodeRates = null!;
    [ObservableProperty] private ObservableCollection<string> _fontEffects = null!;

    [ObservableProperty] private bool _generateIdAuto;
    [ObservableProperty] private string _subtitleId = string.Empty;
    [ObservableProperty] private string _movieTitle = string.Empty;
    [ObservableProperty] private int _reelNumber;
    [ObservableProperty] private string _selectedLanguage = string.Empty;
    [ObservableProperty] private string _issueDate = string.Empty;
    [ObservableProperty] private string _editRate = string.Empty;
    [ObservableProperty] private string _selectedTimeCodeRate = string.Empty;
    [ObservableProperty] private string _startTime = string.Empty;

    [ObservableProperty] private string _fontId = string.Empty;
    [ObservableProperty] private string _fontUri = string.Empty;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private string _selectedFontEffect = string.Empty;
    [ObservableProperty] private Color _fontEffectColor;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _topBottomMargin;
    [ObservableProperty] private int _fadeUpTime;
    [ObservableProperty] private int _fadeDownTime;

    public Window? Window { get; set; }

    public Subtitle Subtitle { get; set; } = new();

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;

    public DCinemaSmpte2014PropertiesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        _languages = new ObservableCollection<string>();
        foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            _languages.Add(x.TwoLetterISOLanguageName);
        }
        var sortedLanguages = _languages.OrderBy(l => l).ToList();
        _languages = new ObservableCollection<string>(sortedLanguages);

        _timeCodeRates = new ObservableCollection<string> { "24", "25", "30", "48" };
        _fontEffects = new ObservableCollection<string> { "None", "Border", "Shadow" };

        LoadSettings();
    }

    private void LoadSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        _generateIdAuto = ss.DCinemaAutoGenerateSubtitleId;

        if (!string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId))
        {
            _subtitleId = ss.CurrentDCinemaSubtitleId;
            _reelNumber = int.TryParse(ss.CurrentDCinemaReelNumber, out int reelNumber) ? reelNumber : 1;
            _movieTitle = ss.CurrentDCinemaMovieTitle ?? string.Empty;
            _selectedLanguage = ss.CurrentDCinemaLanguage ?? "en";
            _fontId = ss.CurrentDCinemaFontId ?? string.Empty;
            _editRate = ss.CurrentDCinemaEditRate ?? "24 1";
            _selectedTimeCodeRate = ss.CurrentDCinemaTimeCodeRate ?? "24";
            _startTime = string.IsNullOrEmpty(ss.CurrentDCinemaStartTime) ? "00:00:00:00" : ss.CurrentDCinemaStartTime;
            _fontUri = ss.CurrentDCinemaFontUri ?? string.Empty;
            _issueDate = ss.CurrentDCinemaIssueDate ?? DateTime.Now.ToString("s");
            _fontColor = ColorFromString(ss.CurrentDCinemaFontColor);

            if (ss.CurrentDCinemaFontEffect == "border")
            {
                _selectedFontEffect = _fontEffects[1];
            }
            else if (ss.CurrentDCinemaFontEffect == "shadow")
            {
                _selectedFontEffect = _fontEffects[2];
            }
            else
            {
                _selectedFontEffect = _fontEffects[0];
            }

            _fontEffectColor = ColorFromString(ss.CurrentDCinemaFontEffectColor);
            _fontSize = ss.CurrentDCinemaFontSize;
            _topBottomMargin = ss.DCinemaBottomMargin;
            _fadeUpTime = ss.DCinemaFadeUpTime;
            _fadeDownTime = ss.DCinemaFadeDownTime;
        }
        else
        {
            _subtitleId = DCinemaSmpte2007.GenerateId();
            _reelNumber = 1;
            _movieTitle = string.Empty;
            _selectedLanguage = "en";
            _fontId = "theFontId";
            _editRate = "24 1";
            _selectedTimeCodeRate = "24";
            _startTime = "00:00:00:00";
            _fontUri = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            _issueDate = DateTime.Now.ToString("s");
            _fontColor = Colors.White;
            _selectedFontEffect = _fontEffects[0];
            _fontEffectColor = Colors.Black;
            _fontSize = 42;
            _topBottomMargin = 8;
            _fadeUpTime = 0;
            _fadeDownTime = 0;
        }
    }

    private void SaveSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        ss.DCinemaAutoGenerateSubtitleId = GenerateIdAuto;
        ss.CurrentDCinemaSubtitleId = SubtitleId;
        ss.CurrentDCinemaMovieTitle = MovieTitle;
        ss.CurrentDCinemaReelNumber = ReelNumber.ToString();
        ss.CurrentDCinemaEditRate = EditRate;
        ss.CurrentDCinemaTimeCodeRate = SelectedTimeCodeRate;
        ss.CurrentDCinemaStartTime = StartTime;
        ss.CurrentDCinemaLanguage = SelectedLanguage;
        ss.CurrentDCinemaIssueDate = IssueDate;
        ss.CurrentDCinemaFontId = FontId;
        ss.CurrentDCinemaFontUri = FontUri;
        ss.CurrentDCinemaFontColor = ColorToString(FontColor);

        if (SelectedFontEffect == FontEffects[1])
        {
            ss.CurrentDCinemaFontEffect = "border";
        }
        else if (SelectedFontEffect == FontEffects[2])
        {
            ss.CurrentDCinemaFontEffect = "shadow";
        }
        else
        {
            ss.CurrentDCinemaFontEffect = string.Empty;
        }

        ss.CurrentDCinemaFontEffectColor = ColorToString(FontEffectColor);
        ss.CurrentDCinemaFontSize = FontSize;
        ss.DCinemaBottomMargin = TopBottomMargin;
        ss.DCinemaFadeUpTime = FadeUpTime;
        ss.DCinemaFadeDownTime = FadeDownTime;

        Se.SaveSettings();
    }

    private Color ColorFromString(string colorString)
    {
        if (string.IsNullOrEmpty(colorString))
        {
            return Colors.White;
        }

        try
        {
            return Color.Parse(colorString);
        }
        catch
        {
            return Colors.White;
        }
    }

    private string ColorToString(Color color)
    {
        return color.ToString();
    }

    public void Initialize(Subtitle subtitle)
    {
        Subtitle = subtitle;
    }

    [RelayCommand]
    private void GenerateSubtitleId()
    {
        SubtitleId = DCinemaSmpte2007.GenerateId();
    }

    [RelayCommand]
    private void GenerateFontUri()
    {
        FontUri = DCinemaSmpte2007.GenerateId();
    }

    [RelayCommand]
    private void SetTodayIssueDate()
    {
        IssueDate = DateTime.Now.ToString("s");
    }

    [RelayCommand]
    private async Task ChooseFontColor()
    {
        var vm = new ColorPickerViewModel { SelectedColor = FontColor };
        var dialog = new ColorPickerWindow(vm);
        if (Window != null)
        {
            var result = await dialog.ShowDialog<bool?>(Window);
            if (result == true)
            {
                FontColor = vm.SelectedColor;
            }
        }
    }

    [RelayCommand]
    private async Task ChooseFontEffectColor()
    {
        var vm = new ColorPickerViewModel { SelectedColor = FontEffectColor };
        var dialog = new ColorPickerWindow(vm);
        if (Window != null)
        {
            var result = await dialog.ShowDialog<bool?>(Window);
            if (result == true)
            {
                FontEffectColor = vm.SelectedColor;
            }
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, "Import D-Cinema properties", "D-Cinema profile", ".DCinema-interop-profile");
        if (fileName == null)
        {
            return;
        }

        try
        {
            var importer = new DcPropertiesSmpte();
            if (importer.Load(fileName))
            {
                GenerateIdAuto = Convert.ToBoolean(importer.GenerateIdAuto, CultureInfo.InvariantCulture);

                if (int.TryParse(importer.ReelNumber, out var reelNumber))
                {
                    ReelNumber = reelNumber;
                }

                SelectedLanguage = importer.Language ?? "en";
                EditRate = importer.EditRate ?? "24 1";
                SelectedTimeCodeRate = importer.TimeCodeRate ?? "24";

                if (double.TryParse(importer.StartTime, out var startTimeMs))
                {
                    var timeCode = new TimeCode(startTimeMs);
                    StartTime = timeCode.ToHHMMSSFF();
                }

                FontId = importer.FontId ?? "theFontId";
                FontUri = importer.FontUri ?? string.Empty;
                FontColor = ColorFromString(importer.FontColor);
                SelectedFontEffect = importer.Effect ?? "None";
                FontEffectColor = ColorFromString(importer.EffectColor);

                if (int.TryParse(importer.FontSize, out var fontSize))
                {
                    FontSize = fontSize;
                }

                if (int.TryParse(importer.TopBottomMargin, out var margin))
                {
                    TopBottomMargin = margin;
                }

                if (int.TryParse(importer.FadeUpTime, out var fadeUp))
                {
                    FadeUpTime = fadeUp;
                }

                if (int.TryParse(importer.FadeDownTime, out var fadeDown))
                {
                    FadeDownTime = fadeDown;
                }
            }
        }
        catch
        {
            // ignore import errors
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, "Export D-Cinema properties", "D-Cinema profile", ".DCinema-interop-profile");
        if (fileName == null)
        {
            return;
        }

        try
        {
            var tc = TimeCode.ParseToMilliseconds(StartTime);

            var exporter = new DcPropertiesSmpte
            {
                GenerateIdAuto = GenerateIdAuto.ToString(CultureInfo.InvariantCulture),
                ReelNumber = ReelNumber.ToString(CultureInfo.InvariantCulture),
                Language = SelectedLanguage,
                EditRate = EditRate,
                TimeCodeRate = SelectedTimeCodeRate,
                StartTime = tc.ToString(CultureInfo.InvariantCulture),
                FontId = FontId,
                FontUri = FontUri,
                FontColor = ColorToString(FontColor),
                Effect = SelectedFontEffect,
                EffectColor = ColorToString(FontEffectColor),
                FontSize = FontSize.ToString(CultureInfo.InvariantCulture),
                TopBottomMargin = TopBottomMargin.ToString(CultureInfo.InvariantCulture),
                FadeUpTime = FadeUpTime.ToString(CultureInfo.InvariantCulture),
                FadeDownTime = FadeDownTime.ToString(CultureInfo.InvariantCulture),
            };

            exporter.Save(fileName);
        }
        catch
        {
            // ignore export errors
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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