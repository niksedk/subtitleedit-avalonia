using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

public partial class AssaStylesViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _textGeneral;
    [ObservableProperty] private string _textMostUsedWords;
    [ObservableProperty] private string _textMostUsedLines;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    public IFileHelper _fileHelper;

    private string _fileName;

    public AssaStylesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        TextGeneral = string.Empty;
        TextMostUsedWords = string.Empty;
        TextMostUsedLines = string.Empty;

        _fileName = string.Empty;
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
    private void Export()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
