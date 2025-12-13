using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using System;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public class BinaryEditWindow : Window
{
    public BinaryEditWindow(BinaryEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.EditImagedBaseSubtitle;
        Width = 1200;
        Height = 700;
        CanResize = true;
        vm.Window = this;
        DataContext = vm;

        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // Menu
                new RowDefinition(GridLength.Star),  // Content
                new RowDefinition(GridLength.Auto),  // Button panel
            },
        };

        // Top menu bar
        var menu = MakeTopMenu(vm);
        mainGrid.Add(menu, 0, 0);

        // Content area (grid + video)
        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
        };

        // Left section - grid with subtitles lines + controls
        var leftContent = new Border
        {
            Child = MakeLayoutListViewAndEditBox(vm),
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
        };
        contentGrid.Add(leftContent, 0, 0);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        contentGrid.Add(splitter, 0, 1);

        // Right section - video player
        var rightContent = new Border
        {
            Child = MakeVideoPlayer(vm),
            BorderBrush = UiUtil.GetBorderBrush(),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(5),
        };
        contentGrid.Add(rightContent, 0, 2);

        mainGrid.Add(contentGrid, 1, 0);

        // Button panel
        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));
        mainGrid.Add(buttonPanel, 2, 0);

        Content = mainGrid;
        KeyDown += (_, args) => vm.OnKeyDown(args);
    }

    private static Menu MakeTopMenu(BinaryEditViewModel vm)
    {
        var l = Se.Language.Main.Menu;
        var menu = new Menu
        {
            Height = 30,
            DataContext = vm,
        };

        // File menu
        menu.Items.Add(new MenuItem
        {
            Header = l.File,
            Items =
            {
                new MenuItem
                {
                    Header = l.Open,
                    Command = vm.FileOpenCommand,
                    Icon = new Icon { Value = IconNames.FolderOpen },
                },
                new MenuItem
                {
                    Header = l.Save,
                    Command = vm.FileSaveCommand,
                    Icon = new Icon { Value = IconNames.ContentSave },
                },
                new Separator(),
                new MenuItem
                {
                    Header = Se.Language.File.Import.TimeCodesDotDotDot,
                    Command = vm.ImportTimeCodesCommand,
                },
                new Separator(),
                new MenuItem
                {
                    Header = l.Exit,
                    Command = vm.CancelCommand,
                },
            },
        });

        // Tools menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Tools,
            Items =
            {
                new MenuItem
                {
                    Header = l.AdjustDurations,
                    Command = vm.AdjustDurationsCommand,
                },
                new MenuItem
                {
                    Header = l.ApplyDurationLimits,
                    Command = vm.ApplyDurationLimitsCommand,
                },
                new MenuItem
                {
                    Header = "Alignment...",
                    Command = vm.AlignmentCommand,
                },
                new MenuItem
                {
                    Header = "Resize images...",
                    Command = vm.ResizeImagesCommand,
                },
                new MenuItem
                {
                    Header = "Adjust brightness...",
                    Command = vm.AdjustBrightnessCommand,
                },
                new MenuItem
                {
                    Header = "Adjust alpha...",
                    Command = vm.AdjustAlphaCommand,
                },
                new MenuItem
                {
                    Header = "Quick OCR...",
                    Command = vm.QuickOcrCommand,
                },
            },
        });

        // Synchronization menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Synchronization,
            Items =
            {
                new MenuItem
                {
                    Header = l.AdjustAllTimes,
                    Command = vm.AdjustAllTimesCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeFrameRate,
                    Command = vm.ChangeFrameRateCommand,
                },
                new MenuItem
                {
                    Header = l.ChangeSpeed,
                    Command = vm.ChangeSpeedCommand,
                },
            },
        });

        // Video menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Video,
            Items =
            {
                new MenuItem
                {
                    Header = l.OpenVideo,
                    Command = vm.OpenVideoCommand,
                    Icon = new Icon { Value = IconNames.Play },
                },
            },
        });

        // Options menu
        menu.Items.Add(new MenuItem
        {
            Header = l.Options,
            Items =
            {
                new MenuItem
                {
                    Header = l.Settings,
                    Command = vm.SettingsCommand,
                    Icon = new Icon { Value = IconNames.Settings },
                },
            },
        });

        return menu;
    }

    private Control MakeLayoutListViewAndEditBox(BinaryEditViewModel vm)
    {
        var mainGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),  // DataGrid
                new RowDefinition(GridLength.Auto),   // Controls
            },
            Margin = new Thickness(5),
        };

        // DataGrid for subtitle lines
        var dataGrid = new DataGrid
        {
            Height = double.NaN,
            Margin = new Thickness(0, 0, 0, 5),
            CanUserSortColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Extended,
            CanUserResizeColumns = true,
            GridLinesVisibility = UiUtil.GetGridLinesVisibility(),
            VerticalGridLinesBrush = UiUtil.GetBorderBrush(),
            HorizontalGridLinesBrush = UiUtil.GetBorderBrush(),
            FontSize = Se.Settings.Appearance.SubtitleGridFontSize,
        };

        vm.SubtitleGrid = dataGrid;

        // Columns: Forced, Number, Show, Hide, Duration, Text
        dataGrid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "Forced",
            Width = new DataGridLength(60),
            MinWidth = 50,
            Binding = new Binding("IsForced"),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new DataGridLength(50),
            MinWidth = 40,
            Binding = new Binding("Number"),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Width = new DataGridLength(120),
            MinWidth = 100,
            Binding = new Binding("StartTime"),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Hide,
            Width = new DataGridLength(120),
            MinWidth = 100,
            Binding = new Binding("EndTime"),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Duration,
            Width = new DataGridLength(80),
            MinWidth = 60,
            Binding = new Binding("Duration"),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Text,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            MinWidth = 100,
            Binding = new Binding("Text"),
        });

        mainGrid.Add(dataGrid, 0, 0);

        // Controls section
        var controlsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
            Margin = new Thickness(0, 5, 0, 0),
        };

        // Start time
        var startTimeLabel = UiUtil.MakeLabel(Se.Language.General.Show);
        startTimeLabel.FontWeight = Avalonia.Media.FontWeight.Bold;
        startTimeLabel.Margin = new Thickness(0, 0, 5, 0);
        controlsGrid.Add(startTimeLabel, 0, 0);

        var startTimeTextBox = new TextBox
        {
            Width = 120,
            Margin = new Thickness(0, 0, 10, 5),
            [!TextBox.TextProperty] = new Binding("SelectedStartTime") { Mode = BindingMode.TwoWay },
        };
        controlsGrid.Add(startTimeTextBox, 0, 1);

        // Duration
        var durationLabel = UiUtil.MakeLabel(Se.Language.General.Duration);
        durationLabel.FontWeight = Avalonia.Media.FontWeight.Bold;
        durationLabel.Margin = new Thickness(0, 0, 5, 0);
        controlsGrid.Add(durationLabel, 1, 0);

        var durationTextBox = new TextBox
        {
            Width = 100,
            Margin = new Thickness(0, 0, 10, 5),
            [!TextBox.TextProperty] = new Binding("SelectedDuration") { Mode = BindingMode.TwoWay },
        };
        controlsGrid.Add(durationTextBox, 1, 1);

        // Forced checkbox
        var forcedCheckBox = new CheckBox
        {
            Content = "Forced",
            Margin = new Thickness(0, 0, 10, 5),
            [!CheckBox.IsCheckedProperty] = new Binding("SelectedIsForced") { Mode = BindingMode.TwoWay },
        };
        controlsGrid.Add(forcedCheckBox, 2, 1);

        // Buttons
        var buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var exportImageButton = UiUtil.MakeButton(vm.ExportImageCommand, IconNames.Export);
        buttonsPanel.Children.Add(exportImageButton);

        var importImageButton = UiUtil.MakeButton(vm.ImportImageCommand, IconNames.Import);
        buttonsPanel.Children.Add(importImageButton);

        var setTextButton = UiUtil.MakeButton(vm.SetTextCommand, IconNames.NewText);
        buttonsPanel.Children.Add(setTextButton);

        controlsGrid.Add(buttonsPanel, 2, 0, 1, 2);
        controlsGrid.Children[controlsGrid.Children.Count - 1].SetValue(Grid.RowSpanProperty, 2);

        mainGrid.Add(controlsGrid, 1, 0);

        return mainGrid;
    }

    private Control MakeVideoPlayer(BinaryEditViewModel vm)
    {
        var videoPanel = new StackPanel
        {
            Margin = new Thickness(5),
        };

        var videoLabel = UiUtil.MakeLabel("Video player");
        videoLabel.FontWeight = Avalonia.Media.FontWeight.Bold;
        videoLabel.Margin = new Thickness(0, 0, 0, 5);
        videoPanel.Children.Add(videoLabel);

        var videoPlaceholder = new Border
        {
            Height = 400,
            Background = Avalonia.Media.Brushes.Black,
            Child = UiUtil.MakeLabel("Video player will be displayed here"),
        };
        videoPanel.Children.Add(videoPlaceholder);

        return videoPanel;
    }

}