using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public class ReviewSpeechWindow : Window
{
    private readonly ReviewSpeechViewModel _vm;

    public ReviewSpeechWindow(ReviewSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "TTS - Review audio segments";
        Width = 1024;
        Height = 650;
        MinWidth = 700;
        MinHeight = 500;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var controls = MakeControls(vm);
        var dataGrid = MakeDataGrid(vm);
        var waveform = MakeWaveform(vm);

        var buttonExport = UiUtil.MakeButton("Export...", vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonExport, buttonOk, buttonCancel);

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(controls, 0, 0);
        grid.Add(dataGrid, 0, 1);
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
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedLine)) { Mode = BindingMode.TwoWay },
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
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(ReviewRow.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = "Voice",
                    Binding = new Binding(nameof(ReviewRow.Voice)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = "Char/sec",
                    Binding = new Binding(nameof(ReviewRow.Cps)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = "Speed",
                    Binding = new Binding(nameof(ReviewRow.Speed)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
                new DataGridTextColumn
                {
                    Header = "Text",
                    Binding = new Binding(nameof(ReviewRow.Text)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto),
                    CellTheme = UiUtil.DataGridNoBorderCellTheme,
                },
            },
        };
        vm.LineGrid = dataGrid;

        var textBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 80,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SelectedLine) + "." + nameof(ReviewRow.Text))
            {
                Mode = BindingMode.TwoWay
            },
            FontSize = Se.Settings.Appearance.SubtitleTextBoxFontSize,
            FontWeight = Se.Settings.Appearance.SubtitleTextBoxFontBold ? FontWeight.Bold : FontWeight.Normal,
            Margin = new Thickness(0, 0, 0, 3),
        };

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
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(dataGrid, 0, 0);
        grid.Add(textBox, 1, 0);
       
        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeControls(ReviewSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithMinWidth(controlMinWidth);
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
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Voice",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(controlMinWidth),
            }
        };

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithWidth(controlMinWidth);
        var panelModel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Model",
                    MinWidth = labelMinWidth,
                },
                comboBoxModels,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasModel)) { Mode = BindingMode.OneWay },
        };
        comboBoxModels.SelectionChanged += vm.SelectedModelChanged;

        var panelRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Region",
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion)).WithWidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasRegion)) { Mode = BindingMode.OneWay },
        };

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithWidth(controlMinWidth);
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = "Language",
                    MinWidth = labelMinWidth,
                },
                comboBoxLanguages,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.SelectedEngine) + "." + nameof(ITtsEngine.HasLanguageParameter)) { Mode = BindingMode.OneWay },
        };
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;


        var elevenLabsControls = MakeElevenLabsControls(vm);

        var buttonRegenerateAudio = new Button
        {
            Content = "Regenerate audio for selected line",
            Command = vm.RegenerateAudioCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
        }.WithIconLeft(IconNames.Recycle);

        var buttonPlay = new Button
        {
            Content = "Play selected line",
            Command = vm.PlayCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.PlayCircle).WithBindIsVisible(nameof(vm.IsPlayVisible));

        var buttonStop = new Button
        {
            Content = "Stop",
            Command = vm.StopCommand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            VerticalAlignment = VerticalAlignment.Center,
        }.WithIconLeft(IconNames.StopCircle).WithBindIsVisible(nameof(vm.IsStopVisible));

        var checkBoxAutoContinue = new CheckBox
        {
            Content = "Auto continue",
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.AutoContinue)) { Mode = BindingMode.TwoWay },
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
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15),
        };

        grid.Add(panelEngine, 0, 0);
        grid.Add(panelVoice, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(elevenLabsControls, 5, 0);
        // 6 is filler
        grid.Add(buttonRegenerateAudio, 7, 0);
        grid.Add(buttonPlay, 8, 0);
        grid.Add(buttonStop, 8, 0);
        grid.Add(checkBoxAutoContinue, 9, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeElevenLabsControls(ReviewSpeechViewModel vm)
    {
        var labelStability = UiUtil.MakeLabel("Stability");
        var sliderStability = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Stability,
            Width = 200,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Stability)),
        };
        var buttonStability = UiUtil.MakeButton(vm.ShowStabilityHelpCommand, IconNames.Help);

        var labelSimilarity = UiUtil.MakeLabel("Similarity");
        var sliderSimilarity = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = vm.Similarity,
            Width = 200,
            [!Slider.ValueProperty] = new Binding(nameof(vm.Similarity)),
        };
        var buttonSimilarity = UiUtil.MakeButton(vm.ShowSimilarityHelpCommand, IconNames.Help);

        var labelSpeakerBoost = UiUtil.MakeLabel("Speaker Boost");
        var sliderSpeakerBoost = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            Value = vm.SpeakerBoost,
            Width = 200,
            [!Slider.ValueProperty] = new Binding(nameof(vm.SpeakerBoost)),
        };
        var buttonSpeakerBoost = UiUtil.MakeButton(vm.ShowSpeakerBoostHelpCommand, IconNames.Help);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!Grid.IsVisibleProperty] = new Binding(nameof(vm.IsElevelLabsControlsVisible)) { Mode = BindingMode.OneWay },
        };

        grid.Add(labelStability, 0, 0);
        grid.Add(sliderStability, 0, 1);
        grid.Add(buttonStability, 0, 2);

        grid.Add(labelSimilarity, 1, 0);
        grid.Add(sliderSimilarity, 1, 1);
        grid.Add(buttonSimilarity, 1, 2);

        grid.Add(labelSpeakerBoost, 2, 0);
        grid.Add(sliderSpeakerBoost, 2, 1);
        grid.Add(buttonSpeakerBoost, 2, 2);

        return grid;
    }

    private static Border MakeWaveform(ReviewSpeechViewModel vm)
    {
        return new Border
        {
            Margin = new Thickness(2),
//          Child = new TextBlock { Text = "Waveform placeholder" } // Placeholder for waveform control
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }
}
