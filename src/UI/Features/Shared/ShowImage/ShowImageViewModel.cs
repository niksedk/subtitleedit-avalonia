using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

public partial class ShowImageViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private Bitmap? _previewImage;

    public Window? Window { get; set; }

    public ShowImageViewModel()
    {
        Title = string.Empty;
        PreviewImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();   
    }

    internal void Initialize(string title, Bitmap bitmap)
    {
        Title = title;
        PreviewImage = bitmap;
    }


    [RelayCommand]
    private async Task CopyImageToClipboard()
    {
        if (PreviewImage == null || Window == null || Window.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.CopyImageToClipboard(PreviewImage);
    }

    [RelayCommand]
    private async Task SaveImageAs()
    {
        if (PreviewImage == null || Window == null || Window.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.CopyImageToClipboard(PreviewImage);
    }


    [RelayCommand]
    private void Ok()
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