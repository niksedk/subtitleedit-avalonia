using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Avalonia.Controls.Primitives;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public class ImportPlainTextWindow : Window
{
    public ImportPlainTextWindow(ImportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportPlainText;
        CanResize = true;
        Width = 1100;
        Height = 850;
        MinWidth = 800;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1.2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };

        var topGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
        };

        topGrid.Children.Add(MakeImportPlainTextGroup(vm));
        Grid.SetColumn(topGrid.Children.Last(), 0);
        topGrid.Children.Add(MakeOptionsGroup(vm));
        Grid.SetColumn(topGrid.Children.Last(), 1);

        mainGrid.Children.Add(topGrid);
        Grid.SetRow(topGrid, 0);

        mainGrid.Children.Add(MakePreviewGroup(vm));
        Grid.SetRow(mainGrid.Children.Last(), 1);

        var buttonRefresh = UiUtil.MakeButton(Se.Language.General.Refresh, vm.RefreshCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonRefresh, buttonOk, buttonCancel);
        mainGrid.Add(panelButtons, 2, 0, 1, 2);

        Content = mainGrid;
        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }

    private static Control MakeImportPlainTextGroup(ImportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            },
            RowSpacing = 5,
        };

        var checkBoxMultiple = UiUtil.MakeCheckBox(Se.Language.File.Import.MultipleFiles, vm, nameof(vm.MultipleFilesOneFileIsOneSubtitle));
        var buttonOpen = UiUtil.MakeButton("Open text file", vm.FileImportCommand);
        var buttonOpenMultiple = UiUtil.MakeButton("...", vm.FilesImportCommand);

        var panelTop = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            }
        };
        panelTop.Add(checkBoxMultiple, 0, 0);
        panelTop.Add(buttonOpen, 0, 2);
        panelTop.Add(buttonOpenMultiple, 0, 3);

        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding);
        var comboEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.SelectedEncoding)).WithWidth(240);
        var panelEncoding = UiUtil.MakeHorizontalPanel(labelEncoding, comboEncoding);

        var textBox = new TextBox { AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PlainText)) { Mode = BindingMode.TwoWay });
        textBox.Bind(Visual.IsVisibleProperty, new Binding("!MultipleFilesOneFileIsOneSubtitle"));

        var listBoxFiles = new ListBox();
        listBoxFiles.Bind(ItemsControl.ItemsSourceProperty, new Binding(nameof(vm.Files)));
        listBoxFiles.Bind(ListBox.SelectedItemProperty, new Binding(nameof(vm.SelectedFile)));
        listBoxFiles.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.MultipleFilesOneFileIsOneSubtitle)));

        grid.Add(panelTop, 0, 0);
        grid.Add(panelEncoding, 1, 0);
        grid.Add(textBox, 2, 0);
        grid.Add(listBoxFiles, 2, 0);

        return MakeGroupBox(Se.Language.File.Import.TitleImportPlainText, grid);
    }

    private static Control MakeOptionsGroup(ImportPlainTextViewModel vm)
    {
        var stack = new StackPanel { Spacing = 10 };

        var splittingStack = new StackPanel { Spacing = 5 };
        splittingStack.Children.Add(UiUtil.MakeRadioButton("Auto split text", vm, nameof(vm.IsAutoSplitText), "SplitGroup"));
        splittingStack.Children.Add(UiUtil.MakeRadioButton(Se.Language.File.Import.BlankLines, vm, nameof(vm.IsSplitAtBlankLines), "SplitGroup"));
        splittingStack.Children.Add(UiUtil.MakeRadioButton("Line mode", vm, nameof(vm.IsSplitAtLineMode), "SplitGroup"));

        var comboLineMode = UiUtil.MakeComboBox(vm.SplitAtOptions, vm, nameof(vm.SelectedSplitAtOption)).WithHorizontalAlignmentStretch();
        comboLineMode.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsSplitAtLineMode)));
        splittingStack.Children.Add(comboLineMode);

        var lineBreakGrid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } }, ColumnSpacing = 5 };
        lineBreakGrid.Add(UiUtil.MakeLabel("Line break"), 0, 0);
        var comboLineBreak = UiUtil.MakeComboBox(vm.LineBreaks, vm, nameof(vm.SelectedLineBreak)).WithHorizontalAlignmentStretch();
        comboLineBreak.IsEditable = true;
        comboLineBreak.Bind(ComboBox.TextProperty, new Binding(nameof(vm.SelectedLineBreak)) { Mode = BindingMode.TwoWay });
        lineBreakGrid.Add(comboLineBreak, 0, 1);
        splittingStack.Children.Add(lineBreakGrid);

        stack.Children.Add(MakeGroupBox(Se.Language.File.Import.SplitTextAt, splittingStack));

        var settingsGrid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } }, RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition() }, RowSpacing = 5 };
        settingsGrid.Add(UiUtil.MakeLabel("Max. line length"), 0, 0);
        settingsGrid.Add(UiUtil.MakeNumericUpDownInt(1, 500, 42, 110, vm, nameof(vm.SingleLineMaxLength)), 0, 1);
        settingsGrid.Add(UiUtil.MakeCheckBox("Merge short lines", vm, nameof(vm.MergeShortLines)), 1, 0, 1, 2);
        settingsGrid.Add(UiUtil.MakeCheckBox(Se.Language.General.AutoBreak, vm, nameof(vm.AutoBreak)), 2, 0, 1, 2);
        settingsGrid.Add(UiUtil.MakeCheckBox("Remove lines without letters", vm, nameof(vm.RemoveLinesWithoutLetters)), 3, 0, 1, 2);

        var endCharsGrid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } }, ColumnSpacing = 5 };
        endCharsGrid.Add(UiUtil.MakeCheckBox("Split at end chars", vm, nameof(vm.SplitAtEndCharsSetting)), 0, 0);
        endCharsGrid.Add(UiUtil.MakeTextBox(110, vm, nameof(vm.EndChars)), 0, 1);
        settingsGrid.Add(endCharsGrid, 4, 0, 1, 2);

        var settingsGroupBox = MakeGroupBox(Se.Language.General.Settings, settingsGrid);
        settingsGroupBox.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.IsAutoSplitText)));
        stack.Children.Add(settingsGroupBox);

        var timeStack = new StackPanel { Spacing = 2 };
        timeStack.Children.Add(UiUtil.MakeCheckBox(Se.Language.File.Import.ImportTimeCodes.Replace("...", string.Empty), vm, nameof(vm.IsTimeCodeGenerate)));
        var checkTakeTime = UiUtil.MakeCheckBox("Take time from current file", vm, nameof(vm.IsTimeCodeTakeFromCurrent));
        checkTakeTime.Bind(InputElement.IsEnabledProperty, new Binding(nameof(vm.IsTimeCodeGenerate)));
        timeStack.Children.Add(checkTakeTime);

        var gapGrid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } }, ColumnSpacing = 5 };
        gapGrid.Add(UiUtil.MakeLabel($"{Se.Language.General.Gap} (ms)"), 0, 0);
        gapGrid.Add(UiUtil.MakeNumericUpDownInt(0, 5000, 90, 110, vm, nameof(vm.GapBetweenSubtitles)), 0, 1);
        timeStack.Children.Add(gapGrid);

        stack.Children.Add(MakeGroupBox(Se.Language.File.Import.TimeCodesDotDotDot.Replace("...", string.Empty), timeStack));

        var durStack = new StackPanel { Spacing = 2 };
        durStack.Children.Add(UiUtil.MakeRadioButton(Se.Language.General.Auto, vm, nameof(vm.IsAutoDuration), "DurGroup"));
        durStack.Children.Add(UiUtil.MakeHorizontalPanel(UiUtil.MakeRadioButton("Fixed", vm, nameof(vm.IsFixedDuration), "DurGroup"),
            UiUtil.MakeNumericUpDownInt(100, 50000, 2000, 110, vm, nameof(vm.FixedDuration)).WithBindEnabled(nameof(vm.IsFixedDuration))));

        stack.Children.Add(MakeGroupBox(Se.Language.General.Duration, durStack));

        return MakeGroupBox("Import options", new ScrollViewer { Content = stack });
    }

    private static Control MakePreviewGroup(ImportPlainTextViewModel vm)
    {
        var dataGrid = new DataGrid { AutoGenerateColumns = false, SelectionMode = DataGridSelectionMode.Single, CanUserResizeColumns = true };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)));
        dataGrid.Columns.Add(new DataGridTextColumn { Header = "#", Binding = new Binding(nameof(SubtitleLineViewModel.Number)), Width = new DataGridLength(1, DataGridLengthUnitType.Auto) });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = Se.Language.General.Show, Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() }, Width = new DataGridLength(1, DataGridLengthUnitType.Auto) });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = Se.Language.General.Hide, Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = new TimeSpanToDisplayFullConverter() }, Width = new DataGridLength(1, DataGridLengthUnitType.Auto) });
        dataGrid.Columns.Add(new DataGridTextColumn { Header = Se.Language.General.Text, Binding = new Binding(nameof(SubtitleLineViewModel.Text)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        var groupBox = MakeGroupBox(Se.Language.General.Preview, border);
        var label = (groupBox as Grid)!.Children[0] as Label;
        label!.Bind(Label.ContentProperty, new Binding(nameof(vm.PreviewSubtitlesModifiedText)));
        return groupBox;
    }

    private static Grid MakeGroupBox(string title, Control content)
    {
        var grid = new Grid { RowDefinitions = { new RowDefinition { Height = GridLength.Auto }, new RowDefinition { Height = GridLength.Star } } };
        grid.Children.Add(UiUtil.MakeLabel(title).WithBold().WithMarginBottom(2));
        grid.Add(content, 1, 0);
        return grid;
    }
}