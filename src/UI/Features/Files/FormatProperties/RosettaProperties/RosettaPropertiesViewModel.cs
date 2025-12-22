using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public partial class RosettaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _languages;
    [ObservableProperty] private string _selectedLanguage;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private string _selectedFontSize;
    [ObservableProperty] private string _selectedLineHeight;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private List<string> bcp47LanguageTags = new List<string>()
    {
        "af-NA",
        "af-ZA",
        "am-ET",
        "ar-AE",
        "ar-BH",
        "ar-DZ",
        "ar-EG",
        "ar-IQ",
        "ar-JO",
        "ar-KW",
        "ar-LB",
        "ar-LY",
        "ar-MA",
        "ar-OM",
        "ar-QA",
        "ar-SA",
        "ar-SD",
        "ar-SY",
        "ar-TN",
        "ar-YE",
        "as-IN",
        "az-AZ",
        "bg-BG",
        "bn-BD",
        "bn-IN",
        "bs-BA",
        "ca-ES",
        "ca-FR",
        "ca-IT",
        "cs-CZ",
        "cy-GB",
        "da-DK",
        "da-GL",
        "de-AT",
        "de-BE",
        "de-CH",
        "de-DE",
        "de-LI",
        "de-LU",
        "el-CY",
        "el-GR",
        "en-AE",
        "en-AG",
        "en-AI",
        "en-AS",
        "en-AU",
        "en-BB",
        "en-BE",
        "en-BM",
        "en-BW",
        "en-BZ",
        "en-CA",
        "en-CC",
        "en-CK",
        "en-CM",
        "en-CX",
        "en-DG",
        "en-DK",
        "en-ER",
        "en-FJ",
        "en-FK",
        "en-FM",
        "en-GB",
        "en-GD",
        "en-GG",
        "en-GH",
        "en-GI",
        "en-GM",
        "en-GU",
        "en-GY",
        "en-HK",
        "en-IE",
        "en-IL",
        "en-IN",
        "en-IO",
        "en-JE",
        "en-JM",
        "en-KE",
        "en-KI",
        "en-KN",
        "en-KY",
        "en-LC",
        "en-LR",
        "en-LS",
        "en-MG",
        "en-MH",
        "en-MO",
        "en-MP",
        "en-MS",
        "en-MT",
        "en-MU",
        "en-MW",
        "en-MY",
        "en-NA",
        "en-NG",
        "en-NZ",
        "en-PG",
        "en-PH",
        "en-PK",
        "en-PN",
        "en-PR",
        "en-PW",
        "en-RW",
        "en-SB",
        "en-SC",
        "en-SD",
        "en-SE",
        "en-SG",
        "en-SH",
        "en-SI",
        "en-SL",
        "en-SS",
        "en-SX",
        "en-SZ",
        "en-TC",
        "en-TK",
        "en-TO",
        "en-TT",
        "en-TV",
        "en-TZ",
        "en-UG",
        "en-UM",
        "en-US",
        "en-VC",
        "en-VG",
        "en-VI",
        "en-VU",
        "en-WS",
        "en-ZA",
        "en-ZM",
        "en-ZW",
        "es-AR",
        "es-BO",
        "es-CL",
        "es-CO",
        "es-CR",
        "es-CU",
        "es-DO",
        "es-EC",
        "es-ES",
        "es-GT",
        "es-HN",
        "es-MX",
        "es-NI",
        "es-PA",
        "es-PE",
        "es-PR",
        "es-PY",
        "es-SV",
        "es-US",
        "es-UY",
        "es-VE",
        "et-EE",
        "eu-ES",
        "fa-AF",
        "fa-IR",
        "fi-FI",
        "fil-PH",
        "fo-FO",
        "fr-BE",
        "fr-BF",
        "fr-BI",
        "fr-BJ",
        "fr-CA",
        "fr-CD",
        "fr-CF",
        "fr-CG",
        "fr-CH",
        "fr-CI",
        "fr-CM",
        "fr-DJ",
        "fr-FR",
        "fr-GA",
        "fr-GF",
        "fr-GN",
        "fr-GP",
        "fr-HT",
        "fr-LU",
        "fr-MA",
        "fr-MC",
        "fr-MG",
        "fr-ML",
        "fr-MQ",
        "fr-MR",
        "fr-MU",
        "fr-NC",
        "fr-NE",
        "fr-PF",
        "fr-RE",
        "fr-RW",
        "fr-SN",
        "fr-SY",
        "fr-TD",
        "fr-TG",
        "fr-TN",
        "fr-VU",
        "fr-WF",
        "fr-YT",
        "ga-IE",
        "gl-ES",
        "gu-IN",
        "he-IL",
        "hi-IN",
        "hr-HR",
        "hu-HU",
        "hy-AM",
        "id-ID",
        "ig-NG",
        "is-IS",
        "it-CH",
        "it-IT",
        "it-SM",
        "ja-JP",
        "ka-GE",
        "kk-KZ",
        "km-KH",
        "kn-IN",
        "ko-KP",
        "ko-KR",
        "ky-KG",
        "lb-LU",
        "lo-LA",
        "lt-LT",
        "lv-LV",
        "mk-MK",
        "ml-IN",
        "mn-MN",
        "mr-IN",
        "ms-BN",
        "ms-MY",
        "mt-MT",
        "nb-NO",
        "nn-NO",
        "ne-IN",
        "ne-NP",
        "nl-AW",
        "nl-BE",
        "nl-NL",
        "nl-SR",
        "pa-IN",
        "pa-PK",
        "pl-PL",
        "ps-AF",
        "pt-AO",
        "pt-BR",
        "pt-CH",
        "pt-CV",
        "pt-GQ",
        "pt-GW",
        "pt-LU",
        "pt-MO",
        "pt-MZ",
        "pt-PT",
        "pt-ST",
        "pt-TL",
        "ro-MD",
        "ro-RO",
        "ru-BY",
        "ru-KG",
        "ru-KZ",
        "ru-MD",
        "ru-RU",
        "ru-UA",
        "si-LK",
        "sk-SK",
        "sl-SI",
        "sq-AL",
        "sq-MK",
        "sr-BA",
        "sr-ME",
        "sr-RS",
        "sv-FI",
        "sv-SE",
        "sw-CD",
        "sw-KE",
        "sw-TZ",
        "sw-UG",
        "ta-IN",
        "ta-LK",
        "ta-MY",
        "ta-SG",
        "te-IN",
        "th-TH",
        "ti-ER",
        "ti-ET",
        "tk-TM",
        "tr-CY",
        "tr-TR",
        "uk-UA",
        "ur-IN",
        "ur-PK",
        "uz-UZ",
        "vi-VN",
        "xh-ZA",
        "yo-BJ",
        "yo-NG",
        "zh-CN",
        "zh-HK",
        "zh-MO",
        "zh-SG",
        "zh-TW",
        "zh-Hans-CN",
        "zh-Hans-SG",
        "zh-Hant-HK",
        "zh-Hant-MO",
        "zh-Hant-TW",
        "zu-ZA"
    };


    public RosettaPropertiesViewModel()
    {
        Languages =
        [
            Se.Language.General.Autodetect,
            .. bcp47LanguageTags,
        ];
        SelectedLanguage = Languages[0];

        SelectedLanguage = string.Empty;
        SelectedFontName = string.Empty;
        SelectedFontSize = string.Empty;
        SelectedLineHeight = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        SelectedLineHeight = Se.Settings.Formats.RosettaLineHeight;
        SelectedFontSize = Se.Settings.Formats.RosettaFontSize;
        if (Se.Settings.Formats.RosettaLanguageAutoDetect)
        {
            SelectedLanguage = Languages[0];
        }
        else
        {
            var ci = new CultureInfo(Se.Settings.Formats.RosettaLanguage);
            SelectedLanguage = ci.TwoLetterISOLanguageName.ToLowerInvariant();
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Formats.RosettaLineHeight = SelectedLineHeight;
        Se.Settings.Formats.RosettaFontSize = SelectedFontSize;

        if (SelectedLanguage == Languages[0])
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = true;
            Se.Settings.Formats.RosettaLanguage = "en";
        }
        else
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = false;
            Se.Settings.Formats.RosettaLanguage = SelectedLanguage;
        }

        Se.SaveSettings();
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