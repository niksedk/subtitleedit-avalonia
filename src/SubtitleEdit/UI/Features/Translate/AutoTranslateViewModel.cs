using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel : ObservableObject
{
    private ObservableCollection<TranslateRow> _rows;
    public ITreeDataGridSource TranslateRowSource { get; }
    public AutoTranslateWindow Window { get; set; }
    public bool OkPressed { get; set; }

    [ObservableProperty] private ObservableCollection<IAutoTranslator> _autoTranslators;
    [ObservableProperty] private IAutoTranslator _selectedAutoTranslator;

    [ObservableProperty] private string _autoTranslatorLinkText;

    [ObservableProperty] private ObservableCollection<TranslationPair> _sourceLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedSourceLanguage;

    [ObservableProperty] private ObservableCollection<TranslationPair> _targetLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedTargetLanguage;

    [ObservableProperty] private bool _apiKeyIsVisible;
    [ObservableProperty] private string _apiKeyText;
    [ObservableProperty] private bool _apiUrlIsVisible;
    [ObservableProperty] private string _apiUrlText;
    [ObservableProperty] private bool _modelIsVisible;
    [ObservableProperty] private string _modelText;

    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _translationInProgress = false;
    private bool _abort = false;
    private List<string> _apiUrls = new();
    private List<string> _apiModels = new();
    private bool _onlyCurrentLine;

    public AutoTranslateViewModel()
    {
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

        _rows = new ObservableCollection<TranslateRow>();
        TranslateRowSource = new FlatTreeDataGridSource<TranslateRow>(_rows)
        {
            Columns =
            {
                new TextColumn<TranslateRow, int>("#", x => x.Number),
                new TextColumn<TranslateRow, string>("Show", x => x.Show),
                new TextColumn<TranslateRow, string>("Duration", x => x.Duration),
                new TextColumn<TranslateRow, string>("Text", x => x.Text),
                new TextColumn<TranslateRow, string>("Translated text", x => x.TranslatedText),
            },
        };

        var dataGridSource = TranslateRowSource as FlatTreeDataGridSource<TranslateRow>;
        dataGridSource!.RowSelection!.SingleSelect = true;
    }

    public void Initialize(Subtitle subtitle)
    {
        var rows = subtitle.Paragraphs.Select(p => new TranslateRow
        {
            Number = p.Number,
            Show = p.StartTime.ToDisplayString(),
            Duration = p.Duration.ToShortDisplayString(),
            Text = p.Text,
        });
        _rows.AddRange(rows);

        UpdateSourceLanguages(SelectedAutoTranslator);
        UpdateTargetLanguages(SelectedAutoTranslator);

        if (!string.IsNullOrEmpty(Se.Settings.Tools.AutoTranslateLastName))
        {
            var autoTranslator = AutoTranslators.FirstOrDefault(x => x.Name == Se.Settings.Tools.AutoTranslateLastName);
            if (autoTranslator != null)
            {
                SelectedAutoTranslator = autoTranslator;
                SetAutoTranslatorEngine(autoTranslator);
            }
        }
    }

    private void UpdateSourceLanguages(IAutoTranslator autoTranslator)
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

        if (SourceLanguages.Count > 0)
        {
            SelectedSourceLanguage = SourceLanguages[0];
        }
        else
        {
            SelectedSourceLanguage = null;
        }
    }

    private void UpdateTargetLanguages(IAutoTranslator autoTranslator)
    {
        TargetLanguages.Clear();
        if (autoTranslator == null)
        {
            return;
        }

        foreach (var language in autoTranslator.GetSupportedSourceLanguages())
        {
            TargetLanguages.Add(language);
        }

        if (TargetLanguages.Count > 0)
        {
            SelectedTargetLanguage = TargetLanguages[0];
        }
        else
        {
            SelectedTargetLanguage = null;
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
        Window?.Close();
    }

    [RelayCommand]
    private async Task GoToAutranslatorUri()
    {
        var autoTranslator = SelectedAutoTranslator;
        if (autoTranslator == null)
        {
            return;
        }

        await Window.Launcher.LaunchUriAsync(new System.Uri(autoTranslator.Url));
    }

    [RelayCommand]
    private async Task Translate()
    {
        var translator = SelectedAutoTranslator;
        if (translator == null)
        {
            return;
        }

        if (_translationInProgress)
        {
            _translationInProgress = false;
            _abort = true;
            await _cancellationTokenSource.CancelAsync();
            return;
        }

        var engineType = translator.GetType();

        if (ApiKeyIsVisible && string.IsNullOrWhiteSpace(ApiKeyText))
        {
            await MessageBox.Show(
                Window,
                "API key required",
                string.Format("{0} requires an API key.", translator.Name),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (ApiUrlIsVisible && string.IsNullOrWhiteSpace(ApiUrlText))
        {
            await MessageBox.Show(
               Window,
               "URL key required",
               string.Format("{0} requires an URL.", translator.Name),
               MessageBoxButtons.OK,
               MessageBoxIcon.Error);
            return;
        }

        _translationInProgress = true;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [RelayCommand]
    private void TranslateRow()
    {

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
        Se.Settings.Tools.AutoTranslateLastName = SelectedAutoTranslator.Name;

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
        AutoTranslatorLinkText = translator.Name;

        ApiKeyIsVisible = false;
        ApiKeyText = string.Empty;
        ApiUrlIsVisible = false;
        ApiUrlText = string.Empty;
        //ButtonApiUrl.IsVisible = false;
        //LabelFormality.IsVisible = false;
        //PickerFormality.IsVisible = false;
        ModelIsVisible = false;
        //ButtonModel.IsVisible = false;
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
            //ButtonModel.IsVisible = true;
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
            //ButtonModel.IsVisible = true;
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
            //ButtonModel.IsVisible = true;
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
            //ButtonModel.IsVisible = true;
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
        //ButtonApiUrl.IsVisible = urls.Count > 0;
    }
}
