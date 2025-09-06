using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

public partial class JoinSubtitlesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<JoinDisplayItem> _joinItems;
    [ObservableProperty] private JoinDisplayItem? _selectedJoinItem;
    [ObservableProperty] private bool _keepTimeCodes;
    [ObservableProperty] private bool _appendTimeCodes;
    [ObservableProperty] private int _appendTimeCodesAddMilliseconds;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;

    public JoinSubtitlesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        JoinItems = new ObservableCollection<JoinDisplayItem>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        KeepTimeCodes = Se.Settings.Tools.JoinKeepTimeCodes;
        AppendTimeCodes = !KeepTimeCodes;
        AppendTimeCodesAddMilliseconds = Se.Settings.Tools.JoinAppendMilliseconds;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.JoinKeepTimeCodes = KeepTimeCodes;
        Se.Settings.Tools.JoinAppendMilliseconds = AppendTimeCodesAddMilliseconds;

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Add()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenSubtitleFiles(Window, Se.Language.General.OpenSubtitleFileTitle, false);
        if (fileNames.Count() == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {
            var subtitle = Subtitle.Parse(fileName);
            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, "Unable to read subtitle from file: " + fileName);
                continue;
            }

            var item = new JoinDisplayItem
            {
                FileName = System.IO.Path.GetFileName(fileName),
                StartTime = subtitle.Paragraphs.Min(p => p.StartTime.TimeSpan),
                EndTime = subtitle.Paragraphs.Max(p => p.EndTime.TimeSpan),
                Lines = subtitle.Paragraphs.Count,
            };
            JoinItems.Add(item);
        }
    }

    [RelayCommand]
    private void Remove()
    {
        var selected = SelectedJoinItem;
        if (selected != null)
        {
            var index = JoinItems.IndexOf(selected);
            JoinItems.Remove(selected);
            if (JoinItems.Count > 0)
            {
                if (index >= JoinItems.Count)
                {
                    index = JoinItems.Count - 1;
                }

                SelectedJoinItem = JoinItems[index];
            }
        }
    }

    [RelayCommand]
    private void Clear()
    {
        JoinItems.Clear();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
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