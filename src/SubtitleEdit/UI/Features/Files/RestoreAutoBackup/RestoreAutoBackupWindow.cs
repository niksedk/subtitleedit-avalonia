using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;

public class RestoreAutoBackupWindow : Window
{
    private RestoreAutoBackupViewModel _vm;

    public RestoreAutoBackupWindow(RestoreAutoBackupViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Restore auto-backup...";
        Width = 810;
        Height = 640;
        MinWidth = 800;
        MinHeight = 600;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var dataGrid = new DataGrid
        {
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            ItemsSource = _vm.Files,
            SelectionMode = DataGridSelectionMode.Single,
            IsReadOnly = true,
            AutoGenerateColumns = false,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "Date and Time",
                    Binding = new Binding(nameof(DisplayFile.DateAndTime))
                },
                new DataGridTextColumn
                {
                    Header = "File Name",
                    Binding = new Binding(nameof(DisplayFile.FileName))
                },
                new DataGridTextColumn
                {
                    Header = "Extension",
                    Binding = new Binding(nameof(DisplayFile.Extension))
                },
                new DataGridTextColumn
                {
                    Header = "Size",
                    Binding = new Binding(nameof(DisplayFile.Size))
                }
            }
        };

        dataGrid.SelectionChanged += vm.DataGridSelectionChanged;

        var linkOpenFolder = UiUtil.MakeLink("Open auto-backup folder", vm.OpenFolderCommand);

        var buttonRestore = UiUtil.MakeButton("Restore auto-backup file", vm.RestoreFileCommand);
        buttonRestore.BindIsEnabled(vm, nameof(vm.IsOkButtonEnabled));
        var buttonOk = UiUtil.MakeButtonOk(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonRestore, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Children.Add(dataGrid);
        Grid.SetRow(dataGrid, 0);
        Grid.SetColumn(dataGrid, 0);
        Grid.SetColumnSpan(dataGrid, 2);

        grid.Children.Add(linkOpenFolder);
        Grid.SetRow(linkOpenFolder, 1);
        Grid.SetColumn(linkOpenFolder, 0);

        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);
        Grid.SetColumn(panelButtons, 0);
        Grid.SetColumnSpan(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
