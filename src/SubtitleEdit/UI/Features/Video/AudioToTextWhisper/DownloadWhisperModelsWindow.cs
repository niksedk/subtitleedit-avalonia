using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic.Config;
using Avalonia.Layout;
using Avalonia;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public class DownloadWhisperModelsWindow : Window
{
    private readonly DownloadWhisperModelsViewModel _vm;

    public DownloadWhisperModelsWindow(DownloadWhisperModelsViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "Downloading Whisper model";
        Width = 400;
        Height = 220;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var labelSelectModel = new TextBlock
        {
            Text = "Select model",
        };

        var comboBoxModel = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithMinwidth(200);

        var buttonDownload = UiUtil.MakeButton(Se.Language.General.Download, vm.DownloadCommand).WithLeftAlignment();

        var buttonOpenFolder = new Button
        {
            Margin = new Thickness(4, 0),
            Padding = new Thickness(12, 6),
            Height = 33,
            Command = vm.OpenModelFolderCommand,
        };
        Attached.SetIcon(buttonOpenFolder, "fa-solid fa-folder-open");

        var panelModelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 0),
            Children =
            {
                buttonDownload,
                buttonOpenFolder
            }
        };  


        var progressSlider = new Slider
        {
            Height = 8,
            Margin = new Thickness(0, 0, 0, 0),
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressSlider.Bind(Slider.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));
        var statusText = new TextBlock
        {
            Margin = new Thickness(0, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));
        statusText.Bind(TextBlock.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));
        var panelStatus = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 35, 0, 10),
            Children =
            {
                progressSlider,
                statusText
            }
        };  

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto"),
            Margin = UiUtil.MakeWindowMargin(),
        };

        var row = 0;
        grid.Children.Add(labelSelectModel);
        Grid.SetColumnSpan(labelSelectModel, 2);
        Grid.SetRow(labelSelectModel, row);
        Grid.SetColumn(labelSelectModel, 0);
        row++;

        grid.Children.Add(comboBoxModel);
        Grid.SetRow(comboBoxModel, row);
        Grid.SetColumn(comboBoxModel, 0);

        grid.Children.Add(panelModelButtons);
        Grid.SetRow(panelModelButtons, row);
        Grid.SetColumn(panelModelButtons, 1);
        row++;

        grid.Children.Add(panelStatus);
        Grid.SetColumnSpan(panelStatus, 2);
        Grid.SetRow(panelStatus, row);
        Grid.SetColumn(panelStatus, 0);
        row++;

        grid.Children.Add(buttonBar);
        Grid.SetColumnSpan(buttonBar, 2);
        Grid.SetRow(buttonBar, row);
        Grid.SetColumn(buttonBar, 0);

        Content = grid;

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        }; 
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}