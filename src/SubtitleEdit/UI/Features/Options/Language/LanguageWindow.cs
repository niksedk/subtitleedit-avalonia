using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Language;

public class LanguageWindow : Window
{
    private StackPanel _contentPanel;
    private LanguageWindowViewModel _vm;
    
    public LanguageWindow()
    {
        Title = "Choose language";
        Width = 300;
        Height = 160;
        CanResize = false;

        _vm = new LanguageWindowViewModel();
        _vm.Window = this;
        DataContext = _vm;

        var languagePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10, 20, 10, 10),
            Children =
            {
                new Label
                {
                    Content = "Language",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new ComboBox
                {
                    ItemsSource = _vm.Languages,
                    SelectedValue = _vm.SelectedLanguage,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            }
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10),
            Children =
            {
                UiUtil.MakeButton("OK",  _vm.CommandOkCommand),
                UiUtil.MakeButton("Cancel",  _vm.CommandCancelCommand),
            }
        };
        
        _contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = new Thickness(10),
            Children =
            {
                languagePanel,
                buttonPanel,                
            }
        };

        Content = _contentPanel;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
