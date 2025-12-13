using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;

public class BinaryEditWindow : Window
{
    public BinaryEditWindow(BinaryEditViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.EditImagedBaseSubtitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
            },
        };

        // Top file menu
        var topContent = new Border
        {
            Child = MakeTopFileMenu(),
            Margin = new Thickness(0, 0, 0, 5),
        };
        contentGrid.Add(topContent, 0, 0, 1, 3);

        // Left section
        var leftContent = new Border
        {
            Child = MakeLayoutListViewAndEditBox(vm)
        };
        contentGrid.Add(leftContent, 1, 0);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        contentGrid.Add(splitter, 1, 1);

        // Right section
        var rightContent = new Border
        {
            Child = MakeVideoPlayer(vm),
        };
        contentGrid.Add(rightContent, 1,2);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        KeyDown += (_, args) => vm.OnKeyDown(args);
    }

    private static Label MakeTopFileMenu()
    {
        return UiUtil.MakeLabel("File menu not implemented yet.");
    }

    private Control MakeLayoutListViewAndEditBox(BinaryEditViewModel vm)
    {
        return UiUtil.MakeLabel("List view and edit box");
    }

    private Control MakeVideoPlayer(BinaryEditViewModel vm)
    {
        return UiUtil.MakeLabel("Video player not implemented yet.");
    }

}