using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.ShowImage;

public partial class ShowImageViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private Bitmap? _previewImage;

    public Window? Window { get; set; }

    private readonly IFileHelper _fileHelper;

    public ShowImageViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

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

        var fileName = await _fileHelper.PickSaveSubtitleFile(Window!, ".png", "image", Se.Language.General.SaveImageAs);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        PreviewImage.Save(fileName, 100);
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