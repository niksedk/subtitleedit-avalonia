using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main;

public static class ViewMenu
{
    public static Menu Make(MainViewModel vm)
    {
        return new Menu
        {
            Height = 30,
            Background = Brushes.Orange,
            DataContext = vm,
            Items =
            {
                new MenuItem
                {
                    Header = "_File",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new Separator(),
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Reopen" },
                        new MenuItem { Header = "Restore auto-backup..." },
                        new Separator(),
                        new MenuItem { Header = "_Save" },
                        new MenuItem { Header = "Save _as..." },
                        new Separator(),
                        new MenuItem { Header = "Export" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand)),
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
                        new MenuItem { Header = "_Open video file..." },
                        new MenuItem { Header = "Open video file from _URL..." },
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
                        new MenuItem { Header = "_Settings..." },
                    }
                },
                new MenuItem
                {
                    Header = "_Translate",
                    Items =
                    {
                        new MenuItem { Header = "_Auto-translate..." },
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
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandShowAboutCommand)),
                        },
                        new Separator(),
                        new MenuItem { Header = "_Help" },
                    }
                },
            }
        };
    }
}