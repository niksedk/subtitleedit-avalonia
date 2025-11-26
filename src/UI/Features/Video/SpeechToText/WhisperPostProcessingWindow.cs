using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class WhisperPostProcessingWindow : Window
{
    private readonly WhisperPostProcessingViewModel _vm;

    public WhisperPostProcessingWindow(WhisperPostProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Whisper post-processing";
        Width = 350;
        Height = 350;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelAdjustTimings = UiUtil.MakeTextBlock("Adjust timings");
        var checkAdjustTimings = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.AdjustTimings));

        var labelMergeShortLines = UiUtil.MakeTextBlock("Merge short lines");
        var checkMergeShortLines = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.MergeShortLines));

        var labelBreakSplitLongLines = UiUtil.MakeTextBlock("Break/split long lines");
        var checkBreakSplitLongLines = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.BreakSplitLongLines));

        var labelFixShortDuration = UiUtil.MakeTextBlock("Fix short duration");
        var checkFixShortDuration = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.FixShortDuration));

        var labelFixCasing = UiUtil.MakeTextBlock("Fix casing");
        var checkFixCasing = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.FixCasing));

        var labelAddPeriods = UiUtil.MakeTextBlock("Add periods");
        var checkAddPeriods = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.AddPeriods));

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.Ok, vm.OKCommand),
            UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand)
        );

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;

        grid.Children.Add(labelAdjustTimings);
        Grid.SetRow(labelAdjustTimings, row);
        Grid.SetColumn(labelAdjustTimings, 0);

        grid.Children.Add(checkAdjustTimings);
        Grid.SetRow(checkAdjustTimings, row);
        Grid.SetColumn(checkAdjustTimings, 1);
        row++;

        grid.Children.Add(labelMergeShortLines);
        Grid.SetRow(labelMergeShortLines, row);
        Grid.SetColumn(labelMergeShortLines, 0);

        grid.Children.Add(checkMergeShortLines);
        Grid.SetRow(checkMergeShortLines, row);
        Grid.SetColumn(checkMergeShortLines, 1);
        row++;

        grid.Children.Add(labelBreakSplitLongLines);
        Grid.SetRow(labelBreakSplitLongLines, row);
        Grid.SetColumn(labelBreakSplitLongLines, 0);

        grid.Children.Add(checkBreakSplitLongLines);
        Grid.SetRow(checkBreakSplitLongLines, row);
        Grid.SetColumn(checkBreakSplitLongLines, 1);
        row++;

        grid.Children.Add(labelFixShortDuration);
        Grid.SetRow(labelFixShortDuration, row);
        Grid.SetColumn(labelFixShortDuration, 0);

        grid.Children.Add(checkFixShortDuration);
        Grid.SetRow(checkFixShortDuration, row);
        Grid.SetColumn(checkFixShortDuration, 1);
        row++;

        grid.Children.Add(labelFixCasing);
        Grid.SetRow(labelFixCasing, row);
        Grid.SetColumn(labelFixCasing, 0);

        grid.Children.Add(checkFixCasing);
        Grid.SetRow(checkFixCasing, row);
        Grid.SetColumn(checkFixCasing, 1);
        row++;

        grid.Children.Add(labelAddPeriods);
        Grid.SetRow(labelAddPeriods, row);
        Grid.SetColumn(labelAddPeriods, 0);

        grid.Children.Add(checkAddPeriods);
        Grid.SetRow(checkAddPeriods, row);
        Grid.SetColumn(checkAddPeriods, 1);
        row++;

        grid.Children.Add(buttonPanel);
        Grid.SetRow(buttonPanel, row);
        Grid.SetColumn(buttonPanel, 0);
        Grid.SetColumnSpan(buttonPanel, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
