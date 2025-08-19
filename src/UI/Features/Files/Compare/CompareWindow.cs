using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Files.Compare;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;

public class CompareWindow : Window
{
    public CompareWindow(CompareViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.File.Compare;
        Width = 1200;
        Height = 600;
        CanResize = true;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star), // subtitle views
                new RowDefinition(GridLength.Auto), // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10
        };

        // left (original)
        var leftView = MakeSubtitlesView(vm.LeftSubtitles, nameof(vm.SelectedLeft));
        grid.Children.Add(leftView);
        Grid.SetRow(leftView, 0);
        Grid.SetColumn(leftView, 0);

        // right (modified)
        var rightView = MakeSubtitlesView(vm.RightSubtitles, nameof(vm.SelectedRight));
        grid.Children.Add(rightView);
        Grid.SetRow(rightView, 0);
        Grid.SetColumn(rightView, 1);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        grid.Children.Add(panelButtons);
        Grid.SetRow(panelButtons, 1);
        Grid.SetColumnSpan(panelButtons, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeSubtitlesView(ObservableCollection<CompareItem> items, string selectedBinding)
    {
        var dg = new DataGrid
        {
            ItemsSource = items,
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Height = double.NaN,
            Margin = new Thickness(2)
        };

        // Number column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new DataGridLength(50),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.NumberBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.Number))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // StartTime column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Show,
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.StartTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.StartTime))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // EndTime column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Hide,
            Width = new DataGridLength(120),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.EndTimeBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.EndTime))
                };

                border.Child = textBlock;
                return border;
            })
        });

        // Text column
        dg.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Text,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            CellTheme = UiUtil.DataGridNoBorderCellTheme,
            CellTemplate = new FuncDataTemplate<CompareItem>((item, ns) =>
            {
                var border = new Border
                {
                    Padding = new Thickness(4, 2),
                    [!Border.BackgroundProperty] = new Binding(nameof(CompareItem.TextBackgroundBrush))
                };

                var textBlock = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    [!TextBlock.TextProperty] = new Binding(nameof(CompareItem.Text))
                };

                border.Child = textBlock;
                return border;
            })
        });

        dg.Bind(DataGrid.SelectedItemProperty, new Binding(selectedBinding)
        {
            Mode = BindingMode.TwoWay
        });

        return UiUtil.MakeBorderForControl(dg);
    }

}
