using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        DataContext = vm;
        vm.Window = this;
        
        var treeDataGrid = new TreeDataGrid();
        treeDataGrid.DataContext = vm;
        Content = treeDataGrid;
        treeDataGrid.Source = vm.TranslateRowSource;
    }
}
