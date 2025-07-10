using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrDbEditWindow : Window
{
    private readonly NOcrDbEditViewModel _vm;

    public NOcrDbEditWindow(NOcrDbEditViewModel vm)
    {
        Title = Se.Language.Ocr.EditNOcrDatabase;
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Controls
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var charactersView = MakeCharacterControlsView(vm);
        var currentItemView = MakeCurrentItemControlsView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(charactersView, 0, 0);
        grid.Add(currentItemView, 0, 1);
        grid.Add(buttonBar, 1, 0, 1, 2);

        Content = grid;

        Activated += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
        };
    }

    private Border MakeCharacterControlsView(NOcrDbEditViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        var labelCharacters = UiUtil.MakeLabel("Character(s)");
        var charactersComboBox = UiUtil.MakeComboBox(vm.Characters, vm, nameof(vm.SelectedCharacter));
        var listBoxCurrentItems = new ListBox
        {
            ItemsSource = vm.CurrentCharacterItems,
            SelectedItem = vm.SelectedCurrentCharacterItem,
            Margin = new Thickness(0, 5, 0, 0),
            MinHeight = 100,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        grid.Add(labelCharacters, 0, 0);
        grid.Add(charactersComboBox, 1, 0);
        grid.Add(listBoxCurrentItems, 2, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeCurrentItemControlsView(NOcrDbEditViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, 
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            Width = double.NaN,
            Height = double.NaN,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };

        return UiUtil.MakeBorderForControl(UiUtil.MakeLabel("Current item"));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }
}
