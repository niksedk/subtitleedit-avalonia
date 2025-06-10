using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public class RemoveTextForHearingImpairedWindow : Window
{
    private RemoveTextForHearingImpairedViewModel _vm;
    
    public RemoveTextForHearingImpairedWindow(RemoveTextForHearingImpairedViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Remove text for hearing impaired";
        Width = 810;
        Height = 640;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var settingsView = MakeSettingsView(vm);
        var fixesView = MakeFixesView(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(
            buttonOk,
            UiUtil.MakeButtonCancel(vm.CancelCommand)
        );
        
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(settingsView, 0, 0);
        grid.Add(fixesView, 0, 1);
        grid.Add(panelButtons, 1, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    private StackPanel MakeSettingsView(RemoveTextForHearingImpairedViewModel vm)
    {
        var removeBetweenView = MakeRemoveBetweenView(vm);

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                removeBetweenView,
            }
        };

        return panel;
    }

    private Border MakeRemoveBetweenView(RemoveTextForHearingImpairedViewModel vm)
    {
        var labelTitle = UiUtil.MakeLabel("Remove text between");
        
        var comboBoxBrackets = UiUtil.MakeCheckBox("Brackets", vm, nameof(vm.IsRemoveBracketsOn));
        var comboBoxCurlyBrackets = UiUtil.MakeCheckBox("Curly brackets", vm, nameof(vm.IsRemoveCurlyBracketsOn));
        var comboBoxParentheses = UiUtil.MakeCheckBox("Parentheses", vm, nameof(vm.IsRemoveParenthesesOn));

        var textBoxCustomStart = UiUtil.MakeTextBox(30, vm, nameof(vm.CustomStart));
        var labelAnd = UiUtil.MakeLabel("and");
        var textBoxCustomEnd = UiUtil.MakeTextBox(30, vm, nameof(vm.CustomEnd));
        var panelCustom = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                 textBoxCustomStart,
                 labelAnd,
                 textBoxCustomEnd, 
            }
        };
        
        var grid = new Grid
        {
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
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelTitle, 0, 0);
        grid.Add(comboBoxBrackets, 1, 0);
        grid.Add(comboBoxCurlyBrackets, 2, 0);
        grid.Add(comboBoxParentheses, 3, 0);
        grid.Add(panelCustom, 4, 0); 
        
        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeFixesView(RemoveTextForHearingImpairedViewModel vm)
    {
        var border = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = UiUtil.GetBorderColor(),
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            Child = UiUtil.MakeLabel("fixes view"),
        };

        return border;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
