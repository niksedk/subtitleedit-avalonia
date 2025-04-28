using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
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
    })
    .SetupWithLifetime(lifetime);

var collection = new ServiceCollection();
collection.AddCommonServices();
collection.AddSingleton<IFileHelper, FileHelper>();
collection.AddTransient<IShortcutManager, ShortcutManager>();
Locator.Services = collection.BuildServiceProvider();
var services = collection.BuildServiceProvider();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Se.LoadSettings();

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