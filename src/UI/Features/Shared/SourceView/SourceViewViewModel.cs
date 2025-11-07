using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
    public TextBox SourceViewTextBox { get; set; }

    private readonly System.Timers.Timer _cursorTimer;

    public SourceViewViewModel()
    {
        SourceViewTextBox = new TextBox();
        Title = string.Empty;
        Text = string.Empty;
        LineAndColumnInfo = string.Empty;
        Subtitle = new Subtitle();
        _subtitleFormat = new SubRip();

        _cursorTimer = new System.Timers.Timer(200);
        _cursorTimer.Elapsed += (sender, args) =>
        {
            if (SourceViewTextBox == null)
            {
                return;
            }

            Dispatcher.UIThread.Post(async void () =>
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