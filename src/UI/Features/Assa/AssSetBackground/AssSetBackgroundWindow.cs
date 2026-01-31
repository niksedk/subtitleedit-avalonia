using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa.AssSetBackground;

public class AssSetBackgroundWindow : Window
{
    public AssSetBackgroundWindow(AssSetBackgroundViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Assa.BackgroundBoxGenerator;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 500;

        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            RowSpacing = 12,
        };

        // Padding settings
        mainGrid.Children.Add(CreatePaddingPanel(vm));
        Grid.SetRow(mainGrid.Children[^1], 0);

        // Fill width settings
        mainGrid.Children.Add(CreateFillWidthPanel(vm));
        Grid.SetRow(mainGrid.Children[^1], 1);

        // Style settings
        mainGrid.Children.Add(CreateStylePanel(vm));
        Grid.SetRow(mainGrid.Children[^1], 2);

        // Colors
        mainGrid.Children.Add(CreateColorsPanel(vm));
        Grid.SetRow(mainGrid.Children[^1], 3);

        // Buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        Grid.SetRow(panelButtons, 4);
        mainGrid.Children.Add(panelButtons);

        Content = mainGrid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += vm.KeyDown;
    }

    private static Border CreatePaddingPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.Assa.BackgroundBoxPadding,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumnSpan(titleLabel, 4);
        grid.Children.Add(titleLabel);

        // Left
        var leftLabel = new TextBlock { Text = Se.Language.General.Left + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(leftLabel, 1);
        Grid.SetColumn(leftLabel, 0);
        grid.Children.Add(leftLabel);

        var leftBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PaddingLeft)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(leftBox, 1);
        Grid.SetColumn(leftBox, 1);
        grid.Children.Add(leftBox);

        // Right
        var rightLabel = new TextBlock { Text = Se.Language.General.Right + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(rightLabel, 1);
        Grid.SetColumn(rightLabel, 2);
        grid.Children.Add(rightLabel);

        var rightBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PaddingRight)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(rightBox, 1);
        Grid.SetColumn(rightBox, 3);
        grid.Children.Add(rightBox);

        // Top
        var topLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarTop + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(topLabel, 2);
        Grid.SetColumn(topLabel, 0);
        grid.Children.Add(topLabel);

        var topBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PaddingTop)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(topBox, 2);
        Grid.SetColumn(topBox, 1);
        grid.Children.Add(topBox);

        // Bottom
        var bottomLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarBottom + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(bottomLabel, 2);
        Grid.SetColumn(bottomLabel, 2);
        grid.Children.Add(bottomLabel);

        var bottomBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.PaddingBottom)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(bottomBox, 2);
        Grid.SetColumn(bottomBox, 3);
        grid.Children.Add(bottomBox);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateFillWidthPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var checkBox = new CheckBox
        {
            Content = Se.Language.Assa.BackgroundBoxFillWidth,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.FillWidth)) { Mode = BindingMode.TwoWay },
            FontWeight = FontWeight.Bold,
        };
        Grid.SetRow(checkBox, 0);
        Grid.SetColumnSpan(checkBox, 4);
        grid.Children.Add(checkBox);

        // Margin Left
        var leftLabel = new TextBlock { Text = Se.Language.General.Left + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(leftLabel, 1);
        Grid.SetColumn(leftLabel, 0);
        grid.Children.Add(leftLabel);

        var leftBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.FillWidthMarginLeft)) { Mode = BindingMode.TwoWay },
            [!NumericUpDown.IsEnabledProperty] = new Binding(nameof(vm.FillWidth)),
        };
        Grid.SetRow(leftBox, 1);
        Grid.SetColumn(leftBox, 1);
        grid.Children.Add(leftBox);

        // Margin Right
        var rightLabel = new TextBlock { Text = Se.Language.General.Right + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(rightLabel, 1);
        Grid.SetColumn(rightLabel, 2);
        grid.Children.Add(rightLabel);

        var rightBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 500,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.FillWidthMarginRight)) { Mode = BindingMode.TwoWay },
            [!NumericUpDown.IsEnabledProperty] = new Binding(nameof(vm.FillWidth)),
        };
        Grid.SetRow(rightBox, 1);
        Grid.SetColumn(rightBox, 3);
        grid.Children.Add(rightBox);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateStylePanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.General.Style,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumnSpan(titleLabel, 2);
        grid.Children.Add(titleLabel);

        // Box style
        var styleLabel = new TextBlock { Text = Se.Language.Assa.ProgressBarStyle + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(styleLabel, 1);
        grid.Children.Add(styleLabel);

        var styleCombo = new ComboBox
        {
            MinWidth = 150,
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.BoxStyles)),
            [!ComboBox.SelectedIndexProperty] = new Binding(nameof(vm.BoxStyleIndex)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(styleCombo, 1);
        Grid.SetColumn(styleCombo, 1);
        grid.Children.Add(styleCombo);

        // Radius (for rounded corners)
        var radiusLabel = new TextBlock { Text = Se.Language.Assa.BackgroundBoxRadius + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(radiusLabel, 2);
        grid.Children.Add(radiusLabel);

        var radiusBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 200,
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Radius)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(radiusBox, 2);
        Grid.SetColumn(radiusBox, 1);
        grid.Children.Add(radiusBox);

        // Outline width
        var outlineLabel = new TextBlock { Text = Se.Language.General.Outline + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(outlineLabel, 3);
        grid.Children.Add(outlineLabel);

        var outlineBox = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 20,
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.OutlineWidth)) { Mode = BindingMode.TwoWay },
        };
        Grid.SetRow(outlineBox, 3);
        Grid.SetColumn(outlineBox, 1);
        grid.Children.Add(outlineBox);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }

    private static Border CreateColorsPanel(AssSetBackgroundViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowSpacing = 8,
            ColumnSpacing = 10,
        };

        var titleLabel = new TextBlock
        {
            Text = Se.Language.General.Color,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 5),
        };
        Grid.SetRow(titleLabel, 0);
        Grid.SetColumnSpan(titleLabel, 2);
        grid.Children.Add(titleLabel);

        // Box color
        var boxLabel = new TextBlock { Text = Se.Language.Assa.BackgroundBoxBoxColor + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(boxLabel, 1);
        grid.Children.Add(boxLabel);

        var boxPicker = UiUtil.MakeColorPicker(vm, nameof(vm.BoxColor));
        boxPicker.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(boxPicker, 1);
        Grid.SetColumn(boxPicker, 1);
        grid.Children.Add(boxPicker);

        // Outline color
        var outlineLabel = new TextBlock { Text = Se.Language.General.Outline + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(outlineLabel, 2);
        grid.Children.Add(outlineLabel);

        var outlinePicker = UiUtil.MakeColorPicker(vm, nameof(vm.OutlineColor));
        outlinePicker.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(outlinePicker, 2);
        Grid.SetColumn(outlinePicker, 1);
        grid.Children.Add(outlinePicker);

        // Shadow color
        var shadowLabel = new TextBlock { Text = Se.Language.General.Shadow + ":", VerticalAlignment = VerticalAlignment.Center };
        Grid.SetRow(shadowLabel, 3);
        grid.Children.Add(shadowLabel);

        var shadowPicker = UiUtil.MakeColorPicker(vm, nameof(vm.ShadowColor));
        shadowPicker.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(shadowPicker, 3);
        Grid.SetColumn(shadowPicker, 1);
        grid.Children.Add(shadowPicker);

        return new Border
        {
            Child = grid,
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
        };
    }
}
