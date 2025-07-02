using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Projektanker.Icons.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public class NOcrCharacterAddWindow : Window
{
    private readonly NOcrCharacterAddViewModel _vm;

    public NOcrCharacterAddWindow(NOcrCharacterAddViewModel vm)
    {
        _vm = vm;
        vm.Window = this;
        Icon = UiUtil.GetSeIcon();
        Title = "";
        Width = 1200;
        Height = 700;
        MinWidth = 900;
        MinHeight = 600;
        CanResize = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = vm;
    }
}
