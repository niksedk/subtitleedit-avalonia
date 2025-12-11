using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.SourceView;

public partial class SourceViewViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _lineAndColumnInfo;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public Subtitle Subtitle { get; private set; }

    public SubtitleFormat _subtitleFormat { get; private set; }
    public ITextBoxWrapper SourceViewTextBox { get; set; }
    public Border TextBoxContainer { get; set; }

    private readonly System.Timers.Timer _cursorTimer;

    public SourceViewViewModel()
    {
        SourceViewTextBox = new TextBoxWrapper(new TextBox());  
        Title = string.Empty;
        Text = string.Empty;
        LineAndColumnInfo = string.Empty;
        Subtitle = new Subtitle();
        _subtitleFormat = new SubRip();
        TextBoxContainer = new Border();

        _cursorTimer = new System.Timers.Timer(200);
        _cursorTimer.Elapsed += (sender, args) =>
        {
            if (SourceViewTextBox == null)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                var caretIndex = SourceViewTextBox.CaretIndex;
                var text = SourceViewTextBox.Text ?? string.Empty;

                // Calculate line and column
                var lineNumber = 1;
                var columnNumber = 1;

                for (int i = 0; i < Math.Min(caretIndex, text.Length); i++)
                {
                    if (text[i] == '\n')
                    {
                        lineNumber++;
                        columnNumber = 1;
                    }
                    else if (text[i] != '\r') // Skip carriage return
                    {
                        columnNumber++;
                    }
                }

                LineAndColumnInfo = string.Format(Se.Language.General.LineXColumnY, lineNumber, columnNumber);
            });
        };
    }

    internal void Initialize(string title, string text, SubtitleFormat subtitleFormat)
    {
        Title = title;
        Text = text;
        _subtitleFormat = subtitleFormat;
        _cursorTimer.Start();

        var  useSimpleTextBox = text.Length > 2_000_000;
        if (useSimpleTextBox)
        {
            SourceViewTextBox = CreateSimpleTextBoxWrapper();
        }
        else
        {
            SourceViewTextBox = CreateAdvancedTextBoxWrapper();
        }

        TextBoxContainer.Child = SourceViewTextBox.ContentControl;

        Dispatcher.UIThread.Post(() =>
        {
            Task.Delay(100).Wait(); // Slight delay to ensure control is ready  
            SourceViewTextBox.Focus();
            SourceViewTextBox.CaretIndex = 0;
        }, DispatcherPriority.Input);
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

    private TextEditorWrapper CreateAdvancedTextBoxWrapper()
    {
        var textBox = new TextEditor
        {
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ShowLineNumbers = true,
            WordWrap = true,
        };

        // Add syntax highlighting for subtitle source formats
        textBox.TextArea.TextView.LineTransformers.Add(new SubtitleSourceSyntaxHighlighting());

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

    [RelayCommand]
    private async Task Ok()
    {
        if (Window == null)
        {
            return;
        }

        var text = Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            OkPressed = false;
            Window?.Close();
            return;
        }

        var lines = Text.SplitToLines();
        var subtitle = new Subtitle();
        _subtitleFormat.LoadSubtitle(subtitle, lines, string.Empty);
        if (subtitle.Paragraphs.Count > 0)
        {
            Subtitle.Paragraphs.Clear();
            Subtitle.Paragraphs.AddRange(subtitle.Paragraphs);
            OkPressed = true;
            Window?.Close();
            return;
        }

        subtitle = Subtitle.Parse(lines, ".srt");
        if (subtitle.Paragraphs.Count > 0)
        {
            Subtitle.Paragraphs.Clear();
            Subtitle.Paragraphs.AddRange(subtitle.Paragraphs);
            OkPressed = true;
            Window?.Close();
            return;
        }

        await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.General.NoSubtitlesFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
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