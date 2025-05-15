using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Help;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.Logic.Download;

namespace Nikse.SubtitleEdit;

public static class DependencyInjectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // Misc services
        collection.AddSingleton<IFileHelper, FileHelper>();
        collection.AddTransient<IShortcutManager, ShortcutManager>();
        collection.AddTransient<IWindowService, WindowService>();
        collection.AddTransient<IZipUnpacker, ZipUnpacker>();
        collection.AddTransient<INamesList, SeNamesList>();
        
        // Download services
        collection.AddTransient<IFfmpegDownloadService, FfmpegDownloadService>();
        collection.AddTransient<IWhisperDownloadService, WhisperDownloadService>();
        collection.AddTransient<ISpellCheckDictionaryDownloadService, SpellCheckDictionaryDownloadService>();
        collection.AddTransient<ITesseractDownloadService, TesseractDownloadService>();

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
        collection.AddTransient<TranslateSettingsWindow>();
        collection.AddTransient<TranslateSettingsViewModel>();
        collection.AddTransient<DownloadFfmpegWindow>();
        collection.AddTransient<DownloadFfmpegViewModel>();
    }
}