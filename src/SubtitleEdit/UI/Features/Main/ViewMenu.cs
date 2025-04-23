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
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Edit",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "Too_ls",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Spell Check",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Video",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "Syn_chronization",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Options",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Translate",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
                new MenuItem
                {
                    Header = "_Help",
                    Items =
                    {
                        new MenuItem { Header = "_New" },
                        new MenuItem { Header = "_Open..." },
                        new MenuItem { Header = "_Save" },
                        new Separator(),
                        new MenuItem
                        {
                            Header = "E_xit",
                            [!MenuItem.CommandProperty] = new Binding(nameof(vm.CommandExitCommand))
                        }
                    }
                },
            }
        };
    }
}