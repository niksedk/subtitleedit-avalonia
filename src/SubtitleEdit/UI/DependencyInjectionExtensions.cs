using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Common;
using Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;
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
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

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
        collection.AddHttpClient<IFfmpegDownloadService, FfmpegDownloadService>();
        collection.AddHttpClient<IWhisperDownloadService, WhisperDownloadService>();
        collection.AddHttpClient<ISpellCheckDictionaryDownloadService, SpellCheckDictionaryDownloadService>();
        collection.AddHttpClient<ITesseractDownloadService, TesseractDownloadService>();
        collection.AddHttpClient<IPaddleOcrDownloadService, PaddleOcrDownloadService>();

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
        collection.AddTransient<GoToLineNumberWindow>();
        collection.AddTransient<GoToLineNumberViewModel>();
        collection.AddTransient<AudioToTextWhisperWindow>();
        collection.AddTransient<AudioToTextWhisperViewModel>();
        collection.AddTransient<BurnInWindow>();
        collection.AddTransient<BurnInViewModel>();
        collection.AddTransient<OpenFromUrlWindow>();
        collection.AddTransient<OpenFromUrlViewModel>();
        collection.AddTransient<TextToSpeechWindow>();
        collection.AddTransient<TextToSpeechViewModel>();
        collection.AddTransient<TransparentSubtitlesWindow>();
        collection.AddTransient<TransparentSubtitlesViewModel>();
        collection.AddTransient<SpellCheckWindow>();
        collection.AddTransient<SpellCheckViewModel>();
        collection.AddTransient<GetDictionariesWindow>();
        collection.AddTransient<GetDictionariesViewModel>();
        collection.AddTransient<AdjustAllTimesWindow>();
        collection.AddTransient<AdjustAllTimesViewModel>();
        collection.AddTransient<ChangeFrameRateWindow>();
        collection.AddTransient<ChangeFrameRateViewModel>();
        collection.AddTransient<ChangeSpeedWindow>();
        collection.AddTransient<ChangeSpeedViewModel>();
        collection.AddTransient<AdjustDurationWindow>();
        collection.AddTransient<AdjustDurationViewModel>();
        collection.AddTransient<BatchConvertWindow >();
        collection.AddTransient<BatchConvertViewModel>();
        collection.AddTransient<ChangeCasingWindow>();
        collection.AddTransient<ChangeCasingViewModel>();
        collection.AddTransient<FixCommonErrorsWindow>();
        collection.AddTransient<FixCommonErrorsViewModel>();
        collection.AddTransient<RemoveTextForHearingImpairedWindow>();
        collection.AddTransient<RemoveTextForHearingImpairedViewModel>();
        collection.AddTransient<FindWindow>();
        collection.AddTransient<FindViewModel>();
        collection.AddTransient<ReplaceWindow>();
        collection.AddTransient<ReplaceViewModel>();
        collection.AddTransient<MultipleReplaceWindow>();
        collection.AddTransient<MultipleReplaceViewModel>();
        collection.AddTransient<ShowHistoryWindow>();
        collection.AddTransient<ShowHistoryViewModel>();
        collection.AddTransient<RestoreAutoBackupWindow>();
        collection.AddTransient<RestoreAutoBackupViewModel>();
    }
}