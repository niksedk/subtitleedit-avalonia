using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInWindow : Window
{
    private BurnInViewModel _vm;

    public BurnInWindow(BurnInViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Generate video with burned-in subtitles";
        Width = 1000;
        Height = 900;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var subtitleSettingsView = MakeSubtitlesView(vm);
        var videoSettingsView = MakeVideoSettingsView(vm);
        var cutView = MakeCutView(vm);
        var previewView = MakePreviewView(vm);
        var audioSettingsView = MakeAudioSettingsView(vm);
        var batchView = MakeBatchView(vm);

        var buttonGenerate = UiUtil.MakeButton("Generate", vm.GenerateCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonPanel = UiUtil.MakeButtonBar(
            buttonGenerate,
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(subtitleSettingsView, 0, 0, 2, 1);
        grid.Add(videoSettingsView, 2, 0);
        grid.Add(cutView, 0, 1);
        grid.Add(previewView, 1, 1);
        grid.Add(audioSettingsView, 2, 1);
        grid.Add(batchView, 0, 3, 3, 1);

        grid.Add(buttonPanel, 4, 0, 1, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeSubtitlesView(BurnInViewModel vm)
    {
        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName));

        var labelFontSizeFactor = UiUtil.MakeLabel(Se.Language.Video.VideoBurnIn.FontSizeFactor);
        var numericUpDownFontSizeFactor = UiUtil.MakeNumericUpDownTwoDecimals(0.1m, 1.0m, 200, vm, nameof(vm.FontFactor));
        var labelFontSizeFactorInfo = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.FontFactorText));
        var panelFontSizeFactor = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                numericUpDownFontSizeFactor,
                labelFontSizeFactorInfo
            }
        };

        var checkBoxUseBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.FontIsBold));

        var labelTextColor = UiUtil.MakeLabel(Se.Language.General.TextColor);
        var colorPickerTextColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontBoxColor));

        var labelBox = UiUtil.MakeLabel(Se.Language.General.Box);
        var textBoxBoxWidth = UiUtil.MakeTextBox(100, vm, nameof(vm.SelectedFontShadowWidth));
        var colorPickerBoxColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontBoxColor));
        var panelBox = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                textBoxBoxWidth,
                colorPickerBoxColor
            }
        };

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
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelFontName, 0, 0);
        grid.Add(comboBoxFontName, 0, 1);

        grid.Add(labelFontSizeFactor, 1, 0);
        grid.Add(panelFontSizeFactor, 1, 1);

        grid.Add(labelTextColor, 2, 0);
        grid.Add(colorPickerTextColor, 2, 1);

        grid.Add(labelBox, 3, 0);
        grid.Add(panelBox, 3, 1);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeVideoSettingsView(BurnInViewModel vm)
    {
        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("VideoSettingsView"));
    }

    private Border MakeCutView(BurnInViewModel vm)
    {
        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("Cutview"));
    }

    private Border MakePreviewView(BurnInViewModel vm)
    {
        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("PreviewView"));
    }

    private Border MakeAudioSettingsView(BurnInViewModel vm)
    {
        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("AudioSettingsView"));
    }

    private Border MakeBatchView(BurnInViewModel vm)
    {
        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("BatchView"));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
