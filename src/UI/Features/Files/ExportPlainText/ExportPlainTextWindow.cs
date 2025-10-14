using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportPlainText;

public class ExportPlainTextWindow : Window
{
    private readonly ExportPlainTextViewModel _vm;

    public ExportPlainTextWindow(ExportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportPlainText;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // Encoding section + buttons
        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding).WithBold().WithMarginTop(10);
        var comboBoxEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.SelectedEncoding))
            .WithMinWidth(180)
            .WithMarginRight(10);
        var buttonSaveAs = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveAsCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar( labelEncoding, comboBoxEncoding, buttonSaveAs, buttonDone);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeSettingsView(vm), 0);
        grid.Add(MakePreviewView(vm), 0, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonSaveAs.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }

    private static Grid MakeSettingsView(ExportPlainTextViewModel vm)
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
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        
        grid.Add(UiUtil.MakeLabel(Se.Language.General.Settings), 0);

        // Line numbers section
        var labelLineNumbers = UiUtil.MakeLabel("Line numbers").WithBold().WithMarginTop(10);
        var checkBoxShowLineNumbers = UiUtil.MakeCheckBox("Show line numbers", vm, nameof(vm.ShowLineNumbers));
        checkBoxShowLineNumbers.PropertyChanged += (s, e) => vm.SetDirty();
        var checkBoxAddNewLineAfterLineNumber = UiUtil.MakeCheckBox("Add new line after line number", vm, nameof(vm.AddNewLineAfterLineNumber))
            .WithBindEnabled(nameof(vm.ShowLineNumbers));
        checkBoxAddNewLineAfterLineNumber.PropertyChanged += (s, e) => vm.SetDirty();

        // Time codes section
        var labelTimeCodes = UiUtil.MakeLabel("Time codes").WithBold().WithMarginTop(10);
        var checkBoxShowTimeCodes = UiUtil.MakeCheckBox("Show time codes", vm, nameof(vm.ShowTimeCodes));
        checkBoxShowTimeCodes.PropertyChanged += (s, e) => vm.SetDirty();
        var labelTimeCodeFormat = UiUtil.MakeLabel("Format:");
        var comboBoxTimeCodeFormat = UiUtil.MakeComboBox(vm.TimeCodeFormats, vm, nameof(vm.SelectedTimeCodeFormats))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes));
        comboBoxTimeCodeFormat.PropertyChanged += (_,_) => vm.SetDirty();
        var panelTimeCodeFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelTimeCodeFormat,
                comboBoxTimeCodeFormat,
            }
        };
        var labelTimeCodeSeparator = UiUtil.MakeLabel("Separator:");
        var comboBoxTimeCodeSeparator = UiUtil.MakeComboBox(vm.TimeCodeSeparators, vm, nameof(vm.SelectedTimeCodeSeparator))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes));
        comboBoxTimeCodeSeparator.PropertyChanged += (sender, args) => vm.SetDirty();
        var panelTimeCodeSeparator = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelTimeCodeSeparator,
                comboBoxTimeCodeSeparator,
            }
        };
        var checkBoxAddNewLineAfterTimeCode = UiUtil.MakeCheckBox("Add new line after time code", vm, nameof(vm.AddNewLineAfterTimeCode))
            .WithBindEnabled(nameof(vm.ShowTimeCodes));
        checkBoxAddNewLineAfterTimeCode.PropertyChanged += (sender, args) => vm.SetDirty(); 

        // Format text section
        var labelFormatText = UiUtil.MakeLabel(Se.Language.General.Text).WithBold().WithMarginTop(10);
        var radioButtonFormatTextNone = UiUtil.MakeRadioButton(Se.Language.General.None, vm, nameof(vm.FormatTextNone), "formatText");
        radioButtonFormatTextNone.PropertyChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextMerge = UiUtil.MakeRadioButton("Merge lines", vm, nameof(vm.FormatTextMerge), "formatText");
        radioButtonFormatTextMerge.PropertyChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextUnbreak = UiUtil.MakeRadioButton("Unbreak lines", vm, nameof(vm.FormatTextUnbreak), "formatText");
        radioButtonFormatTextUnbreak.PropertyChanged += (sender, args) => vm.SetDirty();
        var checkBoxRemoveStyling = UiUtil.MakeCheckBox("Remove styling", vm, nameof(vm.TextRemoveStyling));
        checkBoxRemoveStyling.PropertyChanged += (sender, args) => vm.SetDirty();

        // Spacing section
        var labelSpacing = UiUtil.MakeLabel("Spacing").WithBold().WithMarginTop(10);
        var checkBoxAddLineAfterText = UiUtil.MakeCheckBox("Add new line after text", vm, nameof(vm.AddLineAfterText));
        checkBoxAddLineAfterText.PropertyChanged += (sender, args) => vm.SetDirty();
        var checkBoxAddLineBetweenSubtitles = UiUtil.MakeCheckBox("Add line between subtitles", vm, nameof(vm.AddLineBetweenSubtitles));
        checkBoxAddLineBetweenSubtitles.PropertyChanged += (sender, args) => vm.SetDirty();
        
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                // line numbers
                labelLineNumbers,
                checkBoxShowLineNumbers,
                checkBoxAddNewLineAfterLineNumber,

                // time codes
                labelTimeCodes,
                checkBoxShowTimeCodes,
                panelTimeCodeFormat,
                panelTimeCodeSeparator,
                checkBoxAddNewLineAfterTimeCode,

                // format text
                labelFormatText,
                radioButtonFormatTextNone,
                radioButtonFormatTextMerge,
                radioButtonFormatTextUnbreak,
                checkBoxRemoveStyling,
                checkBoxAddLineAfterText,

                // spacing
                labelSpacing,
                checkBoxAddLineBetweenSubtitles,
            },
        };

        grid.Add(UiUtil.MakeBorderForControl(stackPanel), 1);
        
        return grid;
    }

    private static Grid MakePreviewView(ExportPlainTextViewModel vm)
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
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeLabel(Se.Language.General.Preview), 0);
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        grid.Add(textBox, 1);

        return grid;
    }
}
