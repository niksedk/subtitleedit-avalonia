using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public class TranslateSettingsWindow : Window
{
    public TranslateSettingsWindow(TranslateSettingsViewModel vm)
    {

        Title = "Translate Settings";
        Width = 500;
        MinWidth = 400;
        Height = 300;
        MinHeight = 200;
        DataContext = vm;
        vm.Window = this;
        var topBarPoweredBy = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children =
            {
                UiUtil.MakeTextBlock("Powered by"),
              
            }
        };
        Content = topBarPoweredBy;


    }
}
