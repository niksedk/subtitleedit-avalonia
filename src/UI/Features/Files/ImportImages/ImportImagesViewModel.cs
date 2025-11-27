using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public partial class ImportImagesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ImportImageItem> _images;
    [ObservableProperty] private ImportImageItem? _selectedImage;
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
        "*.jpeg" ,
        "*.gif" ,
        "*.bmp" ,
    };

    public ImportImagesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Images = new ObservableCollection<ImportImageItem>();
        _fileName = string.Empty;
        Header = string.Empty;
        Footer = string.Empty;
        _subtitle = new Subtitle();
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
    private async Task FileImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.Images, _imageExtensions, string.Empty, new List<string>());
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var importImageItem = new ImportImageItem(fileName);
            Images.Add(importImageItem);
        }
        if (Images.Count > 0)
        {
            SelectedImage = Images.First();
        }
    }

    [RelayCommand]
    private void AttachmentRemove()
    {
        var selectedStyle = SelectedImage;
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
                var idx = Images.IndexOf(selectedStyle);
                Images.Remove(selectedStyle);
                SelectedImage = null;
                if (Images.Count > 0)
                {
                    if (idx >= Images.Count)
                    {
                        idx = Images.Count - 1;
                    }
                    SelectedImage = Images[idx];
                }
            }
        });
    }

    [RelayCommand]
    private void AttachemntsRemoveAll()
    {
        Images.Clear();
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
    }

    internal void AttachmentsContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteAllVisible = Images.Count > 0;
        IsDeleteVisible = SelectedImage != null;
    }

    internal void AttachmentsDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedImage != null)
        {
            AttachmentRemove();
            e.Handled = true;
        }
    }
}
