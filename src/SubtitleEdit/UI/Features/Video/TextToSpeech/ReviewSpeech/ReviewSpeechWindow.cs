using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public class ReviewSpeechWindow : Window
{
    private ReviewSpeechViewModel _vm;
    
    public ReviewSpeechWindow(ReviewSpeechViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "TTS - Review audio segments";
        Width = 900;
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
            [!DataGrid.ItemsSourceProperty] = new Binding(nameof(vm.Rows)),
            [!DataGrid.SelectedItemProperty] = new Binding(nameof(vm.SelectedRow)),
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "#",
                    Binding = new Binding(nameof(TtsStepResult.Paragraph) + "." + nameof(TtsStepResult.Paragraph.Number)),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Voice",
                    Binding = new Binding(nameof(TtsStepResult.Paragraph) + "." + nameof(TtsStepResult.Paragraph.Number)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                },
                new DataGridTextColumn
                {
                    Header = "Char/sec",
                    Binding = new Binding(nameof(TtsStepResult.Paragraph) + "." + nameof(TtsStepResult.Paragraph.Id)),
                    Width = new DataGridLength(3, DataGridLengthUnitType.Auto)
                }
            },
        };

        var border = new Border
        {
            Child = dataGrid,
        };

        return border;
    }

    private StackPanel MakeControls(ReviewSpeechViewModel vm)
    {
        var panelControls = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        return panelControls;
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
