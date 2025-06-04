using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;
using Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
using Nikse.SubtitleEdit.Features.Edit.Replace;
using Nikse.SubtitleEdit.Features.Edit.ShowHistory;
using Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Features.Options.Language;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Shared.Ocr;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;
using Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;
using Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Features.Tools.BatchConvert;
using Nikse.SubtitleEdit.Features.Tools.ChangeCasing;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using DownloadFfmpegViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadFfmpegViewModel;
using DownloadLibMpvViewModel = Nikse.SubtitleEdit.Features.Shared.DownloadLibMpvViewModel;
using ElevenLabsSettingsViewModel = Nikse.SubtitleEdit.Features.Video.TextToSpeech.ElevenLabsSettings.ElevenLabsSettingsViewModel;
using PickMatroskaTrackViewModel = Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack.PickMatroskaTrackViewModel;

namespace Nikse.SubtitleEdit;

public static class DependencyInjectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        // Misc services
        collection.AddSingleton<IFileHelper, FileHelper>();
        collection.AddSingleton<IFolderHelper, FolderHelper>();
        collection.AddTransient<IShortcutManager, ShortcutManager>();
        collection.AddTransient<IWindowService, WindowService>();
        collection.AddTransient<IZipUnpacker, ZipUnpacker>();
        collection.AddTransient<INamesList, SeNamesList>();
        collection.AddTransient<IInsertService, InsertService>();
        collection.AddTransient<IMergeManager, MergeManager>();
        collection.AddTransient<IAutoBackupService, AutoBackupService>();
        collection.AddTransient<IUndoRedoManager, UndoRedoManager>();
        collection.AddTransient<ISpellCheckManager, SpellCheckManager>();
        collection.AddTransient<ITtsDownloadService, TtsDownloadService>();
        collection.AddTransient<IBluRayHelper, BluRayHelper>();
        collection.AddTransient<INOcrCaseFixer, NOcrCaseFixer>();

        // Download services
        collection.AddHttpClient<IFfmpegDownloadService, FfmpegDownloadService>();
        collection.AddHttpClient<IWhisperDownloadService, WhisperDownloadService>();
        collection.AddHttpClient<ISpellCheckDictionaryDownloadService, SpellCheckDictionaryDownloadService>();
        collection.AddHttpClient<ITesseractDownloadService, TesseractDownloadService>();
        collection.AddHttpClient<IPaddleOcrDownloadService, PaddleOcrDownloadService>();
        collection.AddHttpClient<ILibMpvDownloadService, LibMpvDownloadService>();

        // Window view models
        collection.AddTransient<MainView>();
        collection.AddTransient<MainViewModel>();
        collection.AddTransient<LayoutViewModel>();
        collection.AddTransient<LanguageViewModel>();
        collection.AddTransient<SettingsViewModel>();
        collection.AddTransient<ShortcutsViewModel>();
        collection.AddTransient<AutoTranslateViewModel>();
        collection.AddTransient<TranslateSettingsViewModel>();
        collection.AddTransient<DownloadFfmpegViewModel>();
        collection.AddTransient<GoToLineNumberViewModel>();
        collection.AddTransient<AudioToTextWhisperViewModel>();
        collection.AddTransient<BurnInViewModel>();
        collection.AddTransient<OpenFromUrlViewModel>();
        collection.AddTransient<TextToSpeechViewModel>();
        collection.AddTransient<TransparentSubtitlesViewModel>();
        collection.AddTransient<SpellCheckViewModel>();
        collection.AddTransient<GetDictionariesViewModel>();
        collection.AddTransient<AdjustAllTimesViewModel>();
        collection.AddTransient<ChangeFrameRateViewModel>();
        collection.AddTransient<ChangeSpeedViewModel>();
        collection.AddTransient<AdjustDurationViewModel>();
        collection.AddTransient<BatchConvertViewModel>();
        collection.AddTransient<ChangeCasingViewModel>();
        collection.AddTransient<FixCommonErrorsViewModel>();
        collection.AddTransient<FixCommonErrorsProfileViewModel>();
        collection.AddTransient<RemoveTextForHearingImpairedViewModel>();
        collection.AddTransient<FindViewModel>();
        collection.AddTransient<ReplaceViewModel>();
        collection.AddTransient<MultipleReplaceViewModel>();
        collection.AddTransient<ShowHistoryViewModel>();
        collection.AddTransient<RestoreAutoBackupViewModel>();
        collection.AddTransient<WhisperAdvancedViewModel>();
        collection.AddTransient<WhisperPostProcessingViewModel>();
        collection.AddTransient<DownloadWhisperModelsViewModel>();
        collection.AddTransient<DownloadWhisperEngineViewModel>();
        collection.AddTransient<DownloadLibMpvViewModel>();
        collection.AddTransient<EditWholeTextViewModel>();
        collection.AddTransient<EncodingSettingsViewModel>();
        collection.AddTransient<ElevenLabsSettingsViewModel>();
        collection.AddTransient<VoiceSettingsViewModel>();
        collection.AddTransient<DownloadTtsViewModel>();
        collection.AddTransient<ReviewSpeechViewModel>();
        collection.AddTransient<PickMatroskaTrackViewModel>();
        collection.AddTransient<OcrViewModel>();
    }
}