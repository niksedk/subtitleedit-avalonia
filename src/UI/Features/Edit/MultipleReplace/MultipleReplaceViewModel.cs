using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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
    public MultipleReplaceWindow? Window { get; set; }
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
    private void NodeCategoryOpenContextMenu(RuleTreeNode node)
    {
        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Edit", Command = CategoryEditCommand },
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
    private async Task CategoryEdit()
    {
        var node = SelectedNode;
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
        }
    }


    [RelayCommand]
    private void CategoryDuplicate()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }


    [RelayCommand]
    private async Task CategoryInsertBefore()
    {
        var node = SelectedNode;
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
    private async Task CategoryInsertAfter()
    {
        var node = SelectedNode;
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
    private void CategoryDelete()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void CategoryMoveUp()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void CategoryMoveDown()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void NodeOpenContextMenu()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
        
        var contextMenu = new ContextMenu
        {
            Items =
            {
                new MenuItem { Header = "Edit", Command = NodeEditCommand },
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
    private async Task NodeEdit()
    {
        var node = SelectedNode;
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
        }
    }


    [RelayCommand]
    private void NodeDuplicate()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private async Task NodeInsertBefore()
    {
        var node = SelectedNode;
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
    private async Task NodeInsertAfter()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void NodeDelete()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void NodeMoveUp()
    {
        var node = SelectedNode;
        if (node == null)
        {
            return;
        }
    }

    [RelayCommand]
    private void NodeMoveDown()
    { 
        var node = SelectedNode;
        if (node == null)
        {
            return;
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

    public void RulesTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        //throw new System.NotImplementedException();
    }
}