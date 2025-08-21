using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Projektanker.Icons.Avalonia.MaterialDesign;
using System;
using System.Text;

var lifetime = new ClassicDesktopStyleApplicationLifetime
{
    Args = args,
    ShutdownMode = ShutdownMode.OnLastWindowClose
};

IconProvider.Current
           .Register<FontAwesomeIconProvider>()
           .Register<MaterialDesignIconProvider>();

Se.LoadSettings();

var appBuilder = AppBuilder.Configure<Application>()
    .UsePlatformDetect()
    .AfterSetup(b =>
    {
        b.Instance?.Styles.Add(new FluentTheme());
        b.Instance?.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml", UriKind.Absolute))
        {
            Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Fluent.xaml")
        });
        b.Instance?.Styles.Add(new StyleInclude(new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml", UriKind.Absolute))
        {
            Source = new Uri("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml", UriKind.Absolute)
        });

        if (Application.Current != null && !string.IsNullOrEmpty(Se.Settings.Appearance.FontName))
        {
            var fontName = Se.Settings.Appearance.FontName;

            // Existing styles
            Application.Current.Styles.Add(new Style(x => x.OfType<TextBlock>())
            {
                Setters =
                {
                    new Setter(TextBlock.FontFamilyProperty, new FontFamily(fontName)),
                }
            });

            Application.Current.Styles.Add(new Style(x => x.OfType<TextBox>())
            {
                Setters =
                {
                    new Setter(TextBox.FontFamilyProperty, new FontFamily(fontName)),
                }
            });

            // Add Button style
            Application.Current.Styles.Add(new Style(x => x.OfType<Button>())
            {
                Setters =
                {
                    new Setter(Button.FontFamilyProperty, new FontFamily(fontName)),
                }
            });

            // Add MenuItem style
            Application.Current.Styles.Add(new Style(x => x.OfType<Avalonia.Controls.MenuItem>())
            {
                Setters =
                {
                    new Setter(Avalonia.Controls.MenuItem.FontFamilyProperty, new FontFamily(fontName)),
                }
            });

            // Add other common controls
            Application.Current.Styles.Add(new Style(x => x.OfType<Label>())
            {
                Setters =
                {
                    new Setter(Label.FontFamilyProperty, new FontFamily(fontName)),
                }
            });

            Application.Current.Styles.Add(new Style(x => x.OfType<ComboBox>())
            {
                Setters =
                {
                    new Setter(ComboBox.FontFamilyProperty, new FontFamily(fontName)),
                }
            });
        }

        // Set application name
        if (b.Instance != null)
        {
            b.Instance.Name = "Subtitle Edit";
        }

        // Add Native Menu
        if (b.Instance != null)
        {
            var nativeMenu = new NativeMenu();
            var aboutMenu = new NativeMenuItem(Se.Language.Help.AboutSubtitleEdit);
            aboutMenu.Click += async (sender, e) =>
            {
                var aboutWindow = new AboutWindow();
                if (lifetime.MainWindow != null)
                {
                    await aboutWindow.ShowDialog(lifetime.MainWindow);
                }
            };
            nativeMenu.Items.Add(aboutMenu);
            NativeMenu.SetMenu(b.Instance, nativeMenu);
        }
    })
    .SetupWithLifetime(lifetime);

var collection = new ServiceCollection();
collection.AddSubtitleEditServices(); // DI

Locator.Services = collection.BuildServiceProvider();
var services = collection.BuildServiceProvider();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
UiUtil.SetCurrentTheme();

// Main window setup
lifetime.MainWindow = new Window
{
    Title = "Subtitle Edit",
    Icon = UiUtil.GetSeIcon(),
    MinWidth = 800,
    MinHeight = 500,
};

lifetime.MainWindow.Content = new MainView();

#if DEBUG
lifetime.MainWindow.AttachDevTools();
#endif

lifetime.Start(args);


