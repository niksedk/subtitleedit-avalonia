using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckWindow : Window
{
    private SpellCheckViewModel _vm;

    public SpellCheckWindow(SpellCheckViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Spell check";
        Width = 810;
        Height = 490;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelLine = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            [!Label.ContentProperty] = new Binding(nameof(SpellCheckViewModel.LineText), BindingMode.OneWay)
        };

        var panelWholeText = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        vm.PanelWholeText = panelWholeText;
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = panelWholeText,
            Height = 85,
        };

        var boderWholeText = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Child = scrollViewerWholeText,
            Padding = new Thickness(10, 0, 10, 0),
            CornerRadius = new CornerRadius(5),
        };

        var panelButtons = MakeWordNotFound(vm);

        var panelSuggestions = MakeSuggestions(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithLeftAlignment();
        var panelButtonsOk = UiUtil.MakeButtonBar(buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 20,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };


        grid.Add(labelLine, 0, 0);
        grid.Add(boderWholeText, 1, 0);
        grid.Add(panelButtons, 2, 0);
        grid.Add(panelButtonsOk, 3, 1);

        grid.Add(panelSuggestions, 1, 1, 2);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    private Grid MakeWordNotFound(SpellCheckViewModel vm)
    {
        var labelWordNotFound = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = "Word not found",
            Margin = new Thickness(0, 10, 0, 0),
        };

        var textBoxWord = new TextBox
        {
            [!TextBox.TextProperty] = new Binding(nameof(SpellCheckViewModel.CurrentWord), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Center,
            Width = double.NaN,
        };

        var buttonChange = new Button
        {
            Content = "Change",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.ChangeCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonChangeAll = new Button
        {
            Content = "Change all",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.ChangeAllCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonSkipOne = new Button
        {
            Content = "Skip one",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SkipCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonSkipAll = new Button
        {
            Content = "Skip all",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SkipAllCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonAddToNames = new Button
        {
            Content = "Add to names",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.AddToNamesListCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonAddToDictionary = new Button
        {
            Content = "Add to user dictionary",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.AddToUserDictionaryCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonGoogleSearch = new Button
        {
            Content = "Google it",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.GoogleItCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 0),
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        grid.Add(labelWordNotFound, 0, 0, 1, 2);
        grid.Add(textBoxWord, 1, 0, 1, 2);
        grid.Add(buttonChange, 2, 0);
        grid.Add(buttonChangeAll, 2, 1);
        grid.Add(buttonSkipOne, 3, 0);
        grid.Add(buttonSkipAll, 3, 1);
        grid.Add(buttonAddToNames, 4, 0, 1, 2);
        grid.Add(buttonAddToDictionary, 5, 0, 1, 2);
        grid.Add(buttonGoogleSearch, 7, 0, 1, 2);

        return grid;
    }

    private Grid MakeSuggestions(SpellCheckViewModel vm)
    {
        var labelDictionary = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = "Dictionary",
        };

        var comboBoxDictionary = new ComboBox
        {
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Dictionaries), BindingMode.OneWay),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedDictionary), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Center,
            Width = 200,
        };

        var buttonDictionaryBrowse = new Button
        {
            Content = "...",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.BrowseDictionaryCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var panelDictionary = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Spacing = 5,
            Children =
            {
                labelDictionary,
                comboBoxDictionary,
                buttonDictionaryBrowse,
            }
        };

        var labelSuggestions = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = "Suggestions",
            Margin = new Thickness(0, 10, 0, 0),
        };

        var listBoxSuggestions = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Suggestions), BindingMode.OneWay),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedSuggestion), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var scrollViewSuggestions = new ScrollViewer
        {
            Content = listBoxSuggestions,
        };

        var borderSuggestions = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            CornerRadius = new CornerRadius(5),
            Child = scrollViewSuggestions,
        };

        var buttonUseOnce = new Button
        {
            Content = "Use once",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseOnceCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonUseAllways = new Button
        {
            Content = "Use all",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseAlwaysCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            Spacing = 5,
            Children =
            {
                buttonUseOnce,
                buttonUseAllways,
            }
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
            Margin = new Thickness(0, 0, 10, 0),
        };

        grid.Add(panelDictionary, 0, 0);
        grid.Add(labelSuggestions, 1, 0);
        grid.Add(borderSuggestions, 2, 0);
        grid.Add(panelButtons, 3, 0);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
