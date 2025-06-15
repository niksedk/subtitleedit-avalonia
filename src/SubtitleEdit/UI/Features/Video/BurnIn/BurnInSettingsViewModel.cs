using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Features.Shared;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceFolder;
    [ObservableProperty] private bool _useOutputFolder;
    [ObservableProperty] private string _outputFolder;


    public BurnInSettingsWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public BurnInSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        OutputFolder = string.Empty;
        LoadSettings();
    }

    private void LoadSettings()
    {
        UseSourceFolder = Se.Settings.Video.BurnIn.SaveInSourceFolder; ;
        UseOutputFolder = !UseSourceFolder;
        OutputFolder = Se.Settings.Video.BurnIn.OutputFolder;
        SelectedTargetEncoding = Se.Settings.Tools.BatchConvert.TargetEncoding;
    }

    private void SaveSettings()
    {
        Se.Settings.Video.BurnIn.SaveInSourceFolder = !UseOutputFolder;
        Se.Settings.Video.BurnIn.OutputFolder = OutputFolder;
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (UseOutputFolder && string.IsNullOrWhiteSpace(OutputFolder))
        {
            await MessageBox.Show(Window!, "Error",
                "Please select output folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task BrowseOutputFolder()
    {
        var folder = await _folderHelper.PickFolderAsync(Window!, "Select output folder");
        if (!string.IsNullOrEmpty(folder))
        {
            OutputFolder = folder;
            UseOutputFolder = true;
            UseSourceFolder = false;
        }
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