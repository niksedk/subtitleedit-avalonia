using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PromptUnknownWordWindow : Window
{
    public PromptUnknownWordWindow(PromptUnknownWordViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 700;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Abort, vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // image
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // whole text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // word + suggestions
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons (abort)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var image = new Image
        {
            Stretch = Avalonia.Media.Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 200,
            DataContext = vm,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.Bitmap)));

        grid.Add(image, 0, 0, 1, 2);
        grid.Add(MakeWholeTextView(vm), 1, 0, 1, 2);
        grid.Add(MakeWordView(vm), 2);
        grid.Add(MakeWordSuggestionsView(vm), 2, 1);
        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Loaded += vm.Onloaded;
        Closing += vm.OnClosing;
    }

    private static Grid MakeWholeTextView(PromptUnknownWordViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
        };

        var textBoxWholeText = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.WholeText))
            .WithHorizontalAlignmentStretch()
            .WithHeight(90);

        vm.PanelWholeText.Width = double.NaN;
        vm.PanelWholeText.HorizontalAlignment = HorizontalAlignment.Left;
        vm.PanelWholeText.VerticalAlignment = VerticalAlignment.Top;
        vm.PanelWholeText.Orientation = Orientation.Horizontal;
        vm.PanelWholeText.Margin = new Thickness(10);
        var scrollViewerWholeText = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = vm.PanelWholeText,
            Height = 85,
        };
        var scrollViewerWholeTextBorder = new Border
        {
            BorderThickness = new Avalonia.Thickness(1),
            BorderBrush = UiUtil.GetBorderBrush(),
            Child = scrollViewerWholeText,
        };

        var buttonEditWholeText = UiUtil.MakeButton(Se.Language.Ocr.EditWholeText);
        var buttonEditWordOnly = UiUtil.MakeButton(Se.Language.Ocr.EditWordOnly);
        var panelButtons = UiUtil.MakeVerticalPanel(buttonEditWholeText, buttonEditWordOnly);

        grid.Add(scrollViewerWholeTextBorder, 0);
        //        grid.Add(textBoxWholeText, 0);
        grid.Add(panelButtons, 0, 1);

        return grid;
    }

    private static Grid MakeWordView(PromptUnknownWordViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            RowSpacing = 5,
            ColumnSpacing = 5,
        };

        var textBoxWord = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.Word)).WithHorizontalAlignmentStretch();
        var buttonChangeAll = UiUtil.MakeButton(Se.Language.General.ChangeAll, vm.ChangeAllCommand).WithHorizontalAlignmentStretch();
        var buttonChangeOnce = UiUtil.MakeButton(Se.Language.General.ChangeOnce, vm.ChangeOnceCommand).WithHorizontalAlignmentStretch();
        var buttonSkipOne = UiUtil.MakeButton(Se.Language.General.SkipOnce, vm.SkipOnceCommand).WithHorizontalAlignmentStretch();
        var buttonGoogleIt = UiUtil.MakeButton(Se.Language.General.GoogleIt, vm.GoogleItCommand).WithHorizontalAlignmentStretch();
        var buttonSkipAll = UiUtil.MakeButton(Se.Language.General.SkipAll, vm.SkipAllCommand).WithHorizontalAlignmentStretch();
        var buttonAddToNameList = UiUtil.MakeButton(Se.Language.General.AddToNamesListCaseSensitive, vm.AddToNamesListCommand).WithHorizontalAlignmentStretch();
        var buttonAddToUserDictionary = UiUtil.MakeButton(Se.Language.General.AddToUserDictionary, vm.AddToUserDictionaryCommand).WithHorizontalAlignmentStretch();

        grid.Add(textBoxWord, 0, 0, 1, 2);
        grid.Add(buttonChangeAll, 1, 0, 1, 2);
        grid.Add(buttonChangeOnce, 2, 0);
        grid.Add(buttonSkipOne, 2, 1);
        grid.Add(buttonGoogleIt, 3, 0);
        grid.Add(buttonSkipAll, 3, 1);
        grid.Add(buttonAddToNameList, 4, 0, 1, 2);
        grid.Add(buttonAddToUserDictionary, 5, 0, 1, 2);

        return grid;
    }

    private static Grid MakeWordSuggestionsView(PromptUnknownWordViewModel vm)
    {
        var labelSuggestions = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            Content = Se.Language.General.Suggestions,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var buttonUseOnce = new Button
        {
            Content = "Use once",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseOnceCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonUseAlways = new Button
        {
            Content = "Use always",
            [!Button.CommandProperty] = new Binding(nameof(SpellCheckViewModel.SuggestionUseAlwaysCommand), BindingMode.OneWay),
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var panelButtons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 10),
            Spacing = 5,
            Children =
            {
                buttonUseOnce,
                buttonUseAlways,
            }
        };

        var listBoxSuggestions = new ListBox
        {
            [!ListBox.ItemsSourceProperty] = new Binding(nameof(SpellCheckViewModel.Suggestions), BindingMode.OneWay),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(SpellCheckViewModel.SelectedSuggestion), BindingMode.TwoWay),
            VerticalAlignment = VerticalAlignment.Top,
            Width = double.NaN,
            Height = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = new SolidColorBrush(Colors.Transparent),
        };
        listBoxSuggestions.DoubleTapped += vm.ListBoxSuggestionsDoubleTapped;

        var scrollViewSuggestions = new ScrollViewer
        {
            Content = listBoxSuggestions,
            Height = 160,
        };

        var borderSuggestions = UiUtil.MakeBorderForControl(scrollViewSuggestions);


        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };

        grid.Add(labelSuggestions, 0, 0);
        grid.Add(borderSuggestions, 1, 0);
        grid.Add(panelButtons, 2, 0);

        return grid;
    }
}
