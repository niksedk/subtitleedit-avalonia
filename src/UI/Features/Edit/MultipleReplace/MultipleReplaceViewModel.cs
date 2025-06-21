using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

public partial class MultipleReplaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<MultipleReplaceFix> _fixes;
    [ObservableProperty] private MultipleReplaceFix? _selectedFix;
    [ObservableProperty] private RuleTreeNode? _selectedNode;
    public ObservableCollection<RuleTreeNode> Nodes { get; }
    public TreeView RulesTreeView { get; internal set; }
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private IWindowService _windowService;

    public MultipleReplaceViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Fixes = new ObservableCollection<MultipleReplaceFix>();
        Nodes = new ObservableCollection<RuleTreeNode>();
        RulesTreeView = new TreeView();

        var subNodes = new ObservableCollection<RuleTreeNode>();
        subNodes.Add(new RuleTreeNode("regex", new MultipleReplaceRule()
        {
            Find = "abc",
            ReplaceWith = "cba",
            Description = "this blaf balf",
            Active = true,
            Type = MultipleReplaceType.RegularExpression,
        }));
        subNodes.Add(new RuleTreeNode("Case sensitive", new MultipleReplaceRule()
        {
            Find = "1abc",
            ReplaceWith = "1cba",
            Description = "1this blaf balf",
            Active = false,
            Type = MultipleReplaceType.CaseSensitive,
        }));
        subNodes.Add(new RuleTreeNode("Normal", new MultipleReplaceRule()
        {
            Find = "1abc",
            ReplaceWith = "1cba",
            Description = "1this blaf balf",
            Active = false,
            Type = MultipleReplaceType.CaseInsensitive,
        }));
        var n1 = new RuleTreeNode("Default", new ObservableCollection<RuleTreeNode>(subNodes), true);
        Nodes.Add(n1);
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void NodeCategoryOpenContextMenu(RuleTreeNode? node)
    {
        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Edit", Command = CategoryEditCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = "Duplicate", Command = CategoryDuplicateCommand, CommandParameter = node },
                new MenuItem { Header = "Insert Before", Command = CategoryInsertBeforeCommand, CommandParameter = node },
                new MenuItem { Header = "Insert After", Command = CategoryInsertAfterCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = "Move up", Command = CategoryMoveUpCommand, CommandParameter = node },
                new MenuItem { Header = "Move down", Command = CategoryMoveDownCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = "Delete", Command = CategoryDeleteCommand, CommandParameter = node }
            }
        };

        RulesTreeView.ContextMenu = contextMenu;
        contextMenu.Open();
    }

    [RelayCommand]
    private async Task CategoryEdit(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.EditCategory, node);
        });

        if (result.OkPressed)
        {
            node.Title = result.CategoryName;
        }
    }


    [RelayCommand]
    private void CategoryDuplicate(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }
    }


    [RelayCommand]
    private async Task CategoryInsertBefore(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewCategory, node);
        });

        if (result.OkPressed)
        {
        }
    }

    [RelayCommand]
    private async Task CategoryInsertAfter(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewCategory, node);
        });

        if (result.OkPressed)
        {
        }
    }

    [RelayCommand]
    private void CategoryDelete(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        Nodes.Remove(node);
    }

    [RelayCommand]
    private void CategoryMoveUp(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index > 0)
        {
            Nodes.Move(index, index - 1);
        }
    }

    [RelayCommand]
    private void CategoryMoveDown(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index < Nodes.Count - 1)
        {
            Nodes.Move(index, index + 1);
        }
    }

    [RelayCommand]
    private void NodeOpenContextMenu(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Edit", Command = NodeEditCommand, CommandParameter = node  },
                new Separator(),
                new MenuItem { Header = "Duplicate", Command = NodeDuplicateCommand, CommandParameter = node },
                new MenuItem { Header = "Insert Before", Command = NodeInsertBeforeCommand, CommandParameter = node },
                new MenuItem { Header = "Insert After", Command = NodeInsertAfterCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = "Move up", Command = NodeMoveUpCommand, CommandParameter = node },
                new MenuItem { Header = "Move down", Command = NodeMoveDownCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = "Delete", Command = NodeDeleteCommand, CommandParameter = node }
            }
        };

        RulesTreeView.ContextMenu = contextMenu;
        contextMenu.Open();
    }

    [RelayCommand]
    private async Task NodeEdit(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.EditRule, node);
        });

        if (result.OkPressed)
        {
            node.Find = result.FindWhat;
            node.ReplaceWith = result.ReplaceWith;
            node.Description = result.Description;
            if (result.IsRegularExpression)
            {
                node.Type = MultipleReplaceType.RegularExpression;
            }
            else if (result.IsCaseSensitive)
            {
                node.Type = MultipleReplaceType.CaseSensitive;
            }
            else
            {
                node.Type = MultipleReplaceType.CaseInsensitive;
            }
        }
    }

    [RelayCommand]
    private void NodeDuplicate(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task NodeInsertBefore(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node);
        });

        if (result.OkPressed)
        {
        }
    }

    [RelayCommand]
    private async Task NodeInsertAfter(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node);
        });

        if (result.OkPressed)
        {
        }
    }

    [RelayCommand]
    private void NodeDelete(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        Nodes.Remove(node);
    }

    [RelayCommand]
    private void NodeMoveUp(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index > 0)
        {
            Nodes.Move(index, index - 1);
        }
    }

    [RelayCommand]
    private void NodeMoveDown(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var index = Nodes.IndexOf(node);
        if (index < Nodes.Count - 1)
        {
            Nodes.Move(index, index + 1);
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    public void ExpandAll()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var allTreeViewItems = FindAllTreeViewItems(RulesTreeView);
            foreach (var item in allTreeViewItems)
            {
                item.IsExpanded = true;
            }
        }, DispatcherPriority.Background);
    }

    private IEnumerable<TreeViewItem> FindAllTreeViewItems(Control parent)
    {
        var result = new List<TreeViewItem>();
        if (parent is TreeViewItem tvi)
        {
            result.Add(tvi);
        }

        foreach (var child in parent.GetLogicalDescendants())
        {
            if (child is TreeViewItem treeViewItem)
            {
                result.Add(treeViewItem);
            }
        }

        return result;
    }

    public void RulesTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        //throw new System.NotImplementedException();
    }

    internal void OnLoaded(RoutedEventArgs e)
    {
        ExpandAll();
    }
}