using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.PromptFileSaved;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.BatchErrorList;

public partial class BatchErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<BatchErrorListItem> _subtitles;
    [ObservableProperty] private BatchErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;

    public Window? Window { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public BatchErrorListViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles = new ObservableCollection<BatchErrorListItem>();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var suggestedFileName = "Subtitle-file-errors";
        var fileName = await _fileHelper.PickSaveFile(Window, ".csv" , suggestedFileName, Se.Language.General.Export);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        // export to csv
        //TODO: implement export and save to utf-8 file
        var sb = new StringBuilder();
        foreach (var line in Subtitles)
        { 
        }

        _ = await _windowService.ShowDialogAsync<PromptFileSavedWindow, PromptFileSavedViewModel>(Window,
        vm =>
        {
            vm.Initialize(Se.Language.General.FileSaved,
                string.Format(Se.Language.Tools.BatchConvert.ErrorsExportedX, Subtitles.Count), fileName, true, true);
        });

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

    internal void Initialize(List<BatchConvertItem> batchItems)
    {
        foreach (var batchItem in batchItems)
        {
            if (batchItem.Subtitle == null)
            {
                continue;
            }

            foreach (var p in batchItem.Subtitle.Paragraphs)
            {
                var errorItem = new BatchErrorListItem(batchItem.FileName, new SubtitleLineViewModel(p, batchItem.Subtitle.OriginalFormat ?? new SubRip()));
                if (!string.IsNullOrEmpty(errorItem.Error))
                {
                    Subtitles.Add(errorItem);
                }
            }
        }
       
        HasErrors = Subtitles.Count > 0;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasErrors = SelectedSubtitle != null;
    }
}