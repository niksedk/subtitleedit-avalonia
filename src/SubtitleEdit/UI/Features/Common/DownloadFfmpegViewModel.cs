using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Common;

public partial class DownloadFfmpegViewModel :ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    
    public DownloadFfmpegWindow? Window { get; set; }

    public DownloadFfmpegViewModel()
    {
        
    }
    
    [RelayCommand]                   
    private void CommandCancel() 
    {
        Window?.Close();
    }
}