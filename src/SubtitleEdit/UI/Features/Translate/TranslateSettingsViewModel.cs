using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class TranslateSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _mergeOptions;
    [ObservableProperty] private string _selectedMergeOptions;

    [ObservableProperty] private decimal? _serverDelaySeconds;
    [ObservableProperty] private decimal? _maxBytesRequest;

    [ObservableProperty] private string _promptEntry;

    [ObservableProperty] private bool _promptIsVisible;

    public TranslateSettingsWindow? Window { get; internal set; }
    public IAutoTranslator? AutoTranslator;
    public bool OkPressed { get; private set; }

    public TranslateSettingsViewModel()
    {
        MergeOptions = new ObservableCollection<string>
        {
            "None",
            "Merge lines",
            "Merge lines with same text",
            "Merge lines with same text and time",
            "Merge lines with same text and time and remove empty lines"
        };
        SelectedMergeOptions = MergeOptions[0];

        ServerDelaySeconds = 0;

        MaxBytesRequest = 1000;

        PromptEntry = string.Empty;
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

    public void SetValues()
    {
        //LineMergeSelectedItem = LineMergeItems[0];
        //PickerLineMerge.SelectedItem = LineMergeSelectedItem;

        //MaximumBytes = Configuration.Settings.Tools.AutoTranslateMaxBytes.ToString(CultureInfo.InvariantCulture);
        //MaximumBytesEntry.Text = MaximumBytes;

        //DelayInSeconds = Configuration.Settings.Tools.AutoTranslateDelaySeconds.ToString(CultureInfo.InvariantCulture);
        //DelayEntry.Text = DelayInSeconds;

        if (AutoTranslator == null)
        {
            return;
        }

        var engineType = AutoTranslator.GetType();
        if (engineType == typeof(ChatGptTranslate))
        {
            PromptEntry = Se.Settings.Tools.AutoTranslate.ChatGptPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().ChatGptPrompt;
            }
        }
        else if (engineType == typeof(OllamaTranslate))
        {
            PromptEntry = Se.Settings.Tools.OllamaPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().OllamaPrompt;
            }
        }
        else if (engineType == typeof(LmStudioTranslate))
        {
            PromptEntry = Se.Settings.Tools.LmStudioPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().LmStudioPrompt;
            }
        }
        else if (engineType == typeof(AnthropicTranslate))
        {
            PromptEntry = Se.Settings.Tools.AnthropicPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().AnthropicPrompt;
            }
        }
        else if (engineType == typeof(GroqTranslate))
        {
            PromptEntry = Se.Settings.Tools.GroqPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().GroqPrompt;
            }
        }
        else if (engineType == typeof(OpenRouterTranslate))
        {
            PromptEntry = Se.Settings.Tools.OpenRouterPrompt;
            if (string.IsNullOrWhiteSpace(PromptEntry))
            {
                PromptEntry = new SeAutoTranslate().OpenRouterPrompt;
            }
        }
        else
        {
            PromptIsVisible = false;
        }
    }
}
