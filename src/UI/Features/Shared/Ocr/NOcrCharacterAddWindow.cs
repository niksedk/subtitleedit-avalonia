using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        var image = new Image
        {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(0, 0, 0, 10),
            MaxHeight = 350,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.SentenceBitmap)));

        var controlsView = MakeControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonUseOnce = UiUtil.MakeButton(Se.Language.General.UseOnce, vm.UseOnceCommand).WithBindIsVisible(nameof(vm.ShowUseOnce));
        var buttonSkip = UiUtil.MakeButton(Se.Language.General.Skip, vm.SkipCommand).WithBindIsVisible(nameof(vm.ShowSkip));
        var buttonAbort = UiUtil.MakeButton(Se.Language.General.Abort, vm.AbortCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonUseOnce, buttonSkip, buttonAbort);

        grid.Add(image, 0, 0);
        grid.Add(controlsView, 1, 0);
        grid.Add(buttonBar, 2, 0);

        Content = grid;

        vm.TextBoxNew.KeyDown += vm.TextBoxNewOnKeyDown;
        vm.TextBoxNew.KeyUp += vm.TextBoxNewOnKeyUp;

        Activated += delegate
        {
            vm.TextBoxNew.Focus(); // hack to make OnKeyDown work
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 20,
            Width = double.NaN,
        };

        vm.TextBoxNew = UiUtil.MakeTextBox(100, vm, nameof(vm.NewText));
        var image = new Image
        {
            Margin = new Thickness(5),
            Stretch = Stretch.Uniform,
            MinWidth = 30,
            MinHeight = 30,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.CurrentBitmap)));


        var panelCurrentImage = new StackPanel
        {
            Background = new SolidColorBrush(Colors.LightGray),
            Children = { image },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Thickness(5, 2, 0, 5),
        };

        var checkBoxItalic = UiUtil.MakeCheckBox(Se.Language.General.Italic, vm, nameof(vm.IsNewTextItalic));
        checkBoxItalic.IsCheckedChanged += vm.ItalicCheckChanged;

        var checkBoAutoSubmitFirsChar = UiUtil.MakeCheckBox(Se.Language.Ocr.AutoSubmitFirstCharacter, vm, nameof(vm.SubmitOnFirstLetter));

        var panelCurrent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.CurrentImage).WithBold(),
                UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ResolutionAndTopMargin)),
                panelCurrentImage,
                vm.TextBoxNew,
                checkBoxItalic,
                checkBoAutoSubmitFirsChar,
            },
        };

        var comboDrawModes = UiUtil.MakeComboBox(vm.DrawModes, vm, nameof(vm.SelectedDrawMode)).WithMarginLeft(5);
        comboDrawModes.SelectionChanged += vm.DrawModeChanged;

        var panelDrawControls = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                UiUtil.MakeLabel(Se.Language.Ocr.LinesToDraw).WithBold(),
                UiUtil.MakeComboBox(vm.NoOfLinesToAutoDrawList, vm, nameof(vm.SelectedNoOfLinesToAutoDraw)),
                UiUtil.MakeButton(Se.Language.Ocr.AutoDrawAgain, vm.DrawAgainCommand).WithMinWidth(100).WithMarginTop(10).WithLeftAlignment(),
                UiUtil.MakeButton(Se.Language.General.Clear, vm.ClearDrawCommand).WithMinWidth(100).WithMarginTop(5).WithLeftAlignment(),
                UiUtil.MakeLabel(Se.Language.Ocr.DrawMode).WithBold().WithMarginTop(10),
                comboDrawModes,
            }
        };

        vm.NOcrDrawingCanvas.SetStrokeWidth(1);
        var borderDrawingCanvas = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
            Child = vm.NOcrDrawingCanvas,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var panelZoom = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                UiUtil.MakeButton(vm.ZoomOutCommand, IconNames.MdiMinus),
                UiUtil.MakeButton(vm.ZoomInCommand, IconNames.MdiPlus),
                UiUtil.MakeLabel(string.Empty).WithMarginLeft(10).WithBindText(vm, nameof(vm.ZoomFactorInfo)),
            }
        };

        var panelImage = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Children =
            {
                panelZoom,
                borderDrawingCanvas,
            }
        };

        var buttonShrink = UiUtil.MakeButton(Se.Language.General.Shrink, vm.ShrinkCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanShrink));
        var buttonExpand = UiUtil.MakeButton(Se.Language.General.Expand, vm.ExpandCommand)
            .WithMinWidth(100).WithBindEnabled(nameof(vm.CanExpand));
        var panelButtons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Width = double.NaN,
            Margin = new Thickness(0, 0, 0, 5),
            Children =
            {
                buttonShrink,
                buttonExpand,
            }
        };

        grid.Add(panelCurrent, 0, 0);
        grid.Add(panelDrawControls, 0, 1);
        grid.Add(panelImage, 0, 2);
        grid.Add(panelButtons, 0, 2, 1, 2);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _vm.KeyUp(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = _vm.Title;
    }
}
