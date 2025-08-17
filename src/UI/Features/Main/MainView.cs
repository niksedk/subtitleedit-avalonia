using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Config.Language;
using System;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Main;

public static class Locator
{
    public static IServiceProvider Services { get; set; } = default!;
}
public class MainView : ViewBase
{
    private MainViewModel? _vm;

    protected override object Build()
    {
        _vm = Locator.Services.GetRequiredService<MainViewModel>();
        if (_vm == null)
        {
            throw new InvalidOperationException("MainViewModel is not registered in the service provider.");
        }
        
        _vm.MainView = this;
        DataContext = _vm;

        _vm.Window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;

        _vm.Window.Closing += _vm.OnClosing;

        _vm.Window.OnLoaded(e =>
        {
            _vm.OnLoaded();
        });

        // load language
        Se.Settings.General.Language = Se.Settings.General.Language ?? "English"; // default to English if not set
        if (Se.Settings.General.Language != "English")
        {
            var jsonFileName = System.IO.Path.Combine(Se.TranslationFolder, Se.Settings.General.Language + ".json");
            if (System.IO.File.Exists(jsonFileName))
            {
                var json = System.IO.File.ReadAllText(jsonFileName, Encoding.UTF8);
                var language = System.Text.Json.JsonSerializer.Deserialize<SeLanguage>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
                if (language != null)
                {
                    Se.Language = language;
                }
            }
        }

        var root = new DockPanel();

        // Menu bar
        InitMenu.Make(_vm);
        root.Children.Add(_vm.Menu.Dock(Dock.Top));

        // Toolbar
        root.Children.Add(InitToolbar.Make(_vm).Dock(Dock.Top));

        // Footer
        root.Children.Add(InitFooter.Make(_vm).Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        _vm.ContentGrid = ViewContent.Make(_vm);
        InitLayout.MakeLayout(this, _vm, Se.Settings.General.LayoutNumber);
        root.Children.Add(_vm.ContentGrid);

        AddHandler(KeyUpEvent, _vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        return root;
    }
}