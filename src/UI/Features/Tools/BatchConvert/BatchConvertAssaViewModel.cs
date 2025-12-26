using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertAssaViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceStylesIfPossible;
    [ObservableProperty] private string _text;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private Bitmap? _imagePreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Border TextBoxContainer { get; set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }

    private readonly IFolderHelper _folderHelper;

    public BatchConvertAssaViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        TextBoxContainer = new Border();    
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        
        foreach (var style in Se.Settings.Assa.StoredStyles)
        {
            if (style.IsDefault)
            {
                CurrentStyle = new  StyleDisplay(style);
                break;
            }
        }

        if (CurrentStyle == null)
        {
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(AdvancedSubStationAlpha.DefaultHeader);
            if (styles.Count > 0)
            {
                CurrentStyle = new StyleDisplay(styles[0]);
            }
        }

        UseSourceStylesIfPossible = true;
    }
    
    [RelayCommand]
    private void EditStyle()
    {
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]
    private void EditAttachment()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void EditProperties()
    {
        OkPressed = true;
        Window?.Close();
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
    
        private TextBoxWrapper CreateSimpleTextBoxWrapper()
    {
        var textBox = new TextBox
        {
            Margin = new Thickness(0, 0, 10, 0),
            [!TextBox.TextProperty] = new Binding(nameof(Text)) { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            AcceptsReturn = true,
        };

        return new TextBoxWrapper(textBox);
    }

    private TextEditorWrapper CreateAdvancedTextBoxWrapper(string text, SubtitleFormat subtitleFormat)
    {
        var textBox = new TextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = true,
            WordWrap = true,
        };

        // Override the built-in link color with our softer pastel color
        textBox.TextArea.TextView.LinkTextForegroundBrush = UiUtil.MakeLinkForeground();

        // Add syntax highlighting for subtitle source formats
        var lineTransformer = GetLineTransformer(text, subtitleFormat);
        if (lineTransformer != null)
        {
            textBox.TextArea.TextView.LineTransformers.Add(lineTransformer);
        }

        // Setup two-way binding manually since TextEditor doesn't support direct binding
        var isUpdatingFromViewModel = false;
        var isUpdatingFromEditor = false;

        void UpdateEditorFromViewModel()
        {
            if (isUpdatingFromEditor)
            {
                return;
            }

            isUpdatingFromViewModel = true;
            try
            {
                var text = Text ?? string.Empty;
                if (textBox.Text != text)
                {
                    textBox.Text = text;
                }
            }
            finally
            {
                isUpdatingFromViewModel = false;
            }
        }

        void UpdateViewModelFromEditor()
        {
            if (isUpdatingFromViewModel)
            {
                return;
            }

            isUpdatingFromEditor = true;
            try
            {
                if (Text != textBox.Text)
                {
                    Text = textBox.Text;
                }
            }
            finally
            {
                isUpdatingFromEditor = false;
            }
        }

        // Listen to ViewModel changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                UpdateEditorFromViewModel();
            }
        };

        // Listen to TextEditor changes
        textBox.TextChanged += (s, e) => UpdateViewModelFromEditor();

        // Initial text load
        UpdateEditorFromViewModel();

        var textBoxBorder = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            Child = textBox,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        return new TextEditorWrapper(textBox, textBoxBorder);
    }

    private static DocumentColorizingTransformer? GetLineTransformer(string text, SubtitleFormat subtitleFormat)
    {
        // SubRip (.srt) and WebVTT (.vtt) use similar time code formats
        if (subtitleFormat is SubRip ||
            subtitleFormat is WebVTT ||
            subtitleFormat is WebVTTFileWithLineNumber)
        {
            return new SubRipSourceSyntaxHighlighting();
        }

        // Advanced SubStation Alpha (.ass) and SubStation Alpha (.ssa) formats
        if (subtitleFormat is AdvancedSubStationAlpha || subtitleFormat is SubStationAlpha)
        {
            return new AssaSourceSyntaxHighlighting();
        }

        // XML-based formats (e.g., TTML, Netflix DFXP, etc.)
        if (subtitleFormat.Extension == ".xml" ||
            subtitleFormat.AlternateExtensions.Contains(".xml") ||
            text.Contains("<?xml version=") ||
            subtitleFormat is Sami ||
            subtitleFormat is SamiModern ||
            subtitleFormat is SamiYouTube ||
            subtitleFormat is SamiAvDicPlayer)
        {
            return new XmlSourceSyntaxHighlighting();
        }

        // Json-based formats
        if (subtitleFormat.Extension == ".json" ||
            subtitleFormat.AlternateExtensions.Contains(".json"))
        {
            return new JsonSourceSyntaxHighlighting();
        }

        // No syntax highlighting for other formats
        return null;
    }


    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
    }
}