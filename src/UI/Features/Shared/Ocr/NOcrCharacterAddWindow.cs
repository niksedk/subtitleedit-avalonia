using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrCharacterAddWindow : Window
{
    private readonly NOcrCharacterAddViewModel _vm;

    public NOcrCharacterAddWindow(NOcrCharacterAddViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "";
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Whole image for subtitle
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var labelTitle = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.Title));

        var image = new Image();

        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonUseOnce = UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand);
        var buttonSkip = UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand);
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonUseOnce, buttonSkip, buttonAbort);

        grid.Add(labelTitle, 0, 0);
        grid.Add(image, 1, 0);
        grid.Add(controlsView, 2, 0);
        grid.Add(buttonBar, 3, 0);

        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    private static Grid MakeControlsView(NOcrCharacterAddViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            Width = double.NaN,
        };

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel("Current image"),
                new Image(),
                UiUtil.MakeTextBox(200, vm, nameof(vm.NewText)),
                UiUtil.MakeCheckBox("Italic", vm, nameof(vm.IsNewTextItalic)),
                UiUtil.MakeLabel("26x32, top margin 11"), //.WithBindText(vm, nameof(vm.Current))
            }
        };

        var panelDrawControls = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel("Lines to draw"),
                UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw)),
                UiUtil.MakeButton("Auto draw again", vm.DrawAgainCommand),
                UiUtil.MakeButton("Clear", vm.ClearDrawCommand),
            }
        };

        var panelImage = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel("- + foreground"),
                new Image(),
            }
        };

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelDrawControls, 0, 1);
        grid.Add(panelImage, 0, 2);

        return grid;
    }
}
