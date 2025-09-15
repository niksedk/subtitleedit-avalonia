using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using SkiaSharp;

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