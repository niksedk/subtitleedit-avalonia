using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public class ModifySelectionWindow : Window
{
    public ModifySelectionWindow(ModifySelectionViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Edit.ModifySelection.Title;
        CanResize = true;
        Width = 800;
        Height = 500;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeRulesView(vm), 0);
        grid.Add(MakeSelectionView(vm), 0, 1);
        grid.Add(MakeSubtitleView(vm), 1, 0, 1, 2);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private Control MakeRulesView(ModifySelectionViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var comboBoxRules = UiUtil.MakeComboBox(vm.Rules, vm, nameof(vm.SelectedRule));
        var textBoxRuleText = UiUtil.MakeTextBox(200, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.Text));
        var checkBoxRuleCaseSensitive = UiUtil.MakeCheckBox(Se.Language.General.CaseSensitive, vm, nameof(vm.SelectedRule) + "." + nameof(vm.SelectedRule.MatchCase));

        grid.Add(comboBoxRules, 0, 0);
        grid.Add(textBoxRuleText, 0, 1);
        grid.Add(checkBoxRuleCaseSensitive, 1, 0, 1, 2);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Control MakeSelectionView(ModifySelectionViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionAdd, vm, nameof(vm.SelectionNew), "selection"), 0);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionAdd, vm, nameof(vm.SelectionAdd), "selection"), 1);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionSubtract, vm, nameof(vm.SelectionSubtract), "selection"), 2);
        grid.Add(UiUtil.MakeRadioButton(Se.Language.Edit.ModifySelection.SelectionIntersect, vm, nameof(vm.SelectionIntersect), "selection"), 3);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Control MakeSubtitleView(ModifySelectionViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            DataContext = vm,
            ItemsSource = vm.Subtitles,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = Se.Language.General.NumberSymbol,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(PreviewItem.Number)),
                    IsReadOnly = true,
                },
                new DataGridTextColumn
                {
                    Header = Se.Language.General.Show,
                    CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
                    Binding = new Binding(nameof(PreviewItem.Text)),
                    IsReadOnly = true,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        };

        return UiUtil.MakeBorderForControlNoPadding(dataGrid);
    }
}
