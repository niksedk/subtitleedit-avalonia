using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitMenu
{
    public static Menu Make(MainViewModel vm)
    {
        var l = Se.Language.Main.Menu;

        vm.MenuReopen = new MenuItem
        {
            Header = l.Reopen,
            Command = vm.CommandFileReopenCommand,
        };

        UpdateRecentFiles(vm);


        return new Menu
        {
            Height = 30,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = l.File,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.New,
                            Command = vm.CommandFileNewCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Open,
                            Command = vm.CommandFileOpenCommand,
                        },
                        vm.MenuReopen,
                        new MenuItem
                        {
                            Header = l.RestoreAutoBackup,
                            Command = vm.ShowRestoreAutoBackupCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Save,
                            Command = vm.CommandFileSaveCommand,
                        },
                        new MenuItem
                        {
                            Header = l.SaveAs,
                            Command = vm.CommandFileSaveAsCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Export,
                            Items =
                            {
                                new MenuItem
                                {
                                    Header = "Blu-ray .sup",
                                    Command = vm.ExportBluRaySupCommand,
                                },
                                new MenuItem
                                {
                                    Header = new CapMakerPlus().Name,
                                    Command = vm.ExportCapMakerPlusCommand,
                                },
                                new MenuItem
                                {
                                    Header = Cavena890.NameOfFormat,
                                    Command = vm.ExportCavena890Command,
                                },
                                new MenuItem
                                {
                                    Header = Pac.NameOfFormat,
                                    Command = vm.ExportPacCommand,
                                },
                                new MenuItem
                                {
                                    Header = new PacUnicode().Name,
                                    Command = vm.ExportPacUnicodeCommand,
                                },
                                new MenuItem
                                {
                                    Header = Ebu.NameOfFormat,
                                    Command = vm.ExportEbuStlCommand,
                                }
                            }
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Exit,
                            Command = vm.CommandExitCommand,
                        }
                    }
                },
                new MenuItem
                {
                    Header = l.Edit,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.Undo,
                            Command = vm.UndoCommand,
                        },
                        new MenuItem
                        {
                            Header = l.Redo,
                            Command = vm.RedoCommand,
                        },
                        new MenuItem
                        {
                            Header = l.ShowHistory,
                            Command = vm.ShowHistoryCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Find,
                            Command = vm.ShowFindCommand,
                        },
                        new MenuItem
                        {
                            Header = l.FindNext,
                            Command = vm.FindNextCommand,
                        },
                        new MenuItem
                        {
                            Header = l.Replace,
                            Command = vm.ShowReplaceCommand,
                        },
                        new MenuItem
                        {
                            Header = l.MultipleReplace,
                            Command = vm.ShowMultipleReplaceCommand,
                        },
                        new MenuItem
                        {
                            Header = l.GoToLineNumber,
                            Command = vm.ShowGoToLineCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = Se.Language.General.SelectAll,
                            Command = vm.SelectAllLinesCommand,
                        },
                        new MenuItem
                        {
                            Header = Se.Language.General.InvertSelection,
                            Command = vm.InverseSelectionCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.Tools,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.AdjustDurations,
                            Command = vm.ShowToolsAdjustDurationsCommand,
                        },
                        new MenuItem
                        {
                            Header = l.FixCommonErrors,
                            Command = vm.ShowToolsFixCommonErrorsCommand,
                        },
                        new MenuItem
                        {
                            Header = l.RemoveTextForHearingImpaired,
                            Command = vm.ShowToolsRemoveTextForHearingImpairedCommand,
                        },
                        new MenuItem
                        {
                            Header = l.ChangeCasing,
                            Command = vm.ShowToolsChangeCasingCommand,
                        },
                        new MenuItem
                        {
                            Header = l.BatchConvert,
                            Command = vm.ShowToolsBatchConvertCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.SpellCheckTitle,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.SpellCheck,
                            Command = vm.ShowSpellCheckCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.GetDictionaries,
                            Command = vm.ShowSpellCheckDictionariesCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.Video,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.OpenVideo,
                            Command = vm.CommandVideoOpenCommand,
                        },
                        new MenuItem
                        {
                            Header = l.OpenVideoFromUrl,
                            Command = vm.ShowVideoOpenFromUrlCommand,
                        },
                        new MenuItem
                        {
                            Header = l.CloseVideoFile,
                            Command = vm.CommandVideoCloseCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.SpeechToText,
                            Command = vm.ShowVideoAudioToTextWhisperCommand,
                        },
                        new MenuItem
                        {
                            Header = l.TextToSpeech,
                            Command = vm.ShowVideoTextToSpeechCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.GenerateBurnIn,
                            Command = vm.ShowVideoBurnInCommand,
                        },
                        new MenuItem
                        {
                            Header = l.GenerateTransparent,
                            Command = vm.ShowVideoTransparentSubtitlesCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.Synchronization,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.AdjustAllTimes,
                            Command = vm.ShowSyncAdjustAllTimesCommand,
                        },
                        new MenuItem
                        {
                            Header = l.ChangeFrameRate,
                            Command = vm.ShowSyncChangeFrameRateCommand,
                        },
                        new MenuItem
                        {
                            Header = l.ChangeSpeed,
                            Command = vm.ShowSyncChangeSpeedCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.Options,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.Settings,
                            Command = vm.CommandShowSettingsCommand,
                        },
                        new MenuItem
                        {
                            Header = l.Shortcuts,
                            Command = vm.CommandShowSettingsShortcutsCommand,
                        },
                        new MenuItem
                        {
                            Header = l.ChooseLanguage,
                            Command = vm.CommandShowSettingsLanguageCommand,
                        },
                    },
                },
                new MenuItem
                {
                    Header = l.Translate,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.AutoTranslate,
                            Command = vm.CommandShowAutoTranslateCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = l.HelpTitle,
                    Items =
                    {
                        new MenuItem
                        {
                            Header = l.About,
                            Command = vm.ShowAboutCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = l.Help,
                            Command = vm.ShowHelpCommand,
                        },
                    }
                },
            }
        };
    }

    public static void UpdateRecentFiles(MainViewModel vm)
    {
        vm.MenuReopen.Items.Clear();
        if (Se.Settings.File.RecentFiles.Count > 0)
        {
            foreach (var file in Se.Settings.File.RecentFiles)
            {
                var item = new MenuItem
                {
                    Header = file.SubtitleFileName,
                    Command = vm.CommandFileReopenCommand,
                };
                item.CommandParameter = file;
                vm.MenuReopen.Items.Add(item);
            }

            vm.MenuReopen.Items.Add(new Separator());

            var clearItem = new MenuItem
            {
                Header = Se.Language.Main.Menu.ClearRecentFiles,
                Command = vm.CommandFileClearRecentFilesCommand,
            };
            vm.MenuReopen.Items.Add(clearItem);

            vm.MenuReopen.IsVisible = true;
        }
        else
        {
            vm.MenuReopen.IsVisible = false;
        }
    }
}