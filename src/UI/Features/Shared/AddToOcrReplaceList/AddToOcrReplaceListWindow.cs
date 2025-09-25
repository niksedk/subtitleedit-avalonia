using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.AddToOcrReplaceList;

public class AddToOcrReplaceListWindow : Window
{
    public AddToOcrReplaceListWindow(AddToOcrReplaceListViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.AddNameToOcrReplaceList;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        var labelFromTo = UiUtil.MakeLabel("Change from/to");
        var textBoxFrom = UiUtil.MakeTextBox(200, vm, nameof(vm.From));
        var textBoxTo = UiUtil.MakeTextBox(200, vm, nameof(vm.To));
        var panelFromTo = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                textBoxFrom,
                UiUtil.MakeLabel("→"),
                textBoxTo
            }
        };  

        var labelDictionary = UiUtil.MakeLabel(Se.Language.General.Dictionary);
        var comboBoxDictionaries = new ComboBox
        {
            Width = 200,
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedDictionary)) { Mode = BindingMode.TwoWay },
            [!ComboBox.ItemsSourceProperty] = new Binding(nameof(vm.Dictionaries)) { Mode = BindingMode.TwoWay },
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

        grid.Add(panelFromTo, 0);
        grid.Add(labelDictionary, 1);
        grid.Add(comboBoxDictionaries, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}
