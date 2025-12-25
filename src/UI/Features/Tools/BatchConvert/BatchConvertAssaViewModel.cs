using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertAssaViewModel : ObservableObject
{
    [ObservableProperty] private bool _useSourceStylesIfPossible;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private Bitmap? _imagePreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;

    public BatchConvertAssaViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;

        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
    }   

    [RelayCommand]
    private void Ok()
    {
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

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
    }
}