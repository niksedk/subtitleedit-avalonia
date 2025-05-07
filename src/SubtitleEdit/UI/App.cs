using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia;

var lifetime = new ClassicDesktopStyleApplicationLifetime
{
    Args = args, 
    ShutdownMode = ShutdownMode.OnLastWindowClose 
};

IconProvider.Current
           .Register<FontAwesomeIconProvider>();
           //.Register<MaterialDesignIconProvider>();

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


Locator.Services = collection.BuildServiceProvider();
var services = collection.BuildServiceProvider();


Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Se.LoadSettings();

if (Se.Settings.Appearance.Theme == "Dark")
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
    Icon = UiUtil.GetSeIcon(),
};


lifetime.MainWindow.Content = new MainView();

#if DEBUG
lifetime.MainWindow.AttachDevTools();
#endif

lifetime.Start(args);