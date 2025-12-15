using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.SetText;

public class SetTextWindow : Window
{
    private readonly SetTextViewModel _vm;

    public SetTextWindow(SetTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Set Text";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = true;
        MinWidth = 600;
        MinHeight = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var textView = MakeTextView(vm);
        var fontView = MakeFontView(vm);
        var previewView = MakePreviewView(vm);
        var buttonPanel = MakeButtonPanel(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Text input
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Font settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Preview
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        grid.Add(textView, 0, 0);
        grid.Add(fontView, 0, 1);
        grid.Add(previewView, 0, 2);
        grid.Add(buttonPanel, 0, 3);

        Content = grid;

        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand.Execute(null);
                e.Handled = true;
            }
        };
    }

    private static Border MakeTextView(SetTextViewModel vm)
    {
        var labelText = UiUtil.MakeLabel("Text:");
        var textBoxText = new TextBox
        {
            MinWidth = 550,
            MinHeight = 100,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
        };
        textBoxText.Bind(TextBox.TextProperty, new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay });

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelText,
                textBoxText,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }

    private static Border MakeFontView(SetTextViewModel vm)
    {
        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName))
            .WithMinWidth(200);

        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.Size);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownInt(8, 200, 48, 100, vm, nameof(vm.FontSize));

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.FontIsBold));

        var labelFontColor = UiUtil.MakeLabel(Se.Language.General.TextColor);
        var colorPickerFontColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontColor));

        var labelOutlineColor = UiUtil.MakeLabel(Se.Language.General.OutlineColor);
        var colorPickerOutlineColor = UiUtil.MakeColorPicker(vm, nameof(vm.OutlineColor));

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.Width);
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 100, vm, nameof(vm.OutlineWidth));

        var labelShadowColor = UiUtil.MakeLabel(Se.Language.General.Shadow);
        var colorPickerShadowColor = UiUtil.MakeColorPicker(vm, nameof(vm.ShadowColor));

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.Width);
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 100, vm, nameof(vm.ShadowWidth));

        var labelBoxType = UiUtil.MakeLabel("Box type:");
        var comboBoxBoxType = UiUtil.MakeComboBox(vm.BoxTypes, vm, nameof(vm.SelectedBoxType))
            .WithMinWidth(150);

        var labelBackgroundColor = UiUtil.MakeLabel("Background color:");
        var colorPickerBackgroundColor = UiUtil.MakeColorPicker(vm, nameof(vm.BackgroundColor));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(5),
        };

        // Row 0: Font name and size
        grid.Add(labelFontName, 0, 0);
        grid.Add(comboBoxFontName, 1, 0);
        grid.Add(labelFontSize, 2, 0);
        grid.Add(numericUpDownFontSize, 3, 0);

        // Row 1: Bold and font color
        grid.Add(checkBoxBold, 0, 1, 2, 1);
        grid.Add(labelFontColor, 2, 1);
        grid.Add(colorPickerFontColor, 3, 1);

        // Row 2: Outline color and width
        grid.Add(labelOutlineColor, 0, 2);
        grid.Add(colorPickerOutlineColor, 1, 2);
        grid.Add(labelOutlineWidth, 2, 2);
        grid.Add(numericUpDownOutlineWidth, 3, 2);

        // Row 3: Shadow color and width
        grid.Add(labelShadowColor, 0, 3);
        grid.Add(colorPickerShadowColor, 1, 3);
        grid.Add(labelShadowWidth, 2, 3);
        grid.Add(numericUpDownShadowWidth, 3, 3);

        // Row 4: Box type and background color
        grid.Add(labelBoxType, 0, 4);
        grid.Add(comboBoxBoxType, 1, 4);
        grid.Add(labelBackgroundColor, 2, 4);
        grid.Add(colorPickerBackgroundColor, 3, 4);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakePreviewView(SetTextViewModel vm)
    {
        var image = new Image
        {
            Stretch = Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.PreviewBitmap)));

        var scrollViewer = new ScrollViewer
        {
            Content = image,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = 200,
        };

        return UiUtil.MakeBorderForControl(scrollViewer);
    }

    private static Panel MakeButtonPanel(SetTextViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return UiUtil.MakeButtonBar(buttonOk, buttonCancel);
    }
}

