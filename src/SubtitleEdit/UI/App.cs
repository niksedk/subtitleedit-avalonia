using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SubtitleAlchemist.Logic.Media;

var lifetime = new ClassicDesktopStyleApplicationLifetime
{
    Args = args, 
    ShutdownMode = ShutdownMode.OnLastWindowClose 
};

var appBuilder = AppBuilder.Configure<Application>()
    .UsePlatformDetect()
    .AfterSetup(b =>
    {
        b.Instance?.Styles.Add(new FluentTheme());
        b.Instance?.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", UriKind.Absolute))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        });
        b.Instance?.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml", UriKind.Absolute))
        {
            Source = new Uri("avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml", UriKind.Absolute)
        });
    })
    .SetupWithLifetime(lifetime);

var collection = new ServiceCollection();
collection.AddCommonServices();
collection.AddSingleton<IFileHelper, FileHelper>();
collection.AddTransient<IShortcutManager, ShortcutManager>();
collection.AddTransient<IWindowService, WindowService>();

// Windows and view models
collection.AddTransient<MainView>();
collection.AddTransient<MainViewModel>();
collection.AddTransient<LayoutWindow>();
collection.AddTransient<LayoutViewModel>();
collection.AddTransient<AboutWindow>();
collection.AddTransient<LanguageWindow>();
collection.AddTransient<LanguageViewModel>();
collection.AddTransient<SettingsWindow>();
collection.AddTransient<SettingsViewModel>();
collection.AddTransient<ShortcutsWindow>();
collection.AddTransient<ShortcutsViewModel>();
collection.AddTransient<AutoTranslateWindow>();
collection.AddTransient<AutoTranslateViewModel>();

Locator.Services = collection.BuildServiceProvider();
var services = collection.BuildServiceProvider();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Se.LoadSettings();

if (Application.Current!.ActualThemeVariant == ThemeVariant.Light)
{
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
}
else
{
    Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
}

// Main window setup
lifetime.MainWindow = new Window
{
    Title = "Subtitle Edit",
    Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Nikse.SubtitleEdit/Assets/se.ico"))),
};


lifetime.MainWindow.Content = new MainView();

#if DEBUG
lifetime.MainWindow.AttachDevTools();
#endif

lifetime.Start(args);