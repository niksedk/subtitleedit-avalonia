using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;

public partial class ExportEbuStlViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CodePageNumberItem> _codePages;
    [ObservableProperty] private CodePageNumberItem? _selectedCodePage;
    [ObservableProperty] private ObservableCollection<string> _diskFormatCodes;
    [ObservableProperty] private string? _selectedDiskFormatCode;
    [ObservableProperty] private ObservableCollection<string> _frameRates;
    [ObservableProperty] private string? _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _displayStandardCodes;
    [ObservableProperty] private string? _selectedDisplayStandardCode;
    [ObservableProperty] private ObservableCollection<string> _characterTables;
    [ObservableProperty] private string? _selectedCharacterTable;
    [ObservableProperty] private ObservableCollection<LanguageItem> _languageCodes;
    [ObservableProperty] private LanguageItem? _selectedLanguageCode;
    [ObservableProperty] private ObservableCollection<string> _timeCodeStatusList;
    [ObservableProperty] private string? _selectedTimeCodeStatus;

    [ObservableProperty] private ObservableCollection<int> _revisionNumbers;
    [ObservableProperty] private int? _selectedRevisionNumber;
    [ObservableProperty] private ObservableCollection<int> _maxCharactersPerRow;
    [ObservableProperty] private int? _selectedMaxCharactersPerRow;
    [ObservableProperty] private ObservableCollection<int> _maxRows;
    [ObservableProperty] private int? _selectedMaxRow;
    [ObservableProperty] private ObservableCollection<int> _discSequenceNumbers;
    [ObservableProperty] private int? _selectedDiscSequenceNumber;
    [ObservableProperty] private ObservableCollection<int> _totalNumerOfDiscsList;
    [ObservableProperty] private int? _selectedTotalNumberOfDiscs;

    [ObservableProperty] private string _originalProgramTitle;
    [ObservableProperty] private string _originalEpisodeTitle;
    [ObservableProperty] private string _translatedProgramTitle;
    [ObservableProperty] private string _translatedEpisodeTitle;
    [ObservableProperty] private string _translatorsName;
    [ObservableProperty] private string _subtitleListReferenceCode;
    [ObservableProperty] private string _countryOfOrigin;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    [ObservableProperty] private ObservableCollection<string> _justifications;
    [ObservableProperty] private string? _selectedJustification;
    [ObservableProperty] private ObservableCollection<int> _topAlignments;
    [ObservableProperty] private int? _selectedTopAlignment;
    [ObservableProperty] private ObservableCollection<int> _bottomAlignments;
    [ObservableProperty] private int? _selectedBottomAlignment;
    [ObservableProperty] private ObservableCollection<int> _rowsAddByNewLine;
    [ObservableProperty] private int? _selectedRowsAddByNewLine;
    [ObservableProperty] private bool _useBox;
    [ObservableProperty] private bool _useDoubleHeight;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public int? PacCodePage { get; private set; }

    private IFileHelper _fileHelper;

    public ExportEbuStlViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        CodePages = new ObservableCollection<CodePageNumberItem>(CodePageNumberItem.GetCodePageNumberItems());
        SelectedCodePage = CodePages[0];

        DiskFormatCodes = new ObservableCollection<string>
        {
            "STL23.01 (non-standard)",
            "STL24.01 (non-standard)",
            "STL25.01",
            "STL29.01 (non-standard)",
            "STL30.01",
        };

        FrameRates = new ObservableCollection<string>
        {
            "23.976",
            "24",
            "25",
            "29.97",
            "30",
        };

        DisplayStandardCodes = new ObservableCollection<string>
        {
            "0 Open subtitling",
            "1 Level-1 teletext",
            "2 Level-2 teletext",
            "Undefined",
        };

        CharacterTables = new ObservableCollection<string>
        {
            "Latin",
            "Latin/Cyrillic",
            "Latin/Arabic",
            "Latin/Greek",
            "Latin/Hebrew",
        };

        LanguageCodes = new ObservableCollection<LanguageItem>
        {
            new("00", ""),
            new("01", "Albanian"),
            new("02", "Breton"),
            new("03", "Catalan"),
            new("04", "Croatian"),
            new("05", "Welsh"),
            new("06", "Czech"),
            new("07", "Danish"),
            new("08", "German"),
            new("09", "English"),
            new("0A", "Spanish"),
            new("0B", "Esperanto"),
            new("0C", "Estonian"),
            new("0D", "Basque"),
            new("0E", "Faroese"),
            new("0F", "French"),
            new("10", "Frisian"),
            new("11", "Irish"),
            new("12", "Gaelic"),
            new("13", "Galician"),
            new("14", "Icelandic"),
            new("15", "Italian"),
            new("16", "Lappish"),
            new("17", "Latin"),
            new("18", "Latvian"),
            new("19", "Luxembourgi"),
            new("1A", "Lithuanian"),
            new("1B", "Hungarian"),
            new("1C", "Maltese"),
            new("1D", "Dutch"),
            new("1E", "Norwegian"),
            new("1F", "Occitan"),
            new("20", "Polish"),
            new("21", "Portuguese"),
            new("22", "Romanian"),
            new("23", "Romansh"),
            new("24", "Serbian"),
            new("25", "Slovak"),
            new("26", "Slovenian"),
            new("27", "Finnish"),
            new("28", "Swedish"),
            new("29", "Turkish"),
            new("2A", "Flemish"),
            new("2B", "Wallon"),
            new("7F", "Amharic"),
            new("7E", "Arabic"),
            new("7D", "Armenian"),
            new("7C", "Assamese"),
            new("7B", "Azerbaijani"),
            new("7A", "Bambora"),
            new("79", "Bielorussian"),
            new("78", "Bengali"),
            new("77", "Bulgarian"),
            new("76", "Burmese"),
            new("75", "Chinese"),
            new("74", "Churash"),
            new("73", "Dari"),
            new("72", "Fulani"),
            new("71", "Georgian"),
            new("70", "Greek"),
            new("6F", "Gujurati"),
            new("6E", "Gurani"),
            new("6D", "Hausa"),
            new("6C", "Hebrew"),
            new("6B", "Hindi"),
            new("6A", "Indonesian"),
            new("69", "Japanese"),
            new("68", "Kannada"),
            new("67", "Kazakh"),
            new("66", "Khmer"),
            new("65", "Korean"),
            new("64", "Laotian"),
            new("63", "Macedonian"),
            new("62", "Malagasay"),
            new("61", "Malaysian"),
            new("60", "Moldavian"),
            new("5F", "Marathi"),
            new("5E", "Ndebele"),
            new("5D", "Nepali"),
            new("5C", "Oriya"),
            new("5B", "Papamiento"),
            new("5A", "Persian"),
            new("59", "Punjabi"),
            new("58", "Pushtu"),
            new("57", "Quechua"),
            new("56", "Russian"),
            new("55", "Ruthenian"),
            new("54", "Serbocroat"),
            new("53", "Shona"),
            new("52", "Sinhalese"),
            new("51", "Somali"),
            new("50", "Sranan Tongo"),
            new("4F", "Swahili"),
            new("4E", "Tadzhik"),
            new("4D", "Tamil"),
            new("4C", "Tatar"),
            new("4B", "Telugu"),
            new("4A", "Thai"),
            new("49", "Ukrainian"),
            new("48", "Urdu"),
            new("47", "Uzbek"),
            new("46", "Vietnamese"),
            new("45", "Zulu"),
        };

        TimeCodeStatusList = new ObservableCollection<string>
        {
            "Not intended for use",
            "Intended for use",
        };

        Justifications = new ObservableCollection<string>
        {
            "Unchanged presentation",
            "Left-justified text",
            "Centered text",
            "Right-justified text",
        };

        RevisionNumbers = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        MaxCharactersPerRow = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        MaxRows = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        DiscSequenceNumbers = new ObservableCollection<int>(Enumerable.Range(0, 10).ToList());
        TotalNumerOfDiscsList = new ObservableCollection<int>(Enumerable.Range(0, 10).ToList());
        TopAlignments = new ObservableCollection<int>(Enumerable.Range(0, 51).ToList());
        BottomAlignments = new ObservableCollection<int>(Enumerable.Range(0, 51).ToList());
        RowsAddByNewLine = new ObservableCollection<int>(Enumerable.Range(0, 11).ToList());
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void Import()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}