using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickSubtitleFormat;

public class PickSubtitleFormatWindow : Window
{
    public PickSubtitleFormatWindow(PickSubtitleFormatViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickSubtitleFormat;
        CanResize = true;
        Width = 800;
        Height = 700;
        MinWidth = 500;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        var labelSearch = UiUtil.MakeLabel(Se.Language.General.Search);
        var textBoxSearch = new TextBox
        {
            Watermark = Se.Language.General.SearchSubtitleFormats,
            Margin = new Thickness(10),
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        textBoxSearch.Bind(TextBox.TextProperty, new Binding(nameof(vm.SearchText)) { Source = vm });
        textBoxSearch.TextChanged += (s, e) => vm.SearchTextChanged();
        var panelSearch = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                labelSearch,
                textBoxSearch,
            }
        };

        
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSearch, 0);
        grid.Add(buttonPanel, 5);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
