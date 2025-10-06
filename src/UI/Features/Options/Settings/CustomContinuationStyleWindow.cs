using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class CustomContinuationStyleWindow : Window
{
    public CustomContinuationStyleWindow(CustomContinuationStyleViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.EditContinuationStyleCustom;
        CanResize = true;
        Width = 1100;
        Height = 750;
        MinWidth = 800;
        MinHeight = 700;
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

        grid.Add(MakeControlsGrid(vm), 0, 0);
        grid.Add(MakePreviewView(vm), 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeControlsGrid(CustomContinuationStyleViewModel vm)
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(0, 10, 0, 0),
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelPrefix = UiUtil.MakeLabel(Se.Language.General.Prefix);
        var comboBoxPrefix = UiUtil.MakeComboBox(vm.PreAndSuffixes, vm, nameof(vm.SelectedPrefix));
        comboBoxPrefix.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxPrefixAddSpace = UiUtil.MakeCheckBox(Se.Language.Options.Settings.AddSpace, vm, nameof(vm.SelectedPrefixAddSpace));
        checkBoxPrefixAddSpace.PropertyChanged += (s, e) => vm.StyleChanged();

        var labelSuffix = UiUtil.MakeLabel(Se.Language.General.Suffix);
        var comboBoxSuffix = UiUtil.MakeComboBox(vm.PreAndSuffixes, vm, nameof(vm.SelectedSuffix));
        comboBoxSuffix.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxSuffixProcessIfEndWithComma = UiUtil.MakeCheckBox(Se.Language.Options.Settings.ProcessIfEndsWithComma, vm, nameof(vm.SelectedSuffixesProcessIfEndWithComma));
        checkBoxSuffixProcessIfEndWithComma.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxSuffixAddSpace = UiUtil.MakeCheckBox(Se.Language.Options.Settings.AddSpace, vm, nameof(vm.SelectedSuffixesAddSpace));
        checkBoxSuffixAddSpace.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxSuffixRemoveComma = UiUtil.MakeCheckBox(Se.Language.Options.Settings.RemoveComma, vm, nameof(vm.SelectedSuffixesRemoveComma));
        checkBoxSuffixRemoveComma.PropertyChanged += (s, e) => vm.StyleChanged();

        var checkBoxUseSpecialStyleAfterLongGaps = UiUtil.MakeCheckBox(Se.Language.Options.Settings.UseSpecialStyleAfterLongGaps, vm, nameof(vm.UseSpecialStyleAfterLongGaps));
        checkBoxUseSpecialStyleAfterLongGaps.PropertyChanged += (s, e) => vm.StyleChanged();
        var labelLongPrefix = UiUtil.MakeLabel(Se.Language.General.Prefix);
        var comboBoxLongPrefix = UiUtil.MakeComboBox(vm.PreAndSuffixes, vm, nameof(vm.SelectedLongGapPrefix));
        comboBoxLongPrefix.PropertyChanged += (s, e) => vm.StyleChanged();  

        var labelLongSuffix = UiUtil.MakeLabel(Se.Language.General.Suffix);
        var comboBoxLongSuffix = UiUtil.MakeComboBox(vm.PreAndSuffixes, vm, nameof(vm.SelectedLongGapSuffix));
        comboBoxLongSuffix.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxLongSuffixProcessIfEndWithComma = UiUtil.MakeCheckBox(Se.Language.Options.Settings.ProcessIfEndsWithComma, vm, nameof(vm.SelectedLongGapSuffixesProcessIfEndWithComma));
        checkBoxLongSuffixProcessIfEndWithComma.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxLongSuffixAddSpace = UiUtil.MakeCheckBox(Se.Language.Options.Settings.AddSpace, vm, nameof(vm.SelectedLongGapSuffixesAddSpace));
        checkBoxLongSuffixAddSpace.PropertyChanged += (s, e) => vm.StyleChanged();
        var checkBoxLongSuffixRemoveComma = UiUtil.MakeCheckBox(Se.Language.Options.Settings.RemoveComma, vm, nameof(vm.SelectedLongGapSuffixesRemoveComma));
        checkBoxLongSuffixRemoveComma.PropertyChanged += (s, e) => vm.StyleChanged();

        var splitButtonLoad = new SplitButton
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            Content = Se.Language.General.LoadDefaults,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItem
                    {
                        Header = Se.Language.General.None,
                        Command = vm.LoadContinuationStyleNoneCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneTrailingDots,
                        Command = vm.LoadContinuationStyleNoneTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingDots,
                        Command = vm.LoadContinuationStyleNoneLeadingTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneTrailingEllipsis,
                        Command = vm.LoadContinuationStyleNoneTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleNoneLeadingTrailingEllipsis,
                        Command = vm.LoadContinuationStyleNoneLeadingTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingDots,
                        Command = vm.LoadContinuationStyleOnlyTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDots,
                        Command = vm.LoadContinuationStyleLeadingTrailingDotsCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleOnlyTrailingEllipsis,
                        Command = vm.LoadContinuationStyleOnlyTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingEllipsis,
                        Command = vm.LoadContinuationStyleLeadingTrailingEllipsisCommand,
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDash,
                        Command = vm.LoadContinuationStyleLeadingTrailingDashCommand
                    },
                    new MenuItem
                    {
                        Header = Se.Language.Options.Settings.ContinuationStyleLeadingTrailingDashDots,
                        Command = vm.LoadContinuationStyleLeadingTrailingDashDotsCommand
                    },
                 }
            }
        };

        grid.Add(labelPrefix, 0);
        grid.Add(comboBoxPrefix, 0, 1);

        grid.Add(checkBoxPrefixAddSpace, 1, 1);
        grid.Add(labelSuffix, 2, 0);
        grid.Add(comboBoxSuffix, 2, 1);
        grid.Add(checkBoxSuffixProcessIfEndWithComma, 3, 1);
        grid.Add(checkBoxSuffixAddSpace, 4, 1);
        grid.Add(checkBoxSuffixRemoveComma, 5, 1);

        grid.Add(checkBoxUseSpecialStyleAfterLongGaps, 6, 0, 1, 2);
        grid.Add(labelLongPrefix, 7, 0);
        grid.Add(comboBoxLongPrefix, 7, 1);
        grid.Add(labelLongSuffix, 8, 0);
        grid.Add(comboBoxLongSuffix, 8, 1);
        grid.Add(checkBoxLongSuffixProcessIfEndWithComma, 9, 1);
        grid.Add(checkBoxLongSuffixAddSpace, 10, 1);
        grid.Add(checkBoxLongSuffixRemoveComma, 11, 1);

        grid.Add(splitButtonLoad, 12, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakePreviewView(CustomContinuationStyleViewModel vm)
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
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var labelPreview = UiUtil.MakeLabel(Se.Language.General.Preview);
        var labelPreviewContent = UiUtil.MakeLabel();

        grid.Add(labelPreview, 0);
        grid.Add(labelPreviewContent, 1);

        return UiUtil.MakeBorderForControl(grid);
    }
}
