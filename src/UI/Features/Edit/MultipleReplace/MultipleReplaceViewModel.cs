using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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
        Nodes = new ObservableCollection<RuleTreeNode>(GetNodes());
        RulesTreeView = new TreeView();
    }

    private IEnumerable<RuleTreeNode> GetNodes()
    {
        var nodes = new List<RuleTreeNode>();

        foreach (var category in Se.Settings.Edit.MultipleReplace.Categories)
        {
            var categoryNode = new RuleTreeNode(category.Name, new ObservableCollection<RuleTreeNode>(), category.IsActive)
            {
                IsCategory = true,
            };
            nodes.Add(categoryNode);

            foreach (var rule in category.Rules)
            {
                var node = new RuleTreeNode(string.Empty, rule);
                categoryNode.SubNodes?.Add(node);
            }
        }

        AddDefaultCategoryIfNone(nodes);

        return nodes;
    }

    private void SaveSettings()
    {
        Se.Settings.Edit.MultipleReplace.Categories.Clear();
        foreach (var category in Nodes)
        {
            var c = new SeEditMultipleReplace.MultipleReplaceCategory();
            c.Name = category.Title;
            c.IsActive = category.IsActive;
            Se.Settings.Edit.MultipleReplace.Categories.Add(c);

            foreach (var rule in category.SubNodes ?? [])
            {
                c.Rules.Add(new MultipleReplaceRule
                {
                    Active = rule.IsActive,
                    Description = rule.Description,
                    Find = rule.Find,
                    ReplaceWith = rule.ReplaceWith,
                    Type = rule.Type,
                });
            }
        }

        Se.SaveSettings();
    }

    private static void AddDefaultCategoryIfNone(List<RuleTreeNode> nodes)
    {
        if (nodes.Count == 0)
        {
            var defaultCategory = new RuleTreeNode(Se.Language.General.Default, new ObservableCollection<RuleTreeNode>(), true)
            {
                IsCategory = true,
            };
            nodes.Add(defaultCategory);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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
        if (node == null || !node.IsCategory)
        {
            return;
        }

        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = Se.Language.General.EditDotDotDot, Command = CategoryEditCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = Se.Language.Edit.MultipleReplace.NewCategory, Command = CategoryAddCategoryCommand, CommandParameter = node },
                new MenuItem { Header = Se.Language.Edit.MultipleReplace.NewRule, Command = CategoryAddRuleCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = Se.Language.General.MoveUp, Command = CategoryMoveUpCommand, CommandParameter = node },
                new MenuItem { Header = Se.Language.General.MoveDown, Command = CategoryMoveDownCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = Se.Language.General.Delete, Command = CategoryDeleteCommand, CommandParameter = node }
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
    private async Task CategoryAddCategory(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var category = new RuleTreeNode(string.Empty, new ObservableCollection<RuleTreeNode>(), true);
        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewCategory, category);
        });

        if (result.OkPressed)
        {
            category.Title = result.CategoryName;   
            Nodes.Add(category);
            SelectedNode = category; 
        }
    }

    [RelayCommand]
    private async Task CategoryAddRule(RuleTreeNode? node)
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
            var rule = new RuleTreeNode(string.Empty, new MultipleReplaceRule
            {
                Active = true,
                Description = result.Description,
                Find = result.FindWhat,
                ReplaceWith = result.ReplaceWith,
                Type = result.IsRegularExpression ? MultipleReplaceType.RegularExpression :
                       result.IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
                       MultipleReplaceType.CaseInsensitive,

            });
            node.SubNodes?.Add(rule);
            SelectedNode = rule;    
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
                new MenuItem { Header = Se.Language.Edit.MultipleReplace.EditRule, Command = NodeEditCommand, CommandParameter = node  },
                new Separator(),
                new MenuItem { Header = Se.Language.General.Duplicate, Command = NodeDuplicateCommand, CommandParameter = node },
                new MenuItem { Header = Se.Language.General.InsertBefore, Command = NodeInsertBeforeCommand, CommandParameter = node },
                new MenuItem { Header = Se.Language.General.InsertAfter, Command = NodeInsertAfterCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = Se.Language.General.MoveUp , Command = NodeMoveUpCommand, CommandParameter = node },
                new MenuItem { Header = Se.Language.General.MoveDown, Command = NodeMoveDownCommand, CommandParameter = node },
                new Separator(),
                new MenuItem { Header = Se.Language.General.Delete, Command = NodeDeleteCommand, CommandParameter = node }
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