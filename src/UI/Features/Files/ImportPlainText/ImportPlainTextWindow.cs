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

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public class ImportPlainTextWindow : Window
{
    public ImportPlainTextWindow(ImportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Import.TitleImportPlainText;
        CanResize = true;
        Width = 1100;
        Height = 800;
        MinWidth = 800;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(3, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 8,
        };

        // Top Region: Import plain text + Options
        var topGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(320, GridUnitType.Pixel) },
            },
            ColumnSpacing = 12,
        };

        topGrid.Children.Add(MakeImportPlainTextGroup(vm));
        Grid.SetColumn(topGrid.Children.Last(), 0);
        topGrid.Children.Add(MakeOptionsGroup(vm));
        Grid.SetColumn(topGrid.Children.Last(), 1);

        mainGrid.Children.Add(topGrid);
        Grid.SetRow(topGrid, 0);

        // Middle Region: Preview
        mainGrid.Children.Add(MakePreviewGroup(vm));
        Grid.SetRow(mainGrid.Children.Last(), 1);

        // Bottom Region: Buttons
        var buttonRefresh = UiUtil.MakeButton(Se.Language.General.Refresh, vm.RefreshCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonRefresh, buttonOk, buttonCancel);
        mainGrid.Add(panelButtons, 2, 0);

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

        var checkBoxMultiple = UiUtil.MakeCheckBox("Multiple files - one file is one subtitle", vm, nameof(vm.MultipleFilesOneFileIsOneSubtitle));
        var buttonOpen = UiUtil.MakeButton("Open text file...", vm.FileImportCommand);
        var panelTop = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
            }
        };
        panelTop.Add(checkBoxMultiple, 0, 0);
        panelTop.Add(buttonOpen, 0, 2);

        var labelEncoding = UiUtil.MakeLabel("Encoding");
        var comboEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.SelectedEncoding)).WithMinWidth(150);
        var buttonEncodingExt = UiUtil.MakeButton(".."); // Place holder for encoding picker
        var panelEncoding = UiUtil.MakeHorizontalPanel(labelEncoding, comboEncoding, buttonEncodingExt);

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PlainText)) { Mode = BindingMode.TwoWay });

        grid.Add(panelTop, 0, 0);
        grid.Add(panelEncoding, 1, 0);
        grid.Add(textBox, 2, 0);

        return MakeGroupBox("Import plain text", grid);
    }

    private static Control MakeOptionsGroup(ImportPlainTextViewModel vm)
    {
        var stack = new StackPanel { Spacing = 10 };

        // Splitting
        var splittingStack = new StackPanel { Spacing = 5 };
        splittingStack.Children.Add(UiUtil.MakeRadioButton("Auto split text", vm, nameof(vm.IsAutoSplitText), "SplitGroup"));
        splittingStack.Children.Add(UiUtil.MakeRadioButton("Split at blank lines", vm, nameof(vm.IsSplitAtBlankLines), "SplitGroup"));
        splittingStack.Children.Add(UiUtil.MakeComboBox(vm.SplitAtOptions, vm, nameof(vm.SelectedSplitAtOption)).WithHorizontalAlignmentStretch());

        var lineBreakGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } },
            ColumnSpacing = 5,
        };
        lineBreakGrid.Add(UiUtil.MakeLabel("Line break"), 0, 0);
        var comboLineBreak = UiUtil.MakeComboBox(vm.LineBreaks, vm, nameof(vm.SelectedLineBreak)).WithHorizontalAlignmentStretch();
        comboLineBreak.IsEditable = true;
        comboLineBreak.Bind(ComboBox.TextProperty, new Binding(nameof(vm.SelectedLineBreak)) { Mode = BindingMode.TwoWay });
        lineBreakGrid.Add(comboLineBreak, 0, 1);
        splittingStack.Children.Add(lineBreakGrid);

        stack.Children.Add(MakeGroupBox("Splitting", splittingStack));

        // Settings
        var settingsGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
            RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition() },
            RowSpacing = 5,
            ColumnSpacing = 5,
        };

        settingsGrid.Add(UiUtil.MakeLabel("Max. number of lines"), 0, 0);
        settingsGrid.Add(UiUtil.MakeNumericUpDownInt(1, 10, 2, 90, vm, nameof(vm.MaxNumberOfLines)), 0, 1);

        settingsGrid.Add(UiUtil.MakeLabel("Single line max. length"), 1, 0);
        settingsGrid.Add(UiUtil.MakeNumericUpDownInt(1, 500, 42, 90, vm, nameof(vm.SingleLineMaxLength)), 1, 1);

        settingsGrid.Add(UiUtil.MakeCheckBox("Split at blank lines", vm, nameof(vm.SplitAtBlankLinesSetting)), 2, 0, 1, 2);
        settingsGrid.Add(UiUtil.MakeCheckBox("Remove lines without letters", vm, nameof(vm.RemoveLinesWithoutLetters)), 3, 0, 1, 2);
        
        var endCharsGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } },
            ColumnSpacing = 5,
        };
        endCharsGrid.Add(UiUtil.MakeCheckBox("Split at end chars", vm, nameof(vm.SplitAtEndCharsSetting)), 0, 0);
        endCharsGrid.Add(UiUtil.MakeTextBox(60, vm, nameof(vm.EndChars)).WithHorizontalAlignmentStretch(), 0, 1);
        settingsGrid.Add(endCharsGrid, 4, 0, 1, 2);

        stack.Children.Add(MakeGroupBox("Settings", settingsGrid));

        // Time codes
        var timeGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
            RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition() },
            RowSpacing = 5,
        };

        timeGrid.Add(UiUtil.MakeCheckBox("Generate time codes", vm, nameof(vm.GenerateTimeCodes)), 0, 0, 1, 2);
        timeGrid.Add(UiUtil.MakeCheckBox("Take time from current file", vm, nameof(vm.TakeTimeFromCurrentFile)), 1, 0, 1, 2);
        timeGrid.Add(UiUtil.MakeLabel("Gap between subtitles (ms)"), 2, 0);
        timeGrid.Add(UiUtil.MakeNumericUpDownInt(0, 5000, 90, 80, vm, nameof(vm.GapBetweenSubtitles)), 2, 1);

        var durStack = new StackPanel { Spacing = 2 };
        durStack.Children.Add(UiUtil.MakeRadioButton("Auto", vm, nameof(vm.IsAutoDuration), "DurGroup"));
        var panelFixedDur = UiUtil.MakeHorizontalPanel(UiUtil.MakeRadioButton("Fixed", vm, nameof(vm.IsFixedDuration), "DurGroup"), 
            UiUtil.MakeNumericUpDownInt(100, 50000, 2500, 80, vm, nameof(vm.FixedDuration)).WithBindEnabled(nameof(vm.IsFixedDuration)));
        durStack.Children.Add(panelFixedDur);

        stack.Children.Add(MakeGroupBox("Time codes", timeGrid));
        stack.Children.Add(MakeGroupBox("Duration", durStack));

        var scrollViewer = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        return MakeGroupBox("Import options", scrollViewer);
    }

    private static Control MakePreviewGroup(ImportPlainTextViewModel vm)
    {
        var fullTimeConverter = new TimeSpanToDisplayFullConverter();
        var shortTimeConverter = new TimeSpanToDisplayShortConverter();
        
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Single,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)));
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Binding = new Binding(nameof(SubtitleLineViewModel.Number)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Binding = new Binding(nameof(SubtitleLineViewModel.StartTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            Binding = new Binding(nameof(SubtitleLineViewModel.EndTime)) { Converter = fullTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Duration,
            Binding = new Binding(nameof(SubtitleLineViewModel.Duration)) { Converter = shortTimeConverter, Mode = BindingMode.OneWay },
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
        });
        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            Binding = new Binding(nameof(SubtitleLineViewModel.Text)),
            IsReadOnly = true,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
        });

        var border = UiUtil.MakeBorderForControlNoPadding(dataGrid);
        var groupBox = MakeGroupBox("Preview", border);
        var label = (groupBox as Grid)!.Children[0] as Label;
        label!.Bind(Label.ContentProperty, new Binding(nameof(vm.PreviewSubtitlesModifiedText)));

        return groupBox;
    }

    private static Grid MakeGroupBox(string title, Control content)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star },
            }
        };
        var label = UiUtil.MakeLabel(title).WithBold().WithMarginBottom(2);
        grid.Children.Add(label);
        grid.Add(content, 1, 0);
        return grid;
    }
}
