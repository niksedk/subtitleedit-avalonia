using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

public class StatisticsWindow : Window
{
    public StatisticsWindow(StatisticsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = Se.Language.File.Statitics;
        CanResize = true;
        Width = 950;
        Height = 850;
        MinWidth = 800;
        MinHeight = 600;

        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };

        var textBoxGeneralStatistics = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };
        textBoxGeneralStatistics.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextGeneral)));

        var textBoxMostUsedWords = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };  
        textBoxMostUsedWords.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextMostUsedWords)));

        var textBoxMostUsedLines = new TextBox
        {
            IsReadOnly = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            DataContext = vm,
        };  
        textBoxMostUsedLines.Bind(TextBox.TextProperty, new Binding(nameof(vm.TextMostUsedLines)));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(textBoxGeneralStatistics, 0, 0, 1, 2);
        grid.Add(textBoxMostUsedWords, 1, 0);
        grid.Add(textBoxMostUsedLines, 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work

    }
}
