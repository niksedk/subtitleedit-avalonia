using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitMenu
{
    public static Menu Make(MainViewModel vm)
    {
        vm.MenuReopen = new MenuItem
        {
            Header = "_Reopen",
            Command =vm.CommandFileReopenCommand,
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
                            Command =vm.CommandFileNewCommand,
                        },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Open...",
                            Command =vm.CommandFileOpenCommand,
                        },
                        vm.MenuReopen,
                        new MenuItem { Header = "Restore auto-backup..." },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "_Save",
                            Command =vm.CommandFileSaveCommand,
                        },
                        new MenuItem
                        {
                            Header = "Save _as...",
                            Command =vm.CommandFileSaveAsCommand,
                        },
                        new Separator(),
                        new MenuItem { Header = "Export" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            Command =vm.CommandExitCommand,
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Edit",
                    Items =
                    {
                        new MenuItem { Header = "_Undo" },
                        new MenuItem { Header = "_Redo" },
                        new MenuItem { Header = "Show _history..." },
                        new Separator(),
                        new MenuItem { Header = "_Find..." },
                        new MenuItem { Header = "Find _next" },
                        new MenuItem { Header = "_Multiple replace" },
                        new Separator(),
                        new MenuItem { Header = "Select _all" },
                        new MenuItem { Header = "_Inverse selection" },
                    }
                },
                new MenuItem
                {
                    Header = "Too_ls",
                    Items =
                    {
                        new MenuItem { Header = "_Adjust durations..." },
                        new MenuItem { Header = "_Fix common errors..." },
                        new MenuItem { Header = "_Remove text for hearing impaired..." },
                        new MenuItem { Header = "_Change casing..." },
                        new MenuItem { Header = "_Batch convert..." },
                    }
                },
                new MenuItem
                {
                    Header = "_Spell Check",
                    Items =
                    {
                        new MenuItem { Header = "_Spell check..." },
                        new Separator(),
                        new MenuItem { Header = "_Get dictionaries..." },
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
                            Command =vm.CommandVideoOpenCommand,
                        },
                        new MenuItem { Header = "Open video file from _URL..." },
                        new MenuItem
                        {
                            Header = "_Close video file",
                            Command =vm.CommandVideoCloseCommand,
                        },
                        new Separator(),
                        new MenuItem { Header = "_Audio to text (Whisper)..." },
                        new MenuItem { Header = "_Text to speech and add to video..." },
                        new Separator(),
                        new MenuItem { Header = "Generate video with burned-in subtitles..." },
                        new MenuItem { Header = "Generate transparent video with subtitles..." },
                    }
                },
                new MenuItem
                {
                    Header = "Syn_chronization",
                    Items =
                    {
                        new MenuItem { Header = "_Adjust all times (show earlier/later)..." },
                        new MenuItem { Header = "_Change frame rate..." },
                        new MenuItem { Header = "_Change speed in percent" },
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
                            Command =vm.CommandShowSettingsCommand,
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
                            Command =vm.CommandShowAboutCommand,
                        },
                        new Separator(),
                        new MenuItem { Header = "_Help" },
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
                    Command =vm.CommandFileReopenCommand,
                };
                item.CommandParameter = file;
                vm.MenuReopen.Items.Add(item);
            }

            vm.MenuReopen.Items.Add(new Separator());

            var clearItem = new MenuItem
            {
                Header = "Clear recent files",
                Command =vm.CommandFileClearRecentFilesCommand,
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