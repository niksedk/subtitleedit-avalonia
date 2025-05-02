using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel : ObservableObject
{
    private ObservableCollection<TranslateRow> _rows;
    public ITreeDataGridSource TranslateRowSource { get;   }
    public AutoTranslateWindow Window { get; set; }
    public bool OkPressed { get; set; }
    
    [ObservableProperty] private ObservableCollection<IAutoTranslator> _autoTranslators;
    [ObservableProperty] private IAutoTranslator _selectedAutoTranslator;

    [ObservableProperty] private ObservableCollection<TranslationPair> _sourceLanguages = new();
    [ObservableProperty] private TranslationPair? _selectedSourceLanguage;

    [ObservableProperty] private ObservableCollection<TranslationPair> _targetLanguages  = new();
    [ObservableProperty] private TranslationPair? _selectedTargetLanguage;

    private CancellationTokenSource _cancellationTokenSource = new();
    
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
             Duration = p.Duration.ToDisplayString(),
             Text = p.Text,
        });
        _rows.AddRange(rows);

        UpdateSourceLanguages(SelectedAutoTranslator);
        UpdateTargetLanguages(SelectedAutoTranslator);
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
    private void Translate()
    {
     
    }

    [RelayCommand]
    private void TranslateRow()
    {

    }
}

public class TranslateRow
{
    public int Number { get; set; }
    public string Show { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    public string TranslatedText { get; set; }
}