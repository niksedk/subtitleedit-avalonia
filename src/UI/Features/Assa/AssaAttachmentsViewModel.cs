using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaAttachmentsViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<AssaAttachmentItem> _attachments;
    [ObservableProperty] private AssaAttachmentItem? _selectedAttachment;
    [ObservableProperty] private string _previewTitle;
    [ObservableProperty] private Bitmap? _previewImage;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }
    public string Footer { get; set; }

    private readonly IFileHelper _fileHelper;
    private string _fileName;
    private Subtitle _subtitle;
    private readonly List<string> _imageExtensions = new List<string>
    {
        "*.png",
        "*.jpg" ,
        "*.gif" ,
        "*.bmp" ,
        "*.ico"
    };

    public AssaAttachmentsViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        Attachments = new ObservableCollection<AssaAttachmentItem>();
        PreviewTitle = string.Empty;
        _fileName = string.Empty;
        Header = string.Empty;
        Footer = string.Empty;
        _subtitle = new Subtitle();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Footer = GetUpdatedFooter();
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task FileAttach()
    {
        var fileNames = await _fileHelper.PickOpenFiles(Window!, "Select attachment", "True type font", ["*.ttf"], Se.Language.Assa.Graphics, _imageExtensions);

        foreach (var fileName in fileNames)
        {
            var attachmentFileName = Path.GetFileName(fileName);
            var attachment = new AssaAttachmentItem(fileName);
            var ext = Path.GetExtension(attachmentFileName)?.ToLowerInvariant();
            Attachments.Add(attachment);
            SelectedAttachment = attachment;
        }
    }

    [RelayCommand]
    private void FileImport()
    {
    }

    [RelayCommand]
    private void FileExport()
    {
    }

    [RelayCommand]
    private void AttachmentRemove()
    {
        var selectedStyle = SelectedAttachment;
        if (selectedStyle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = await MessageBox.Show(
            Window!,
            "Delete attachment?",
            $"Do you want to delete {selectedStyle.FileName}?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            if (selectedStyle != null)
            {
                var idx = Attachments.IndexOf(selectedStyle);
                Attachments.Remove(selectedStyle);
                SelectedAttachment = null;
                if (Attachments.Count > 0)
                {
                    if (idx >= Attachments.Count)
                    {
                        idx = Attachments.Count - 1;
                    }
                    SelectedAttachment = Attachments[idx];
                }
            }
        });
    }

    [RelayCommand]
    private void AttachemntsRemoveAll()
    {
        Attachments.Clear();
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
        Footer = subtitle.Footer ?? string.Empty;
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

        if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        ListAttachments(Footer.SplitToLines() ?? []);

        if (Attachments.Count > 0)
        {
            SelectedAttachment = Attachments[0];
        }

        UpdatePreview();
    }

    private List<AssaAttachmentItem> ListAttachments(List<string> lines)
    {
        var attachments = new List<AssaAttachmentItem>();
        bool attachmentOn = false;
        var attachmentContent = new StringBuilder();
        var attachmentFileName = string.Empty;
        var category = string.Empty;
        foreach (var line in lines)
        {
            var s = line.Trim();
            if (attachmentOn)
            {
                if (s == "[V4+ Styles]" || s == "[V4 Styles]" || s == "[Events]")
                {
                    AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
                    attachmentOn = false;
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                }
                else if (s == string.Empty)
                {
                    AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                }
                else if (s.Equals("[Fonts]", StringComparison.OrdinalIgnoreCase))
                {
                    AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                    category = Se.Language.General.Fonts;
                }
                else if (s.Equals("[Graphics]", StringComparison.OrdinalIgnoreCase))
                {
                    AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
                    attachmentContent = new StringBuilder();
                    attachmentFileName = string.Empty;
                    category = Se.Language.Assa.Graphics;
                }
                else if (s.StartsWith("filename:") || s.StartsWith("fontname:"))
                {
                    AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
                    attachmentContent = new StringBuilder();
                    attachmentFileName = s.Remove(0, 9).Trim();
                }
                else
                {
                    attachmentContent.AppendLine(s);
                }
            }
            else if (s.Equals("[Fonts]", StringComparison.OrdinalIgnoreCase))
            {
                category = Se.Language.General.Fonts;
                attachmentOn = true;
                attachmentContent = new StringBuilder();
                attachmentFileName = string.Empty;
            }
            else if (s.Equals("[Graphics]", StringComparison.OrdinalIgnoreCase))
            {
                category = Se.Language.Assa.Graphics;
                attachmentOn = true;
                attachmentContent = new StringBuilder();
                attachmentFileName = string.Empty;
            }
        }

        AddToListIfNotEmpty(attachmentContent.ToString(), attachmentFileName, category);
        return attachments;
    }

    private string GetUpdatedFooter()
    {
        var sb = new StringBuilder();

        var fonts = Attachments.Where(p => p.Category == Se.Language.General.Fonts).ToList();
        if (fonts.Count > 0)
        {
            sb.AppendLine("[Fonts]");
            foreach (var font in fonts)
            {
                sb.AppendLine("fontname: " + Path.GetFileName(font.FileName));
                sb.AppendLine(font.Content.Trim());
            }
            sb.AppendLine();
        }

        var graphics = Attachments.Where(p => p.Category == Se.Language.Assa.Graphics).ToList();
        if (graphics.Count > 0)
        {
            sb = new StringBuilder(sb.ToString().Trim());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("[Graphics]");
            foreach (var g in graphics)
            {
                sb.AppendLine("filename: " + Path.GetFileName(g.FileName));
                sb.AppendLine(g.Content.Trim());
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimStart();
    }

    private void AddToListIfNotEmpty(string attachmentContent, string attachmentFileName, string category)
    {
        var content = attachmentContent.Trim();
        if (!string.IsNullOrWhiteSpace(attachmentFileName) && !string.IsNullOrEmpty(content))
        {
            var bytes = UUEncoding.UUDecode(content);
            var attachment = new AssaAttachmentItem { FileName = attachmentFileName, Bytes = bytes, Category = category, Content = content };
            Attachments.Add(attachment);
        }
    }

    private void ResetHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        Header = sub.Header;
    }

    private void UpdatePreview()
    {
        var selectedItem = SelectedAttachment;
        if (selectedItem == null)
        {
            PreviewImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        if (selectedItem.Category == Se.Language.General.Fonts)
        {
            ShowFont(selectedItem.Bytes);
        }
        else if (selectedItem.Category == Se.Language.Assa.Graphics)
        {
            ShowImage(selectedItem.Bytes);
        }
        else
        {
            PreviewImage = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
        }
    }

    private void ShowImage(byte[] bytes)
    {
        // Assuming you have width/height properties or can get them from your view
        var previewWidth = 400; // Replace with actual width from your control
        var previewHeight = 300; // Replace with actual height from your control
        if (previewWidth <= 1 || previewHeight <= 1)
        {
            return;
        }
        // Load image using SkiaSharp
        using var skImage = SKImage.FromEncodedData(bytes);
        if (skImage == null)
        {
            return;
        }
        // Set the info text
        PreviewTitle = $"{Se.Language.General.Image}: {skImage.Width}x{skImage.Height}";
        // Dispose previous image
        PreviewImage?.Dispose();
        // Create Skia surface and canvas
        var imageInfo = new SKImageInfo(previewWidth, previewHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        // Clear background
        canvas.Clear(SKColors.Transparent);
        // Calculate scaling to fit the image within the preview area
        float scale = Math.Min((float)previewWidth / skImage.Width, (float)previewHeight / skImage.Height);
        float scaledWidth = skImage.Width * scale;
        float scaledHeight = skImage.Height * scale;
        float x = (previewWidth - scaledWidth) / 2;
        float y = (previewHeight - scaledHeight) / 2;
        // Draw the image centered and scaled
        var destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);
        canvas.DrawImage(skImage, destRect);
        // Convert to Avalonia bitmap
        using var finalImage = surface.Snapshot();
        using var skData = finalImage.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(skData.ToArray());
        PreviewImage = new Bitmap(stream);
    }

    public void ShowFont(byte[] fontBytes)
    {
        var previewWidth = 400; // Replace with actual width from your control
        var previewHeight = 300; // Replace with actual height from your control
        if (previewWidth <= 1 || previewHeight <= 1)
        {
            return;
        }
        using var skTypeface = SKTypeface.FromData(SKData.CreateCopy(fontBytes));
        if (skTypeface == null)
        {
            return;
        }
        PreviewTitle = $"{Se.Language.General.FontName}: {skTypeface.FamilyName}";
        PreviewImage?.Dispose();

        // Create Skia surface and canvas
        var imageInfo = new SKImageInfo(previewWidth, previewHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        // Clear background
        canvas.Clear(SKColors.Transparent);

        // Create SKFont and SKPaint (modern approach)
        using var font = new SKFont(skTypeface, 25);
        using var paint = new SKPaint
        {
            Color = SKColors.Orange,
            IsAntialias = true
        };

        // Draw the text
        var text = $"{skTypeface.FamilyName}\n\n{Configuration.Settings.Tools.AssaAttachmentFontTextPreview}";
        var lines = text.SplitToLines() ?? [];
        float y = 23;
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                canvas.DrawText(line, 12, y, font, paint);
            }
            y += font.Size + 5; // Line spacing using font.Size instead of paint.TextSize
        }

        // Convert to Avalonia bitmap
        using var skImage = surface.Snapshot();
        using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(skData.ToArray());
        PreviewImage = new Bitmap(stream);
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void DataGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }

    internal void AttachmentsContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteAllVisible = Attachments.Count > 0;
        IsDeleteVisible = SelectedAttachment != null;
    }

    internal void AttachmentsDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedAttachment != null)
        {
            AttachmentRemove();
            e.Handled = true;
        }
    }
}
