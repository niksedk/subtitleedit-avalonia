using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TranslateRow> _rows;
    public AutoTranslateWindow? Window { get; set; }
    public bool OkPressed { get; set; }

    [ObservableProperty] private ObservableCollection<IAutoTranslator> _autoTranslators;
    [ObservableProperty] private IAutoTranslator _selectedAutoTranslator;

    [ObservableProperty] private string _autoTranslatorLinkText;

    [ObservableProperty] private ObservableCollection<TranslationPair> _sourceLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedSourceLanguage;

    [ObservableProperty] private ObservableCollection<TranslationPair> _targetLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedTargetLanguage;

    [ObservableProperty] private TranslateRow? _selectedTranslateRow;

    [ObservableProperty] private bool _isTranslateEnabled;

    [ObservableProperty] double _progressValue;
    [ObservableProperty] bool _isProgressEnabled;

    [ObservableProperty] private bool _apiKeyIsVisible;
    [ObservableProperty] private string _apiKeyText;
    [ObservableProperty] private bool _apiUrlIsVisible;
    [ObservableProperty] private string _apiUrlText;
    [ObservableProperty] private bool _buttonApiUrlIsVisible;
    [ObservableProperty] private bool _modelIsVisible;
    [ObservableProperty] private string _modelText;
    [ObservableProperty] private bool _buttonModelIsVisible;

    public DataGrid? RowGrid { get; set; }

    private CancellationTokenSource _cancellationTokenSource;
    private bool _translationInProgress = false;
    private bool _abort = false;
    private List<string> _apiUrls = new();
    private List<string> _apiModels = new();
    private bool _onlyCurrentLine;
    private Subtitle _subtitle = new Subtitle();
    private int _translationProgressIndex;
    private readonly IWindowService _windowService;

    public AutoTranslateViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        ApiKeyText = string.Empty;
        ApiUrlText = string.Empty;
        ModelText = string.Empty;

        AutoTranslators = new ObservableCollection<IAutoTranslator>
        {
            new GoogleTranslateV1(),
            new GoogleTranslateV2(),
            new MicrosoftTranslator(),
            new DeepLTranslate(),
            new LibreTranslate(),
            new MyMemoryApi(),
            new ChatGptTranslate(),
            new LmStudioTranslate(),
            new OllamaTranslate(),
            new AnthropicTranslate(),
            new GroqTranslate(),
            new OpenRouterTranslate(),
            new GeminiTranslate(),
            new PapagoTranslate(),
            new NoLanguageLeftBehindServe(),
            new NoLanguageLeftBehindApi(),
        };
        SelectedAutoTranslator = AutoTranslators[0];
        AutoTranslatorLinkText = SelectedAutoTranslator.Name;

        Rows = new ObservableCollection<TranslateRow>();
        IsTranslateEnabled = true;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
        var rows = subtitle.Paragraphs.Select(p => new TranslateRow
        {
            Number = p.Number,
            Show = p.StartTime.TimeSpan,
            Hide = p.EndTime.TimeSpan,
            Duration = p.Duration.ToShortDisplayString(),
            Text = p.Text,
        });
        Rows.AddRange(rows);

        UpdateSourceLanguages(SelectedAutoTranslator);
        UpdateTargetLanguages(SelectedAutoTranslator);

        if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.AutoTranslateLastName))
        {
            var autoTranslator = AutoTranslators.FirstOrDefault(x => x.Name == Se.Settings.AutoTranslate.AutoTranslateLastName);
            if (autoTranslator != null)
            {
                SetAutoTranslatorEngine(autoTranslator);
            }
        }

        if (Rows.Count > 0)
        {
            SelectedTranslateRow = Rows[0];
        }
    }

    private void UpdateSourceLanguages(IAutoTranslator autoTranslator)
    {
        UpdateSourceLanguages(autoTranslator, SourceLanguages);
    }

    private void UpdateSourceLanguages(IAutoTranslator autoTranslator, ObservableCollection<TranslationPair> sourceLanguages)
    {
        SourceLanguages.Clear();
        if (autoTranslator == null)
        {
            return;
        }

        foreach (var language in autoTranslator.GetSupportedSourceLanguages())
        {
            SourceLanguages.Add(language);
        }

        SelectedSourceLanguage = null;
        var sourceLanguageIsoCode = EvaluateDefaultSourceLanguageCode(_subtitle.OriginalEncoding, _subtitle, sourceLanguages);
        if (!string.IsNullOrEmpty(sourceLanguageIsoCode))
        {
            var lang = SourceLanguages.FirstOrDefault(p => p.Code == sourceLanguageIsoCode);
            if (lang != null)
            {
                SelectedSourceLanguage = lang;
            }
        }

        if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.AutoTranslateLastSource))
        {
            var lang = SourceLanguages.FirstOrDefault(p => p.Code == Se.Settings.AutoTranslate.AutoTranslateLastSource);
            if (lang != null)
            {
                SelectedSourceLanguage = lang;
            }
        }

        if (SelectedSourceLanguage == null && SourceLanguages.Count > 0)
        {
            SelectedSourceLanguage = SourceLanguages[0];
        }
    }

    private void UpdateTargetLanguages(IAutoTranslator autoTranslator)
    {
        TargetLanguages.Clear();
        if (autoTranslator == null)
        {
            return;
        }

        foreach (var language in autoTranslator.GetSupportedTargetLanguages())
        {
            TargetLanguages.Add(language);
        }

        SelectedTargetLanguage = null;
        var targetLanguageIsoCode = EvaluateDefaultTargetLanguageCode(SelectedTargetLanguage?.Code ?? string.Empty);
        if (!string.IsNullOrEmpty(targetLanguageIsoCode))
        {
            var lang = TargetLanguages.FirstOrDefault(p => p.Code == targetLanguageIsoCode);
            if (lang != null)
            {
                SelectedTargetLanguage = lang;
            }
        }

        if (!string.IsNullOrEmpty(Se.Settings.AutoTranslate.AutoTranslateLastTarget))
        {
            var lang = TargetLanguages.FirstOrDefault(p => p.Code == Se.Settings.AutoTranslate.AutoTranslateLastTarget);
            if (lang != null)
            {
                SelectedTargetLanguage = lang;
            }
        }

        if (SelectedTargetLanguage == null && TargetLanguages.Count > 0)
        {
            SelectedTargetLanguage = TargetLanguages[0];
        }
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
        _cancellationTokenSource.Cancel();
        _abort = true;
        IsProgressEnabled = false;

        if (IsTranslateEnabled)
        {
            Window?.Close();
            return;
        }
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        await _windowService.ShowDialogAsync<TranslateSettingsWindow, TranslateSettingsViewModel>(Window!, vm =>
        {
            vm.LoadValues(SelectedAutoTranslator);
        });
    }

    [RelayCommand]
    private async Task GoToAutranslatorUri()
    {
        var autoTranslator = SelectedAutoTranslator;
        if (autoTranslator == null)
        {
            return;
        }

        await Window!.Launcher.LaunchUriAsync(new System.Uri(autoTranslator.Url));
    }

    [RelayCommand]
    private async Task Translate()
    {
        _onlyCurrentLine = false;
        bool flowControl = await DoTranslate();
        if (!flowControl)
        {
            return;
        }
    }


    [RelayCommand]
    private async Task TranslateRow()
    {
        _onlyCurrentLine = true;
        await DoTranslate();
    }

    private async Task<bool> DoTranslate()
    {
        var translator = SelectedAutoTranslator;
        if (translator == null)
        {
            return false;
        }

        if (_translationInProgress)
        {
            _translationInProgress = false;
            _abort = true;
            await _cancellationTokenSource.CancelAsync();
            return false;
        }

        _abort = false;
        IsProgressEnabled = true;
        var engineType = translator.GetType();

        if (ApiKeyIsVisible && string.IsNullOrWhiteSpace(ApiKeyText))
        {
            await MessageBox.Show(
                Window!,
                "API key required",
                string.Format("{0} requires an API key.", translator.Name),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (ApiUrlIsVisible && string.IsNullOrWhiteSpace(ApiUrlText))
        {
            await MessageBox.Show(
               Window!,
               "URL key required",
               string.Format("{0} requires an URL.", translator.Name),
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
            return false;
        }

        _translationInProgress = true;
        _cancellationTokenSource = new CancellationTokenSource();


        SaveSettings();

        IsTranslateEnabled = false;

        translator.Initialize();

        var sourceLanguage = translator.GetSupportedSourceLanguages()
            .FirstOrDefault(p => p.Name.Equals(SelectedSourceLanguage!.ToString(), StringComparison.InvariantCultureIgnoreCase));

        var targetLanguage = translator.GetSupportedTargetLanguages()
            .FirstOrDefault(p => p.Name.Equals(SelectedTargetLanguage!.ToString(), StringComparison.InvariantCultureIgnoreCase));

        if (sourceLanguage == null || targetLanguage == null)
        {
            return false;
        }

        Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage = sourceLanguage.TwoLetterIsoLanguageName;
        Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage = targetLanguage.TwoLetterIsoLanguageName;


        // do translation in background
#pragma warning disable CS4014
        Task.Run(() =>
        {
            DoTranslate(sourceLanguage, targetLanguage, translator, _cancellationTokenSource.Token);
        });
#pragma warning restore CS4014
        return true;
    }

    private async Task DoTranslate(TranslationPair sourceLanguage, TranslationPair targetLanguage, IAutoTranslator translator, CancellationToken cancellationToken)
    {
        try
        {
            var start = 0;
            if (SelectedTranslateRow is TranslateRow selectedItem)
            {
                start = Rows.IndexOf(selectedItem);
            }

            var forceSingleLineMode = Configuration.Settings.Tools.AutoTranslateStrategy ==
                                      TranslateStrategy.TranslateEachLineSeparately.ToString() ||
                                      translator.Name ==
                                      NoLanguageLeftBehindApi.StaticName || // NLLB seems to miss some text...
                                      translator.Name == NoLanguageLeftBehindServe.StaticName ||
                                      _onlyCurrentLine;

            var index = start;
            var linesTranslated = 0;
            var errorCount = 0;
            while (index < Rows.Count)
            {
                if (_abort || cancellationToken.IsCancellationRequested)
                {
                    IsTranslateEnabled = true;
                    break;
                }

                var linesMergedAndTranslated = 0;

                if (!_onlyCurrentLine)
                {
                    linesMergedAndTranslated = await MergeAndSplitHelper.MergeAndTranslateIfPossible(Rows, sourceLanguage, targetLanguage, index, translator, forceSingleLineMode,
                        cancellationToken);
                }

                if (linesMergedAndTranslated > 0)
                {
                    index += linesMergedAndTranslated;

                    var index1 = index;
                    if (!_onlyCurrentLine)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            ProgressValue = (double)index1 * 100 / Rows.Count;
                            SelectAndScrollToRow(index1-1);
                        });
                    }

                    linesTranslated += linesMergedAndTranslated;
                    _translationProgressIndex = index;
                    errorCount = 0;
                    continue;
                }

                errorCount++;
                if (errorCount > 3)
                {
                    forceSingleLineMode = true;
                }

                var translateCount = await MergeAndSplitHelper.MergeAndTranslateIfPossible(
                    Rows,
                    sourceLanguage,
                    targetLanguage,
                    index,
                    translator,
                    forceSingleLineMode,
                    _cancellationTokenSource.Token);

                if (_abort || cancellationToken.IsCancellationRequested)
                {
                    IsTranslateEnabled = true;
                    return;
                }

                if (translateCount > 0)
                {
                    index += translateCount;
                    var progressIndex = index;
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ProgressValue = (double)progressIndex * 100 / Rows.Count;
                        SelectAndScrollToRow(progressIndex-1);
                    });

                    if (_onlyCurrentLine)
                    {
                        _translationProgressIndex = index - 1;
                        IsTranslateEnabled = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

        }
        catch (Exception ex)
        {
            _ = Dispatcher.UIThread.Invoke(async () =>
            {
                await MessageBox.Show(Window!, ex.Message, ex.StackTrace ?? "An error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }
        finally
        {
            IsTranslateEnabled = true;
            IsProgressEnabled = false;

            var lastTranslatedRow = Rows.LastOrDefault(p => !string.IsNullOrEmpty(p.Text));
            if (lastTranslatedRow != null)
            {
                SelectAndScrollToRow(Rows.IndexOf(lastTranslatedRow));
            }
        }
    }

    private void SelectAndScrollToRow(int index)
    {
        if (RowGrid == null)
        {
            return;
        }

        Dispatcher.UIThread.Invoke(async() =>
        {
            index = Math.Max(0, index);
            RowGrid.SelectedItem = Rows[index];

            var scrollIndex = Math.Min(index + 5, Rows.Count - 1);
            if (scrollIndex < 0)
            {
                return;
            }

            await Task.Delay(5);
            RowGrid.ScrollIntoView(Rows[scrollIndex], null);
        });
    }

    public void SaveSettings()
    {
        var translator = SelectedAutoTranslator;
        if (translator == null)
        {
            return;
        }

        var engineType = translator.GetType();
        var apiKey = ApiKeyText ?? string.Empty;
        var apiUrl = ApiUrlText ?? string.Empty;
        var apiModel = ModelText ?? string.Empty;

        if (engineType == typeof(GoogleTranslateV2))
        {
            Configuration.Settings.Tools.GoogleApiV2Key = apiKey.Trim();
        }

        if (engineType == typeof(MicrosoftTranslator))
        {
            Configuration.Settings.Tools.MicrosoftTranslatorApiKey = apiKey.Trim();
        }

        if (engineType == typeof(DeepLTranslate))
        {
            Configuration.Settings.Tools.AutoTranslateDeepLUrl = apiUrl.Trim();
            Configuration.Settings.Tools.AutoTranslateDeepLApiKey = apiKey.Trim();
        }

        if (engineType == typeof(LibreTranslate))
        {
            Configuration.Settings.Tools.AutoTranslateLibreUrl = apiUrl.Trim();
            Configuration.Settings.Tools.AutoTranslateLibreApiKey = apiKey.Trim();
        }

        if (engineType == typeof(MyMemoryApi))
        {
            Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey = apiKey.Trim();
        }

        if (engineType == typeof(ChatGptTranslate))
        {
            Configuration.Settings.Tools.ChatGptApiKey = apiKey.Trim();
            Configuration.Settings.Tools.ChatGptUrl = apiUrl.Trim();
            Configuration.Settings.Tools.ChatGptModel = apiModel.Trim();
        }

        if (engineType == typeof(LmStudioTranslate))
        {
            Configuration.Settings.Tools.LmStudioApiUrl = apiUrl.Trim();
            Configuration.Settings.Tools.LmStudioModel = apiModel.Trim();
        }

        if (engineType == typeof(OllamaTranslate))
        {
            Configuration.Settings.Tools.OllamaApiUrl = apiUrl.Trim();
            Configuration.Settings.Tools.OllamaModel = apiModel.Trim();
        }

        if (engineType == typeof(AnthropicTranslate))
        {
            Configuration.Settings.Tools.AnthropicApiKey = apiKey.Trim();
            Configuration.Settings.Tools.AnthropicApiModel = apiModel.Trim();
        }

        if (engineType == typeof(GroqTranslate))
        {
            Configuration.Settings.Tools.GroqApiKey = apiKey.Trim();
            Configuration.Settings.Tools.GroqModel = apiModel.Trim();
        }

        if (engineType == typeof(OpenRouterTranslate))
        {
            Configuration.Settings.Tools.OpenRouterApiKey = apiKey.Trim();
            Configuration.Settings.Tools.OpenRouterModel = apiModel.Trim();
        }

        if (engineType == typeof(GeminiTranslate))
        {
            Configuration.Settings.Tools.GeminiProApiKey = apiKey.Trim();
        }

        if (engineType == typeof(PapagoTranslate))
        {
            Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId = apiUrl.Trim();
            Configuration.Settings.Tools.AutoTranslatePapagoApiKey = apiKey.Trim();
        }

        Configuration.Settings.Tools.AutoTranslateLastName = SelectedAutoTranslator.Name;
        Se.Settings.AutoTranslate.AutoTranslateLastName = SelectedAutoTranslator.Name;
        Se.Settings.AutoTranslate.AutoTranslateLastSource = SelectedSourceLanguage?.Code ?? string.Empty;
        Se.Settings.AutoTranslate.AutoTranslateLastTarget = SelectedTargetLanguage?.Code ?? string.Empty;

        Se.SaveSettings();
    }

    internal void AutoTranslatorChanged(AvaloniaObject sender)
    {
        var translator = SelectedAutoTranslator;
        if (translator == null)
        {
            return;
        }

        SetAutoTranslatorEngine(translator);
        UpdateSourceLanguages(translator);
        UpdateTargetLanguages(translator);
    }

    private void SetAutoTranslatorEngine(IAutoTranslator translator)
    {
        SelectedAutoTranslator = translator;
        AutoTranslatorLinkText = translator.Name;

        ApiKeyIsVisible = false;
        ApiKeyText = string.Empty;
        ApiUrlIsVisible = false;
        ApiUrlText = string.Empty;
        ButtonApiUrlIsVisible = false;
        //LabelFormality.IsVisible = false;
        //PickerFormality.IsVisible = false;
        ModelIsVisible = false;
        ButtonModelIsVisible = false;
        ModelText = string.Empty;
        //LabelApiUrl.Text = "API url";
        //LabelApiKey.Text = "API key";

        _apiUrls.Clear();
        _apiModels.Clear();

        var engineType = translator.GetType();

        if (engineType == typeof(GoogleTranslateV1))
        {
            return;
        }

        if (engineType == typeof(GoogleTranslateV2))
        {
            ApiKeyText = Configuration.Settings.Tools.GoogleApiV2Key;
            ApiKeyIsVisible = true;
            return;
        }

        if (engineType == typeof(MicrosoftTranslator))
        {
            ApiKeyText = Configuration.Settings.Tools.MicrosoftTranslatorApiKey;
            ApiKeyIsVisible = true;
            return;
        }

        if (engineType == typeof(DeepLTranslate))
        {
            //LabelFormality.IsVisible = true;
            //PickerFormality.IsVisible = true;

            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.AutoTranslateDeepLUrl,
                Configuration.Settings.Tools.AutoTranslateDeepLUrl.Contains("api-free.deepl.com") ? "https://api.deepl.com/" : "https://api-free.deepl.com/",
            });

            ApiKeyText = Configuration.Settings.Tools.AutoTranslateDeepLApiKey;
            ApiKeyIsVisible = true;

            //SelectFormality();

            return;
        }

        if (engineType == typeof(NoLanguageLeftBehindServe))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.AutoTranslateNllbServeUrl,
                "http://127.0.0.1:6060/",
                "http://192.168.8.127:6060/",
            });

            return;
        }

        if (engineType == typeof(NoLanguageLeftBehindApi))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.AutoTranslateNllbApiUrl,
                "http://localhost:7860/api/v2/",
                "https://winstxnhdw-nllb-api.hf.space/api/v2/",
            });

            return;
        }

        if (engineType == typeof(LibreTranslate))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.AutoTranslateLibreUrl,
                "http://localhost:5000/",
                "https://libretranslate.com/",
                "https://translate.argosopentech.com/",
                "https://translate.terraprint.co/",
            });

            ApiKeyText = Configuration.Settings.Tools.AutoTranslateLibreApiKey;
            ApiKeyIsVisible = true;

            return;
        }

        if (engineType == typeof(PapagoTranslate))
        {
            //LabelApiUrl.Text = "Client ID";
            ApiUrlText = Configuration.Settings.Tools.AutoTranslatePapagoApiKeyId;
            ApiUrlIsVisible = true;
            //LabelApiUrl.IsVisible = true;

            //LabelApiKey.Text = "Client secret";
            ApiKeyText = Configuration.Settings.Tools.AutoTranslatePapagoApiKey;
            ApiKeyIsVisible = true;

            return;
        }


        if (engineType == typeof(MyMemoryApi))
        {
            ApiKeyText = Configuration.Settings.Tools.AutoTranslateMyMemoryApiKey;
            ApiKeyIsVisible = true;
            return;
        }

        if (engineType == typeof(ChatGptTranslate))
        {
            Configuration.Settings.Tools.ChatGptUrl ??= "https://api.openai.com/v1/chat/completions";

            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.ChatGptUrl.TrimEnd('/'),
                Configuration.Settings.Tools.ChatGptUrl.StartsWith("http://localhost:1234/v1/chat/completions", StringComparison.OrdinalIgnoreCase) ? "https://api.openai.com/v1/chat/completions" : "http://localhost:1234/v1/chat/completions"
            });

            ModelIsVisible = true;
            _apiModels = ChatGptTranslate.Models.ToList();

            if (string.IsNullOrWhiteSpace(Configuration.Settings.Tools.ChatGptModel))
            {
                Configuration.Settings.Tools.ChatGptModel = ChatGptTranslate.Models[0];
            }

            ModelText = Configuration.Settings.Tools.ChatGptModel;

            ApiKeyText = Configuration.Settings.Tools.ChatGptApiKey;
            ApiKeyIsVisible = true;
            return;
        }

        if (engineType == typeof(LmStudioTranslate))
        {
            if (string.IsNullOrEmpty(Configuration.Settings.Tools.LmStudioApiUrl))
            {
                Configuration.Settings.Tools.LmStudioApiUrl = "http://localhost:1234/v1/chat/completions";
            }

            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.LmStudioApiUrl.TrimEnd('/'),
            });

            return;
        }

        if (engineType == typeof(OllamaTranslate))
        {
            if (Configuration.Settings.Tools.OllamaApiUrl == null)
            {
                Configuration.Settings.Tools.OllamaApiUrl = "http://localhost:11434/api/generate";
            }

            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.OllamaApiUrl.TrimEnd('/'),
            });

            _apiModels = Configuration.Settings.Tools.OllamaModels.Split(',').ToList();
            ModelIsVisible = true;
            ButtonModelIsVisible = true;
            ModelText = Configuration.Settings.Tools.OllamaModel;

            //comboBoxFormality.ContextMenuStrip = contextMenuStripOlamaModels;

            return;
        }

        if (engineType == typeof(AnthropicTranslate))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.AnthropicApiUrl,
            });

            ApiKeyText = Configuration.Settings.Tools.AnthropicApiKey;
            ApiKeyIsVisible = true;

            _apiModels = AnthropicTranslate.Models.ToList();
            ModelIsVisible = true;
            ButtonModelIsVisible = true;
            ModelText = Configuration.Settings.Tools.AnthropicApiModel;

            return;
        }

        if (engineType == typeof(GroqTranslate))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.GroqUrl,
            });

            ApiKeyText = Configuration.Settings.Tools.GroqApiKey;
            ApiKeyIsVisible = true;

            _apiModels = GroqTranslate.Models.ToList();
            ModelIsVisible = true;
            ButtonModelIsVisible = true;
            ModelText = string.IsNullOrEmpty(Configuration.Settings.Tools.GroqModel) ? _apiModels[0] : Configuration.Settings.Tools.GroqModel;

            return;
        }


        if (engineType == typeof(OpenRouterTranslate))
        {
            FillUrls(new List<string>
            {
                Configuration.Settings.Tools.OpenRouterUrl,
            });

            ApiKeyText = Configuration.Settings.Tools.OpenRouterApiKey;
            ApiKeyIsVisible = true;

            _apiModels = OpenRouterTranslate.Models.ToList();
            ModelIsVisible = true;
            ButtonModelIsVisible = true;
            ModelText = string.IsNullOrEmpty(Configuration.Settings.Tools.OpenRouterModel) ? _apiModels[0] : Configuration.Settings.Tools.OpenRouterModel;

            return;
        }

        if (engineType == typeof(GeminiTranslate))
        {
            ApiKeyText = Configuration.Settings.Tools.GeminiProApiKey;
            ApiKeyIsVisible = true;
            return;
        }

        throw new Exception($"Engine {translator.Name} not handled!");
    }

    private void FillUrls(List<string> urls)
    {
        ApiUrlText = urls.Count > 0 ? urls[0] : string.Empty;
        _apiUrls = urls;
        ApiUrlIsVisible = true;
        ButtonApiUrlIsVisible = urls.Count > 0;
    }

    public static string EvaluateDefaultSourceLanguageCode(Encoding? encoding, Subtitle subtitle, ObservableCollection<TranslationPair> sourceLanguages)
    {
        var defaultSourceLanguageCode = string.Empty;
        if (encoding != null)
        {
            defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(encoding); // Guess language via encoding
        }
        if (string.IsNullOrEmpty(defaultSourceLanguageCode))
        {
            defaultSourceLanguageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle); // Guess language based on subtitle contents
        }

        if (!string.IsNullOrEmpty(Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage) &&
            Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage.StartsWith(defaultSourceLanguageCode) &&
            sourceLanguages.Any(p => p.Code == Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage))
        {
            return Configuration.Settings.Tools.GoogleTranslateLastSourceLanguage;
        }

        return defaultSourceLanguageCode;
    }

    public static string EvaluateDefaultTargetLanguageCode(string defaultSourceLanguage)
    {
        var installedLanguages = new List<string>(); // Get installed languages

        var currentCulture = CultureInfo.CurrentCulture;
        var currentLanguage = currentCulture.Name.Split('-').LastOrDefault();
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var cultureByName = cultures.FirstOrDefault(p => p.Name.EndsWith(currentLanguage));
            if (cultureByName != null)
            {
                installedLanguages.Add(cultureByName.TwoLetterISOLanguageName);
            }
        }

        var uiCultureTargetLanguage = Configuration.Settings.Tools.GoogleTranslateLastTargetLanguage;
        if (uiCultureTargetLanguage == defaultSourceLanguage)
        {
            foreach (var s in Utilities.GetDictionaryLanguages())
            {
                var temp = s.Replace("[", string.Empty).Replace("]", string.Empty);
                if (temp.Length > 4)
                {
                    temp = temp.Substring(temp.Length - 5, 2).ToLowerInvariant();
                    if (temp != defaultSourceLanguage && installedLanguages.Any(p => p.Contains(temp)))
                    {
                        uiCultureTargetLanguage = temp;
                        break;
                    }
                }
            }
        }

        if (uiCultureTargetLanguage == defaultSourceLanguage)
        {
            foreach (var language in installedLanguages)
            {
                if (language != defaultSourceLanguage)
                {
                    uiCultureTargetLanguage = language;
                    break;
                }
            }
        }

        if (uiCultureTargetLanguage == defaultSourceLanguage)
        {
            var name = CultureInfo.CurrentCulture.Name;
            if (name.Length > 2)
            {
                name = name.Remove(0, name.Length - 2);
            }
            var iso = IsoCountryCodes.ThreeToTwoLetterLookup.FirstOrDefault(p => p.Value == name);
            if (!iso.Equals(default(KeyValuePair<string, string>)))
            {
                var iso639 = Iso639Dash2LanguageCode.GetTwoLetterCodeFromThreeLetterCode(iso.Key);
                if (!string.IsNullOrEmpty(iso639))
                {
                    uiCultureTargetLanguage = iso639;
                }
            }
        }

        // Set target language to something different than source language
        if (uiCultureTargetLanguage == defaultSourceLanguage && defaultSourceLanguage == "en")
        {
            uiCultureTargetLanguage = "es";
        }
        else if (uiCultureTargetLanguage == defaultSourceLanguage)
        {
            uiCultureTargetLanguage = "en";
        }

        return uiCultureTargetLanguage;
    }
}
