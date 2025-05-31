using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public class ReviewSpeechWindow : Window
{
    private ReviewSpeechViewModel _vm;

    public ReviewSpeechWindow(ReviewSpeechViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "TTS - Review audio segments";
        Width = 1000;
        Height = 650;
        MinWidth = 700;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var dataGrid = MakeDataGrid(vm);
        var controls = MakeControls(vm);
        var waveform = MakeWaveform(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(controls, 0, 1);
        grid.Add(waveform, 1, 0, 1, 2);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private Border MakeDataGrid(ReviewSpeechViewModel vm)
    {
        var dataGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
            Margin = new Thickness(0, 10, 0, 0),
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.Lines)),
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedLine)),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                new DataGridCheckBoxColumn
                {
                    Header = "Include",
                    Binding = new Binding(nameof(ReviewRow.Include)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(ReviewRow.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Voice",
                    Binding = new Binding(nameof(ReviewRow.Voice)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Char/sec",
                    Binding = new Binding(nameof(ReviewRow.Cps)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Speed",
                    Binding = new Binding(nameof(ReviewRow.Speed)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    Binding = new Binding(nameof(ReviewRow.Text)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                },
            },
        };

        var border = new Border
        {
            Child = dataGrid,
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return border;
    }

    private Border MakeControls(ReviewSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinwidth(controlMinWidth);
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;

        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Engine",
                    MinWidth = labelMinWidth,
                },
                comboBoxEngines,
            }
        };

        var panelVoice = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Voice",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithMinwidth(controlMinWidth),
            }
        };

        var panelModel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Model",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithMinwidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasModel)) { Mode = BindingMode.OneWay },
        };

        var panelRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Region",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion)).WithMinwidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasRegion)) { Mode = BindingMode.OneWay },
        };

        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new Label
                {
                    Content = "Language",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithMinwidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasLanguageParameter)) { Mode = BindingMode.OneWay },
        };

        //var panelApiKey = new StackPanel
        //{
        //    Orientation = Orientation.Horizontal,
        //    Children =
        //    {
        //        new Label
        //        {
        //            Content = "API key",
        //            MinWidth = labelMinWidth,
        //        },
        //        UiUtil.MakeTextBox(325, vm, nameof(vm.ApiKey)),
        //    },
        //    [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasApiKey)) { Mode = BindingMode.OneWay },
        //};

        var buttonRegenerateAudio = new Button
        {
            Content = "Regenerate audio for selected line",
            Command = vm.RegenerateAudioCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithMarginBottom(20)
         .WithIconLeft(IconNames.MdiRecycle);

        var buttonPlay = new Button
        {
            Content = "Play selected line",
            Command = vm.RegenerateAudioCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.MdiPlayCircle).WithBindIsVisible(nameof(vm.IsPlayVisible));

        var buttonStop = new Button
        {
            Content = "Stop",
            Command = vm.RegenerateAudioCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.MdiStopCircle).WithBindIsVisible(nameof(vm.IsStopVisible));

        var checkBoxAutoContinue = new CheckBox
        {
            Content = "Auto continue",
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.AutoContinue)) { Mode = BindingMode.TwoWay },
            Margin = new Thickness(0, 0, 0, 5),
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // filler
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15),
        };

        grid.Add(panelEngine, 0, 0);
        grid.Add(panelVoice, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(buttonRegenerateAudio, 6, 0);
        grid.Add(buttonPlay, 7, 0);
        grid.Add(buttonStop, 7, 0);
        grid.Add(checkBoxAutoContinue, 8, 0);

        var boder = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Child = grid,
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        return boder;
    }

    private Border MakeWaveform(ReviewSpeechViewModel vm)
    {
        return new Border
        {
            Margin = new Thickness(10),
            Child = new TextBlock { Text = "Waveform placeholder" } // Placeholder for waveform control
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
