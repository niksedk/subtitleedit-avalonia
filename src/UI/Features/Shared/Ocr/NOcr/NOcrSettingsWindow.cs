using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr.NOcr;

public class NOcrSettingsWindow : Window
{
    private readonly NOcrSettingsViewModel _vm;

    public NOcrSettingsWindow(NOcrSettingsViewModel vm)
    {
        Title = Se.Language.Ocr.NOcrDatabase;
        _vm = vm;
        vm.Window = this;
        UiUtil.InitializeWindow(this);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Select action
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var labelTitle = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ActionText)).HorizontalContentAlignmentCenter();

        var buttonEdit = UiUtil.MakeButton(Se.Language.General.Edit, vm.EditCommand);
        var buttonDelete = UiUtil.MakeButton(Se.Language.General.Delete, vm.DeleteCommand);
        var buttonRename = UiUtil.MakeButton(Se.Language.General.Rename, vm.RenameCommand);
        var buttonNew = UiUtil.MakeButton(Se.Language.General.New, vm.NewCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonEdit, buttonDelete, buttonRename, buttonNew, buttonCancel);

        grid.Add(labelTitle, 0, 0);
        grid.Add(buttonBar, 1, 0);

        Content = grid;

        Activated += delegate
        {
            buttonEdit.Focus(); // hack to make OnKeyDown work
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }
}
