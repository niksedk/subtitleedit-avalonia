using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public class EditCustomTextFormatWindow : Window
{
    private readonly EditCustomTextFormatViewModel _vm;

    public EditCustomTextFormatWindow(EditCustomTextFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportCustomFormat;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 700;
        MinHeight = 500;
        Bind(Window.TitleProperty, new Binding(nameof(vm.Title)));  
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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

        grid.Add(MakeFormatsView(vm), 0);
        grid.Add(MakePreviewView(vm), 0, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private static Grid MakeFormatsView(EditCustomTextFormatViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        var labelName = UiUtil.MakeLabel(Se.Language.General.Name).WithMinWidth(100); 
        var textBoxName = UiUtil.MakeTextBox(200, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Name));
        var panelName = UiUtil.MakeHorizontalPanel(labelName, textBoxName);
        grid.Add(panelName, 0);

        var labelFileExtension = UiUtil.MakeLabel(Se.Language.General.FileExtension).WithMinWidth(100);
        var textBoxFileExtension = UiUtil.MakeTextBox(200, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.Extension));
        var panelFileExtension = UiUtil.MakeHorizontalPanel(labelFileExtension, textBoxFileExtension).WithMarginTop(5);
        grid.Add(panelFileExtension, 1);

        var labelHeader = UiUtil.MakeLabel(Se.Language.General.Header).WithMarginTop(5);
        var textBoxHeader = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatHeader));
        textBoxHeader.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxHeader.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxHeader.AcceptsReturn = true; 
        grid.Add(labelHeader, 2);
        grid.Add(textBoxHeader, 3);

        var labelText = UiUtil.MakeLabel(Se.Language.General.Text).WithMarginTop(5);
        var textBoxText = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatParagraph));
        textBoxText.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxText.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxText.AcceptsReturn = true;
        grid.Add(labelText, 4);
        grid.Add(textBoxText, 5);

        var labelFooter = UiUtil.MakeLabel(Se.Language.General.Footer).WithMarginTop(5);
        var textBoxFooter = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SelectedCustomFormat) + "." + nameof(CustomFormatItem.FormatFooter));
        textBoxFooter.HorizontalAlignment = HorizontalAlignment.Stretch;
        textBoxFooter.VerticalAlignment = VerticalAlignment.Stretch;
        textBoxFooter.AcceptsReturn = true;
        grid.Add(labelFooter, 6);
        grid.Add(textBoxFooter, 7);

        return grid;
    }

    private static Grid MakePreviewView(EditCustomTextFormatViewModel vm)
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
