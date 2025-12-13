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
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left section
        var leftContent = new Border
        {
            Child = MakeLayoutListViewAndEditBox(vm)
        };
        contentGrid.Add(leftContent, 0);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Width = UiUtil.SplitterWidthOrHeight,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        contentGrid.Add(splitter,1);

        // Right section
        var rightContent = new Border
        {
            Child = MakeVideoPlayer(vm),
        };
        contentGrid.Add(rightContent, 2);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        KeyDown += (_, args) => vm.OnKeyDown(args);
    }

    private Control MakeVideoPlayer(BinaryEditViewModel vm)
    {
        throw new NotImplementedException();
    }

    private Control MakeLayoutListViewAndEditBox(BinaryEditViewModel vm)
    {
        throw new NotImplementedException();
    }
}