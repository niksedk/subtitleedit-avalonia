using Avalonia.Controls;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitMenu
{
    public static Menu Make(MainViewModel vm)
    {
        vm.MenuReopen = new MenuItem
        {
            Header = "_Reopen",
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
                    Header = "_File",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_New",
                            Command = vm.CommandFileNewCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Open...",
                            Command = vm.CommandFileOpenCommand,
                        },
                        vm.MenuReopen,
                        new MenuItem
                        {
                            Header = "Restore auto-backup...",
                            Command = vm.ShowRestoreAutoBackupCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Save",
                            Command = vm.CommandFileSaveCommand,
                        },
                        new MenuItem
                        {
                            Header = "Save _as...",
                            Command = vm.CommandFileSaveAsCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "Export",
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
                            Header = "E_xit",
                            Command = vm.CommandExitCommand,
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Edit",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Undo",
                            Command = vm.UndoCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Redo",
                            Command = vm.RedoCommand,
                        },
                        new MenuItem
                        {
                            Header = "Show _history...",
                            Command = vm.ShowHistoryCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Find...",
                            Command = vm.ShowFindCommand,
                        },
                        new MenuItem
                        {
                            Header = "Find _next",
                            Command = vm.FindNextCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Replace",
                            Command = vm.ShowReplaceCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Multiple replace",
                            Command = vm.ShowMultipleReplaceCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Go to line number...",
                            Command = vm.ShowGoToLineCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "Select _all",
                            Command = vm.SelectAllLinesCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Inverse selection",
                            Command = vm.InverseSelectionCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "Too_ls",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Adjust durations...",
                            Command = vm.ShowToolsAdjustDurationsCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Fix common errors...",
                            Command = vm.ShowToolsFixCommonErrorsCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Remove text for hearing impaired...",
                            Command = vm.ShowToolsRemoveTextForHearingImpairedCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Change casing...",
                            Command = vm.ShowToolsChangeCasingCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Batch convert...",
                            Command = vm.ShowToolsBatchConvertCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "_Spell Check",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Spell check...",
                            Command = vm.ShowSpellCheckCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Get dictionaries...",
                            Command = vm.ShowSpellCheckDictionariesCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "_Video",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Open video file...",
                            Command = vm.CommandVideoOpenCommand,
                        },
                        new MenuItem
                        {
                            Header = "Open video file from _URL...",
                            Command = vm.ShowVideoOpenFromUrlCommand,
                        },
                        new MenuItem
                        {
                            Header = "_Close video file",
                            Command = vm.CommandVideoCloseCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Audio to text (Whisper)...",
                            Command = vm.ShowVideoAudioToTextWhisperCommand,
                        },
                        new MenuItem
                        {
                             Header = "_Text to speech and add to video...",
                            Command = vm.ShowVideoTextToSpeechCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                             Header = "Generate video with burned-in subtitles...",
                            Command = vm.ShowVideoBurnInCommand,
                        },
                        new MenuItem
                        {
                             Header = "Generate transparent video with subtitles...",
                            Command = vm.ShowVideoTransparentSubtitlesCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "Syn_chronization",
                    Items =
                    {
                        new MenuItem
                        {
                             Header = "_Adjust all times (show earlier/later)...",
                            Command = vm.ShowSyncAdjustAllTimesCommand,
                        },
                        new MenuItem
                        {
                             Header = "_Change frame rate...",
                            Command = vm.ShowSyncChangeFrameRateCommand,
                        },
                        new MenuItem
                        {
                             Header = "_Change speed in percent",
                            Command = vm.ShowSyncChangeSpeedCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "_Options",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Settings...",
                            Command = vm.CommandShowSettingsCommand,
                        },
                        new MenuItem
                        {
                            Header = "Short_cuts...",
                            Command = vm.CommandShowSettingsShortcutsCommand,
                        },
                        new MenuItem
                        {
                            Header = "Choose _language...",
                            Command = vm.CommandShowSettingsLanguageCommand,
                        },
                    },
                },
                new MenuItem
                {
                    Header = "_Translate",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_Auto-translate...",
                            Command = vm.CommandShowAutoTranslateCommand,
                        },
                    }
                },
                new MenuItem
                {
                    Header = "_Help",
                    Items =
                    {
                        new MenuItem
                        {
                            Header = "_About",
                            Command = vm.ShowAboutCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Help",
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
                Header = "Clear recent files",
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