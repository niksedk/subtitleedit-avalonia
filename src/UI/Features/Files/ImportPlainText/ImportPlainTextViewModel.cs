using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public partial class ImportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _files;
    [ObservableProperty] private SubtitleLineViewModel? _selectedFile;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;
    [ObservableProperty] private string _plainText;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly List<string> _textExtensions = new List<string>
    {
        "*.txt",
        "*.rtf" ,
    };

    public ImportPlainTextViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        PlainText = string.Empty;
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

        var fileNames = await _fileHelper.PickOpenFiles(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.Images, _textExtensions, string.Empty, new List<string>());
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            
        }        
    }

    [RelayCommand]
    private void ImageRemove()
    {
        //var selectedStyle = SelectedImage;
        //if (selectedStyle == null)
        //{
        //    return;
        //}

        //Dispatcher.UIThread.Post(async void () =>
        //{
        //    var answer = await MessageBox.Show(
        //    Window!,
        //    "Remove image?",
        //    $"Do you want to remove {selectedStyle.FileName}?",
        //    MessageBoxButtons.YesNoCancel,
        //    MessageBoxIcon.Question);

        //    if (answer != MessageBoxResult.Yes)
        //    {
        //        return;
        //    }

        //    if (selectedStyle != null)
        //    {
        //        var idx = Images.IndexOf(selectedStyle);
        //        Images.Remove(selectedStyle);
        //        SelectedImage = null;
        //        if (Images.Count > 0)
        //        {
        //            if (idx >= Images.Count)
        //            {
        //                idx = Images.Count - 1;
        //            }
        //            SelectedImage = Images[idx];
        //        }
        //    }
        //});
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void AttachmentsDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedFile != null)
        {
            ImageRemove();
            e.Handled = true;
        }
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        if (!_textExtensions.Any(x => x.EndsWith(ext)))
                        {
                            continue;
                        }

                        //var importImageItem = new ImportImageItem(path);
                        //Images.Add(importImageItem);
                    }
                }
            });
        }
    }
}
