using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public class AutoTranslateWindow : Window
{
    public AutoTranslateWindow(AutoTranslateViewModel vm)
    {
        DataContext = vm;
        vm.Window = this;
        
       var sourceLangCombo = new ComboBox
        {
            //Items = vm.SourceLanguages,
            //SelectedItem = vm.SourceLanguage,
           // [!ComboBox.SelectedItemProperty] = new Binding("SourceLanguage"),
            Width = 150
        };

        var targetLangCombo = new ComboBox
        {
           // Items = vm.AvailableLanguages,
            //SelectedItem = vm.TargetLanguage,
           // [!ComboBox.SelectedItemProperty] = new Binding("TargetLanguage"),
            Width = 150
        };

        var topBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "From:" },
                sourceLangCombo,
                new TextBlock { Text = "To:" },
                targetLangCombo
            }
        };

        var treeDataGrid = new TreeDataGrid
        {
            [!TreeDataGrid.SourceProperty] = new Binding("TranslateRowSource"),
            Height = Double.NaN // auto size inside scroll viewer
        };

        var scrollViewer = new ScrollViewer
        {
            Content = treeDataGrid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var okButton = UiUtil.MakeButton("OK", vm.CancelCommand);

        var cancelButton = UiUtil.MakeButton("Cancel", vm.CancelCommand);

        var bottomBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10),
            Spacing = 10,
            Children = { okButton, cancelButton }
        };

        Content = new DockPanel
        {
            Children =
            {
                new DockPanel
                {
                    LastChildFill = true,
                    Children =
                    {
                        topBar, // with { [DockPanel.DockProperty] = Dock.Top },
                        scrollViewer, // with { [DockPanel.DockProperty] = Dock.Top },
                        bottomBar, // with { [DockPanel.DockProperty] = Dock.Bottom }
                    }
                }
            }
        };
    }
}
