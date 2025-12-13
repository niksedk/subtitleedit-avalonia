using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using Projektanker.Icons.Avalonia;
using MenuItem = Avalonia.Controls.MenuItem;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

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
        Closing += (_, _) => vm.Closing();
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

    private static Grid MakeLayoutListViewAndEditBox(BinaryEditViewModel vm)
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
        
        dataGrid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(vm.Subtitles)));
        dataGrid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(vm.SelectedSubtitle)) { Mode = BindingMode.Default });

        vm.SubtitleGrid = dataGrid;

        // Columns: Forced, Number, Show, Duration, Text, Image
        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Forced,
            Width = new DataGridLength(60),
            MinWidth = 50,
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
                new Border
                {
                    Background = Avalonia.Media.Brushes.Transparent, // Prevents highlighting
                    Padding = new Thickness(4),
                    Child = new CheckBox
                    {
                        [!CheckBox.IsCheckedProperty] = new Binding(nameof(BinarySubtitleItem.IsForced)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }),
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.NumberSymbol,
            Width = new DataGridLength(50),
            MinWidth = 40,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.Number)),
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Show,
            Width = new DataGridLength(120),
            MinWidth = 100,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.StartTime)) { Converter = new TimeSpanToDisplayFullConverter() },
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = Se.Language.General.Duration,
            Width = new DataGridLength(80),
            MinWidth = 60,
            IsReadOnly = true,
            Binding = new Binding(nameof(BinarySubtitleItem.Duration)) { Converter = new TimeSpanToDisplayShortConverter() },
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = Se.Language.General.Image,
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            MinWidth = 80,
            IsReadOnly = true,
            CellTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<BinarySubtitleItem>((_, _) =>
            {
                var image = new Image
                {
                    MaxHeight = 22,
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    [!Image.SourceProperty] = new Binding(nameof(BinarySubtitleItem.Bitmap)),
                };
                return image;
            }),
            CellTheme = UiUtil.DataGridNoBorderNoPaddingCellTheme,
        });

        mainGrid.Add(dataGrid, 0, 0);

        // Controls section - using Grid layout for better organization
        var controlsGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto), // Labels
                new RowDefinition(GridLength.Auto), // Controls
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto), // Start Time
                new ColumnDefinition(GridLength.Auto), // Duration
                new ColumnDefinition(GridLength.Auto), // X
                new ColumnDefinition(GridLength.Auto), // Y
                new ColumnDefinition(GridLength.Auto), // Forced
                new ColumnDefinition(GridLength.Star), // Buttons
            },
            Margin = new Thickness(0, 10, 0, 0),
        };

        // Start Time - Column 0
        var startTimeLabel = new TextBlock
        {
            Text = Se.Language.General.Show,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 10, 2),
        };
        Grid.SetRow(startTimeLabel, 0);
        Grid.SetColumn(startTimeLabel, 0);
        controlsGrid.Children.Add(startTimeLabel);

        var startTimeUpDown = new Nikse.SubtitleEdit.Controls.TimeCodeUpDown
        {
            DataContext = vm,
            Margin = new Thickness(0, 0, 10, 0),
            [!Nikse.SubtitleEdit.Controls.TimeCodeUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.StartTime)}") 
            { 
                Mode = BindingMode.TwoWay 
            },
        };
        Grid.SetRow(startTimeUpDown, 1);
        Grid.SetColumn(startTimeUpDown, 0);
        controlsGrid.Children.Add(startTimeUpDown);

        // Duration - Column 1
        var durationLabel = new TextBlock
        {
            Text = Se.Language.General.Duration,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 10, 2),
        };
        Grid.SetRow(durationLabel, 0);
        Grid.SetColumn(durationLabel, 1);
        controlsGrid.Children.Add(durationLabel);

        var durationUpDown = new Nikse.SubtitleEdit.Controls.SecondsUpDown
        {
            DataContext = vm,
            Margin = new Thickness(0, 0, 10, 0),
            [!Nikse.SubtitleEdit.Controls.SecondsUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Duration)}") 
            { 
                Mode = BindingMode.TwoWay 
            },
        };
        Grid.SetRow(durationUpDown, 1);
        Grid.SetColumn(durationUpDown, 1);
        controlsGrid.Children.Add(durationUpDown);

        // X Position - Column 2
        var xLabel = new TextBlock
        {
            Text = "X",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 10, 2),
        };
        Grid.SetRow(xLabel, 0);
        Grid.SetColumn(xLabel, 2);
        controlsGrid.Children.Add(xLabel);

        var xUpDown = new NumericUpDown
        {
            Width = 100,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            Margin = new Thickness(0, 0, 10, 0),
            [!NumericUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.X)}") 
            { 
                Mode = BindingMode.TwoWay 
            },
        };
        Grid.SetRow(xUpDown, 1);
        Grid.SetColumn(xUpDown, 2);
        controlsGrid.Children.Add(xUpDown);

        // Y Position - Column 3
        var yLabel = new TextBlock
        {
            Text = "Y",
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 10, 2),
        };
        Grid.SetRow(yLabel, 0);
        Grid.SetColumn(yLabel, 3);
        controlsGrid.Children.Add(yLabel);

        var yUpDown = new NumericUpDown
        {
            Width = 100,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            DataContext = vm,
            Margin = new Thickness(0, 0, 10, 0),
            [!NumericUpDown.ValueProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Y)}") 
            { 
                Mode = BindingMode.TwoWay 
            },
        };
        Grid.SetRow(yUpDown, 1);
        Grid.SetColumn(yUpDown, 3);
        controlsGrid.Children.Add(yUpDown);

        // Forced Checkbox - Column 4
        var forcedLabel = new TextBlock
        {
            Text = Se.Language.General.Forced,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 10, 2),
        };
        Grid.SetRow(forcedLabel, 0);
        Grid.SetColumn(forcedLabel, 4);
        controlsGrid.Children.Add(forcedLabel);

        var forcedCheckBox = new CheckBox
        {
            [!CheckBox.IsCheckedProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.IsForced)}") { Mode = BindingMode.TwoWay },
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
        };
        Grid.SetRow(forcedCheckBox, 1);
        Grid.SetColumn(forcedCheckBox, 4);
        controlsGrid.Children.Add(forcedCheckBox);

        // Buttons - Column 5
        var buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var exportImageButton = UiUtil.MakeButton(vm.ExportImageCommand, IconNames.Export);
        buttonsPanel.Children.Add(exportImageButton);

        var importImageButton = UiUtil.MakeButton(vm.ImportImageCommand, IconNames.Import);
        buttonsPanel.Children.Add(importImageButton);

        var setTextButton = UiUtil.MakeButton(vm.SetTextCommand, IconNames.NewText);
        buttonsPanel.Children.Add(setTextButton);

        Grid.SetRow(buttonsPanel, 1);
        Grid.SetColumn(buttonsPanel, 5);
        controlsGrid.Children.Add(buttonsPanel);

        mainGrid.Add(controlsGrid, 1, 0);

        // Subscribe to X and Y changes to update overlay position
        xUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();
        yUpDown.ValueChanged += (_, _) => vm.UpdateOverlayPosition();

        return mainGrid;
    }

    private static Grid MakeVideoPlayer(BinaryEditViewModel vm)
    {
        var vp = InitVideoPlayer.MakeVideoPlayer();
        vp.FullScreenIsVisible = false;

        // Create a grid to hold the video player and overlay image
        var videoGrid = new Grid
        {
            ClipToBounds = true,
        };

        // Add video player as background
        videoGrid.Children.Add(vp);

        // Create overlay image for subtitle bitmap
        var overlayImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            [!Visual.IsVisibleProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Bitmap)}") 
            { 
                Converter = new NotNullConverter() 
            },
            [!Image.SourceProperty] = new Binding($"{nameof(vm.SelectedSubtitle)}.{nameof(BinarySubtitleItem.Bitmap)}"),
        };

        // Set up transform group for scaling and positioning
        var scaleTransform = new Avalonia.Media.ScaleTransform();
        var translateTransform = new TranslateTransform();
        var transformGroup = new Avalonia.Media.TransformGroup();
        transformGroup.Children.Add(scaleTransform);
        transformGroup.Children.Add(translateTransform);
        overlayImage.RenderTransform = transformGroup;

        videoGrid.Children.Add(overlayImage);
        
        // Store references
        vm.VideoPlayerControl = vp;
        vm.SubtitleOverlayImage = overlayImage;
        vm.SubtitleOverlayTransform = translateTransform;
        vm.SubtitleOverlayScaleTransform = scaleTransform;

        // Update position when video player size changes
        vp.SizeChanged += (_, _) => vm.UpdateOverlayPosition();

        // Implement mouse dragging for overlay image
        Point? dragStartPoint = null;
        int originalX = 0;
        int originalY = 0;

        overlayImage.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(overlayImage).Properties.IsLeftButtonPressed && vm.SelectedSubtitle != null)
            {
                dragStartPoint = e.GetPosition(videoGrid);
                originalX = vm.SelectedSubtitle.X;
                originalY = vm.SelectedSubtitle.Y;
                e.Handled = true;
            }
        };

        overlayImage.PointerMoved += (_, e) =>
        {
            if (dragStartPoint.HasValue && vm.SelectedSubtitle != null)
            {
                var currentPoint = e.GetPosition(videoGrid);
                var delta = currentPoint - dragStartPoint.Value;

                // Convert screen delta to subtitle coordinate delta (inverse of scale)
                if (vm.SubtitleOverlayScaleTransform != null)
                {
                    var deltaX = (int)(delta.X / vm.SubtitleOverlayScaleTransform.ScaleX);
                    var deltaY = (int)(delta.Y / vm.SubtitleOverlayScaleTransform.ScaleY);

                    vm.SelectedSubtitle.X = originalX + deltaX;
                    vm.SelectedSubtitle.Y = originalY + deltaY;

                    vm.UpdateOverlayPosition();
                }

                e.Handled = true;
            }
        };

        overlayImage.PointerReleased += (_, e) =>
        {
            if (dragStartPoint.HasValue)
            {
                dragStartPoint = null;
                e.Handled = true;
            }
        };

        return videoGrid;
    }
}