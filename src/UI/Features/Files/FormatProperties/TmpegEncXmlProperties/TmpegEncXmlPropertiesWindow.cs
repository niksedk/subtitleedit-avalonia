using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public class TmpegEncXmlPropertiesWindow : Window
{
    public TmpegEncXmlPropertiesWindow(TmpegEncXmlPropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.TmpegEncXmlProperties;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 200;

        var labelFontName = UiUtil.MakeLabel(Se.Language.General.Language).WithMinWidth(labelWidth);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName));
        var panelFontName = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontName,
                comboBoxFontName,
            }
        };

        var labelFontHeight = UiUtil.MakeLabel(Se.Language.General.FontHeight).WithMinWidth(labelWidth);
        var numericUpDownFontHeight = UiUtil.MakeNumericUpDownThreeDecimals(0.04m, 1.0m, 150, vm, nameof(vm.FontHeight));
        numericUpDownFontHeight.Increment = 0.001m;
        var panelFontHeight = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelFontHeight,
                numericUpDownFontHeight,
            }
        };

        var labelCheckBoxIsBold = UiUtil.MakeLabel().WithMinWidth(labelWidth);
        var checkBoxIsBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.IsFontBold));
        var panelCheckBoxIsBold = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelCheckBoxIsBold,
                checkBoxIsBold,
            }
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

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
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelFontName, 0);
        grid.Add(panelFontHeight, 1);
        grid.Add(panelCheckBoxIsBold, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
