using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
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
    private Subtitle _subtitle = new Subtitle();
    private readonly Dictionary<string, Regex> _compiledRegExList;

    public MultipleReplaceViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        Fixes = new ObservableCollection<MultipleReplaceFix>();
        Nodes = new ObservableCollection<RuleTreeNode>(GetNodes());
        RulesTreeView = new TreeView();

        _compiledRegExList = new Dictionary<string, Regex>();
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;
    }

    private IEnumerable<RuleTreeNode> GetNodes()
    {
        var nodes = new List<RuleTreeNode>();

        foreach (var category in Se.Settings.Edit.MultipleReplace.Categories)
        {
            var categoryNode = new RuleTreeNode(null, category.Name, new ObservableCollection<RuleTreeNode>(), category.IsActive)
            {
                IsCategory = true,
            };
            nodes.Add(categoryNode);

            foreach (var rule in category.Rules)
            {
                var node = new RuleTreeNode(categoryNode, rule);
                node.Parent = categoryNode;
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
            c.Name = category.CategoryName;
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
            var defaultCategory = new RuleTreeNode(null, Se.Language.General.Default, new ObservableCollection<RuleTreeNode>(), true)
            {
                IsCategory = true,
            };
            nodes.Add(defaultCategory);
        }
    }

    private void AddDefaultCategoryIfNone()
    {
        if (Nodes.Count == 0)
        {
            var defaultCategory = new RuleTreeNode(null, Se.Language.General.Default, new ObservableCollection<RuleTreeNode>(), true)
            {
                IsCategory = true,
            };
            Nodes.Add(defaultCategory);
            SelectedNode = defaultCategory;
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
            node.CategoryName = result.CategoryName;
        }
    }

    [RelayCommand]
    private async Task CategoryAddCategory(RuleTreeNode? node)
    {
        if (node == null)
        {
            return;
        }

        var category = new RuleTreeNode(node, string.Empty, new ObservableCollection<RuleTreeNode>(), true);
        var result = await _windowService.ShowDialogAsync<EditCategoryWindow, EditCategoryViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewCategory, category);
        });

        if (result.OkPressed)
        {
            category.CategoryName = result.CategoryName;
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
            var rule = MakeRuleTreeNode(node, result);
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
        AddDefaultCategoryIfNone();
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
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index >= 0)
        {
            nodes.Insert(index, new RuleTreeNode(node.Parent, new MultipleReplaceRule
            {
                Active = node.IsActive,
                Description = node.Description,
                Find = node.Find,
                ReplaceWith = node.ReplaceWith,
                Type = node.Type,
            }));
        }
    }

    [RelayCommand]
    private async Task NodeInsertBefore(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node);
        });

        if (result.OkPressed)
        {
            var nodes = node.Parent.SubNodes;
            var index = nodes.IndexOf(node);
            var rule = MakeRuleTreeNode(node, result);
            nodes.Insert(index, rule);
            SelectedNode = rule;
        }
    }

    [RelayCommand]
    private async Task NodeInsertAfter(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditRuleWindow, EditRuleViewModel>(Window!, vm =>
        {
            vm.Initialize(Se.Language.Edit.MultipleReplace.NewRule, node);
        });

        if (result.OkPressed)
        {
            var nodes = node.Parent.SubNodes;
            var index = nodes.IndexOf(node);
            var rule = MakeRuleTreeNode(node, result);
            nodes.Insert(index + 1, rule);
            SelectedNode = rule;
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
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index > 0)
        {
            nodes.Move(index, index - 1);
        }
    }

    [RelayCommand]
    private void NodeMoveDown(RuleTreeNode? node)
    {
        if (node == null || node.Parent == null || node.Parent.SubNodes == null)
        {
            return;
        }

        var nodes = node.Parent.SubNodes;
        var index = nodes.IndexOf(node);
        if (index < nodes.Count - 1)
        {
            nodes.Move(index, index + 1);
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

    private static IEnumerable<TreeViewItem> FindAllTreeViewItems(Control parent)
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

    private static RuleTreeNode MakeRuleTreeNode(RuleTreeNode node, EditRuleViewModel result)
    {
        return new RuleTreeNode(node.Parent, new MultipleReplaceRule
        {
            Active = node.IsActive,
            Description = result.Description,
            Find = result.FindWhat,
            ReplaceWith = result.ReplaceWith,
            Type = result.IsRegularExpression ? MultipleReplaceType.RegularExpression :
                               result.IsCaseSensitive ? MultipleReplaceType.CaseSensitive :
                               MultipleReplaceType.CaseInsensitive,
        });
    }

    private void GeneratePreview()
    {
        var FixedSubtitle = new Subtitle(_subtitle, false);
        int fixedLines = 0;
        var replaceExpressions = new HashSet<ReplaceExpression>();
        foreach (var group in Nodes.Where(p => p.IsActive && p.SubNodes != null))
        {
            foreach (var rule in group.SubNodes!)
            {
                if (rule.IsActive)
                {
                    string findWhat = rule.Find;
                    if (!string.IsNullOrEmpty(findWhat)) // allow space or spaces
                    {
                        string replaceWith = RegexUtils.FixNewLine(rule.ReplaceWith);
                        findWhat = RegexUtils.FixNewLine(findWhat);
                        var ruleInfo = string.IsNullOrEmpty(rule.Description) ? $"Group name: {group.CategoryName} - Rule number: {group.SubNodes.IndexOf(rule) + 1}" : $"Group name: {group.CategoryName} - Rule number: {group.SubNodes.IndexOf(rule) + 1}. {rule.Description}";
                        var mpi = new ReplaceExpression(findWhat, replaceWith, rule.Type.ToString(), ruleInfo);
                        replaceExpressions.Add(mpi);
                        if (mpi.SearchType == ReplaceExpression.SearchRegEx && !_compiledRegExList.ContainsKey(findWhat))
                        {
                            _compiledRegExList.Add(findWhat, new Regex(findWhat, RegexOptions.Compiled | RegexOptions.Multiline));
                        }
                    }
                }
            }
        }

        for (var i = 0; i < _subtitle.Paragraphs.Count; i++)
        {
            Paragraph p = _subtitle.Paragraphs[i];
            bool hit = false;
            string newText = p.Text;
            string ruleInfo = string.Empty;
            foreach (ReplaceExpression item in replaceExpressions)
            {
                if (item.SearchType == ReplaceExpression.SearchCaseSensitive)
                {
                    if (newText.Contains(item.FindWhat))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = newText.Replace(item.FindWhat, item.ReplaceWith);
                    }
                }
                else if (item.SearchType == ReplaceExpression.SearchRegEx)
                {
                    Regex r = _compiledRegExList[item.FindWhat];
                    if (r.IsMatch(newText))
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        newText = RegexUtils.ReplaceNewLineSafe(r, newText, item.ReplaceWith);
                    }
                }
                else
                {
                    int index = newText.IndexOf(item.FindWhat, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        hit = true;
                        ruleInfo = string.IsNullOrEmpty(ruleInfo) ? item.RuleInfo : $"{ruleInfo} + {item.RuleInfo}";
                        do
                        {
                            newText = newText.Remove(index, item.FindWhat.Length).Insert(index, item.ReplaceWith);
                            index = newText.IndexOf(item.FindWhat, index + item.ReplaceWith.Length, StringComparison.OrdinalIgnoreCase);
                        } while (index >= 0);
                    }
                }
            }

            if (hit && newText != p.Text)
            {
                fixedLines++;
                //fixes.Add(MakePreviewListItem(p, newText, ruleInfo));
                FixedSubtitle.Paragraphs[i].Text = newText;
            }
        }

        //groupBoxLinesFound.Text = string.Format(LanguageSettings.Current.MultipleReplace.LinesFoundX, fixedLines);
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