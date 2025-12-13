using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

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

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        KeyDown += (_, args) => vm.OnKeyDown(args);
    }
}