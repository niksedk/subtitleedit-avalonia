using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.Export;

public class ExportEbuStlWindow : Window
{
    private readonly ExportEbuStlViewModel _vm;

    public ExportEbuStlWindow(ExportEbuStlViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Export EBU STL";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var tabControl = new TabControl();
        var tabGeneral = new TabItem
        {
            Header = Se.Language.General.General,
            Content = MakeGeneralView(vm),
        };
        tabControl.Items.Add(tabGeneral);

        var tabTextAndTiming = new TabItem
        {
            Header = Se.Language.File.EbuSaveOptions.TextAndTimingInformation,
            Content = MakeTextAndTimingView(vm),
        };
        tabControl.Items.Add(tabTextAndTiming);

        var tabErrors = new TabItem
        {
            Header = Se.Language.File.EbuSaveOptions.Errors,
            Content = MakeErrorsView(vm),
        };
        tabControl.Items.Add(tabErrors);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar( buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tabControl, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeGeneralView(ExportEbuStlViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };


        var textBoxWidth = 250;

        var labelCodePageNumber = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.CodePageNumber);
        var comboBoxCodeNumbers = UiUtil.MakeComboBox(vm.CodePages, vm, nameof(vm.SelectedCodePage)).WithMinWidth(textBoxWidth);

        var labelDiskFormatCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DiskFormatCode);
        var comboboxDiskFormatCodes = UiUtil.MakeComboBox(vm.DiskFormatCodes, vm, nameof(vm.SelectedCodePage)).WithMinWidth(textBoxWidth);

        var labelFrameRate = UiUtil.MakeLabel(Se.Language.General.FrameRate);
        var comboBoxFrameRates = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)).WithMinWidth(textBoxWidth);

        var labelDisplayStandardCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DisplayStandardCode);
        var comboBoxDisplayStandardCodes = UiUtil.MakeComboBox(vm.DisplayStandardCodes, vm, nameof(vm.SelectedDisplayStandardCode)).WithMinWidth(textBoxWidth);

        var labelCharacterTable = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.CharacterCodeTable);
        var comboBoxCharacterTables = UiUtil.MakeComboBox(vm.CharacterTables, vm, nameof(vm.SelectedCharacterTable)).WithMinWidth(textBoxWidth);

        var labelLanguageCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.LanguageCode);
        var comboBoxLanguageCodes = UiUtil.MakeComboBox(vm.LanguageCodes, vm, nameof(vm.SelectedLanguageCode)).WithMinWidth(textBoxWidth);

        var labelOriginalProgramTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.OriginalProgramTitle);
        var textBoxOriginalProgramTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.OriginalProgramTitle));

        var labelOriginalEpisodeTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.OriginalEpisodeTitle);
        var textBoxOriginalEpisodeTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.OriginalEpisodeTitle));


        grid.Add(labelCodePageNumber, 0, 0);
        grid.Add(comboBoxCodeNumbers, 0, 1);

        grid.Add(labelDiskFormatCode, 1, 0);
        grid.Add(comboboxDiskFormatCodes, 1, 1);

        grid.Add(labelFrameRate, 2, 0);
        grid.Add(comboBoxFrameRates, 2, 1);

        grid.Add(labelDisplayStandardCode, 3, 0);
        grid.Add(comboBoxDisplayStandardCodes, 3, 1);

        grid.Add(labelCharacterTable, 4, 0);
        grid.Add(comboBoxCharacterTables, 4, 1);

        grid.Add(labelLanguageCode, 5, 0);
        grid.Add(comboBoxLanguageCodes, 5, 1);

        grid.Add(labelOriginalProgramTitle, 6, 0);
        grid.Add(textBoxOriginalProgramTitle, 6, 1);

        grid.Add(labelOriginalEpisodeTitle, 7, 0);
        grid.Add(textBoxOriginalEpisodeTitle, 7, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private object MakeTextAndTimingView(ExportEbuStlViewModel vm)
    {
        var label = UiUtil.MakeLabel("Text and timing");
        return UiUtil.MakeBorderForControl(label);
    }
    private Border MakeErrorsView(ExportEbuStlViewModel vm)
    {
        var label = UiUtil.MakeLabel("Errors");
        return UiUtil.MakeBorderForControl(label);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
