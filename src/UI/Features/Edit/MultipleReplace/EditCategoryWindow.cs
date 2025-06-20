using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public class EditCategoryWindow : Window
{
    private readonly EditCategoryViewModel _vm;
    
    public EditCategoryWindow(EditCategoryViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelCategoryName = UiUtil.MakeLabel("Category name");
        var textBoxCategoryName = UiUtil.MakeTextBox(300, vm, nameof(vm.CategoryName));

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelCategoryName, 0, 0);
        grid.Add(textBoxCategoryName, 0, 1);

        grid.Add(buttonPanel, 4, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = _vm.Title;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
