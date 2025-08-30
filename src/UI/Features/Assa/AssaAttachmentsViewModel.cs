using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaAttachmentsViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _attachments;
    [ObservableProperty] private StyleDisplay? _selectedAttachment;
    [ObservableProperty] private string _previewTitle;
    [ObservableProperty] private Bitmap? _previewImage;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }

    private readonly IFileHelper _fileHelper;
    private string _fileName;
    private Subtitle _subtitle;

    public AssaAttachmentsViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        Attachments = new ObservableCollection<StyleDisplay>();
        PreviewTitle = string.Empty;
        _fileName = string.Empty;
        Header = string.Empty;
        _subtitle = new Subtitle();

        LoadSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void FileImport()
    {
    }


    [RelayCommand]
    private void FileExport()
    {
    }


    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
        Title = string.Format(Se.Language.Assa.AttachmentsTitleX, fileName);
        Header = subtitle.Header;
        _subtitle = subtitle;

        if (Header != null && Header.Contains("http://www.w3.org/ns/ttml"))
        {
            var s = new Subtitle { Header = Header };
            AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, Header, AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
            Header = s.Header;
        }
        else if (Header != null && Header.StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            _subtitle = WebVttToAssa.Convert(subtitle, new SsaStyle(), 0, 0);
            Header = _subtitle.Header;
        }

        //if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        //{
        //    ResetHeader();
        //}
    }

    private void SaveSettings()
    {
    }

    private void LoadSettings()
    {
    }

    private void UpdatePreview()
    {
        PreviewImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
