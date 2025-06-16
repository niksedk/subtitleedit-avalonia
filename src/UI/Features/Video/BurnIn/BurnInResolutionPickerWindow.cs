using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class BurnInResolutionPickerWindow : Window
{
    private readonly BurnInResolutionPickerViewModel _vm;

    public BurnInResolutionPickerWindow(BurnInResolutionPickerViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Burn-in - pick resolution";
        SizeToContent = SizeToContent.WidthAndHeight;
        MaxHeight = 900;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var listBoxResolutions = new ListBox
        {
            ItemsSource = vm.Resolutions,
            SelectedItem = vm.SelectedResolution,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 0),
            Padding = new Thickness(0, 0, 0, 0),
            [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedResolution)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
            ItemTemplate = new FuncDataTemplate<ResolutionItem>((item, namescope) =>
            {
                var panel = new StackPanel();

                var border = new Border
                {
                    Padding = new Thickness(2)
                };
                border.Bind(Border.BackgroundProperty, new Binding(nameof(ResolutionItem.BackgroundColor)));

                var textBlock = new TextBlock();
                textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(ResolutionItem.DisplayName)));
                textBlock.Bind(TextBlock.ForegroundProperty, new Binding(nameof(ResolutionItem.TextColor)));

                border.Child = textBlock;
                panel.Children.Add(border);

                // Bind to the IsSeperator property to decide if we should add a separator
                if (item.IsSeperator)
                {
                    panel.Children.Add(new Separator
                    {
                        Margin = new Thickness(0, 2, 0, 2)
                    });
                }

                return panel;
            }, true)
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(listBoxResolutions, 0, 0);
        grid.Add(panelButtons, 1, 0);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}