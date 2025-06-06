using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public class FixNamesWindow : Window
{

    private FixNamesViewModel _vm;

    public FixNamesWindow(FixNamesViewModel vm)
    {
        var lang = Se.Language.Tools.ChangeCasing;
        Icon = UiUtil.GetSeIcon();
        Title = lang.Title;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                 new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },    // Title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Name title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },     // Names
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Buttons
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Extra names label
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Extra names entry
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Hits title
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },     // Hits
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },     // Buttons
            },
             ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(20, 20, 20, 20)
        };

        var row = 0;
        var labelTitle = UiUtil.MakeLabel(lang.FixNames);
        grid.Add(labelTitle, row, 0);

        row++;
        var labelNamesCount = new Label();
        labelNamesCount.Bind(Label.ContentProperty, new Binding(nameof(vm.NamesCount)) { Mode = BindingMode.OneWay });
        grid.Add(labelNamesCount, row, 0);

        row++;
        grid.Add(MakeNamesView(vm), row, 0);

        row++;
        var buttonSelectAll = UiUtil.MakeButton(Se.Language.General.SelectAll, vm.NamesSelectAllCommand);
        var buttonInvertSelection = UiUtil.MakeButton(Se.Language.General.InvertSelection, vm.NamesInvertSelectionCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonSelectAll,
            buttonInvertSelection
        );  
        grid.Add(panelButtons, 0, row);

        row++;
        //var labelExtraNames = new Label
        //{
        //    Text = "Extra names",
        //    HorizontalOptions = LayoutOptions.Start,
        //}.BindDynamicTheme();
        //grid.Add(labelExtraNames, 0, row);

        //row++;
        //var entryExtraNames = new Entry
        //{
        //    Placeholder = "Enter extra names separated by comma",
        //    MinimumWidthRequest = 600,
        //}.BindDynamicTheme().BindText(nameof(vm.ExtraNames));
        //var buttonAddExtraName = new Button
        //{
        //    Text = "Refresh",
        //    Command = vm.AddExtraNameCommand,
        //    Margin = new Thickness(10, 0, 10, 0),
        //}.BindDynamicTheme();
        //var stackExtraNames = new StackLayout
        //{
        //    Orientation = StackOrientation.Horizontal,
        //    Margin = new Thickness(0, 5, 0, 20),
        //    Children =
        //    {
        //        entryExtraNames,
        //        buttonAddExtraName,
        //    }
        //};
        //grid.Add(stackExtraNames, 0, row);

        //row++;
        //var labelHits = new Label
        //{
        //    HorizontalOptions = LayoutOptions.Start,
        //    Margin = new Thickness(0, 20, 0, 0),
        //}.BindDynamicTheme().BindText(nameof(vm.HitCount));
        //grid.Add(labelHits, 0, row);

        //row++;
        //grid.Add(MakeHitsView(vm), 0, row);

        // OK and Cancel buttons
        row++;
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtonsBottom = UiUtil.MakeButtonBar(
            buttonOk,
            buttonCancel
        );

        grid.Add(panelButtonsBottom, row, 0);

        Content = grid;
    }

    private static Border MakeNamesView(FixNamesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // header
                new RowDefinition { Height = GridLength.Star }, // collection view of batch items
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            },
            Margin = new Thickness(2),
            //Padding = new Thickness(2),
            //RowSpacing = 2,
            //ColumnSpacing = 2,
            //HorizontalOptions = LayoutOptions.Fill,
            //VerticalOptions = LayoutOptions.Fill,
        };


        // Create the header grid
        var gridHeader = new Grid
        {
            //HorizontalOptions = LayoutOptions.Fill,
            //BackgroundColor = (Color)Application.Current!.Resources[ThemeNames.TableHeaderBackgroundColor],
            //Padding = new Thickness(5),
            ColumnDefinitions =
            {
                //new ColumnDefinition { Width =  100 }, // Enabled
                new ColumnDefinition { Width = GridLength.Star }, // Name
            },
        };

        // Add headers
        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "Enabled",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 0);
        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "Name",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 1);

        //// Add the header grid to the main grid
        //grid.Add(gridHeader, 0);

        //var collectionView = new CollectionView
        //{
        //    SelectionMode = SelectionMode.Single,
        //    HorizontalOptions = LayoutOptions.Fill,
        //    VerticalOptions = LayoutOptions.Fill,
        //    ItemTemplate = new DataTemplate(() =>
        //    {
        //        var jobItemGrid = new Grid
        //        {
        //            ColumnDefinitions =
        //            {
        //                new ColumnDefinition { Width = 100 }, // Enabled
        //                new ColumnDefinition { Width = GridLength.Star }, // Name
        //            },
        //        }.BindDynamicTheme();

        //        var isSelectedSwitch = new Switch
        //        {
        //            VerticalOptions = LayoutOptions.Center,
        //        }.BindDynamicThemeTextColorOnly();
        //        isSelectedSwitch.SetBinding(Switch.IsToggledProperty, nameof(FixNameItem.IsChecked));
        //        jobItemGrid.Add(isSelectedSwitch, 0, 0);
        //        isSelectedSwitch.Toggled += vm.OnNameToggled;

        //        var nameLabel = new Label
        //        {
        //            VerticalOptions = LayoutOptions.Center,
        //        }.BindDynamicThemeTextColorOnly();
        //        nameLabel.SetBinding(Label.TextProperty, nameof(FixNameItem.Name));
        //        jobItemGrid.Add(nameLabel, 1, 0);

        //        return jobItemGrid;
        //    }),
        //}.BindDynamicTheme();

        //grid.Add(collectionView, 0, 1);
        //collectionView.BindingContext = vm;
        //collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(vm.Names), BindingMode.TwoWay);

        var border = new Border
        {
            Padding = new Thickness(5),
            Margin = new Thickness(2),
            Child = grid,
        };

        return border;
    }

    private static Border MakeHitsView(FixNamesViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // header
                new RowDefinition { Height = GridLength.Star }, // collection view of batch items
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
            },
            Margin = new Thickness(2),
            //Padding = new Thickness(2),
            //RowSpacing = 2,
            //ColumnSpacing = 2,
            //HorizontalOptions = LayoutOptions.Fill,
            //VerticalOptions = LayoutOptions.Fill,
        };


        // Create the header grid
        var gridHeader = new Grid
        {
            //HorizontalOptions = LayoutOptions.Fill,
            //BackgroundColor = (Color)Application.Current!.Resources[ThemeNames.TableHeaderBackgroundColor],
            //Padding = new Thickness(5),
            ColumnDefinitions =
            {
                //new ColumnDefinition { Width = 100 }, // Apply
                //new ColumnDefinition { Width = 65 }, // Line#
                new ColumnDefinition { Width = GridLength.Star }, // Before
                new ColumnDefinition { Width = GridLength.Star }, // After
            },
        };

        //// Add headers
        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "Apply",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 0);
        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "Line#",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 1);

        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "Before",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 2);

        //gridHeader.Add(
        //    new Label
        //    {
        //        Text = "After",
        //        FontAttributes = FontAttributes.Bold,
        //        HorizontalTextAlignment = TextAlignment.Start,
        //        VerticalTextAlignment = TextAlignment.Center
        //    }, 3);


        // Add the header grid to the main grid
        grid.Add(gridHeader, 0);

        //var collectionView = new CollectionView
        //{
        //    SelectionMode = SelectionMode.Single,
        //    HorizontalOptions = LayoutOptions.Fill,
        //    VerticalOptions = LayoutOptions.Fill,
        //    ItemTemplate = new DataTemplate(() =>
        //    {
        //        var jobItemGrid = new Grid
        //        {
        //            ColumnDefinitions =
        //            {
        //                new ColumnDefinition { Width = 100 }, // Apply
        //                new ColumnDefinition { Width = 65 }, // Line#
        //                new ColumnDefinition { Width = GridLength.Star }, // Before
        //                new ColumnDefinition { Width = GridLength.Star }, // After
        //            },
        //        };

        //        var switchHitActive = new Switch
        //        {
        //        }.BindDynamicThemeTextColorOnly();
        //        switchHitActive.SetBinding(Switch.IsToggledProperty, nameof(FixNameHitItem.IsEnabled));
        //        jobItemGrid.Add(switchHitActive);

        //        var labelLineNumber = new Label
        //        {
        //            HorizontalTextAlignment = TextAlignment.Start,
        //            VerticalTextAlignment = TextAlignment.Center
        //        }.BindDynamicThemeTextColorOnly();
        //        labelLineNumber.SetBinding(Label.TextProperty, nameof(FixNameHitItem.LineIndexDisplay));
        //        jobItemGrid.Add(labelLineNumber, 1);

        //        var labelBefore = new Label
        //        {
        //            HorizontalTextAlignment = TextAlignment.Start,
        //            VerticalTextAlignment = TextAlignment.Center
        //        }.BindDynamicThemeTextColorOnly();
        //        labelBefore.SetBinding(Label.TextProperty, nameof(FixNameHitItem.Before));
        //        jobItemGrid.Add(labelBefore, 2);

        //        var labelAfter = new Label
        //        {
        //            HorizontalTextAlignment = TextAlignment.Start,
        //            VerticalTextAlignment = TextAlignment.Center
        //        }.BindDynamicThemeTextColorOnly();
        //        labelAfter.SetBinding(Label.TextProperty, nameof(FixNameHitItem.After));
        //        jobItemGrid.Add(labelAfter, 3);

        //        return jobItemGrid;
        //    }),
        //}.BindDynamicTheme();

        //grid.Add(collectionView, 0, 1);
        //collectionView.BindingContext = vm;
        //collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(vm.Hits), BindingMode.TwoWay);

        var border = new Border
        {
            Padding = new Thickness(5),
            Margin = new Thickness(2),
            Child = grid,
        };

        return border;
    }
}
