using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    public MultipleReplaceViewModel()
    {
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
        } ));
        subNodes.Add(new RuleTreeNode("Case sensitive", new MultipleReplaceRule()
        {
            Find = "1abc",
            ReplaceWith = "1cba",
            Description = "1this blaf balf",
            Active = false,
            Type = MultipleReplaceType.CaseSensitive,
        } ));
        subNodes.Add(new RuleTreeNode("Normal", new MultipleReplaceRule()
        {
            Find = "1abc",
            ReplaceWith = "1cba",
            Description = "1this blaf balf",
            Active = false,
            Type = MultipleReplaceType.Normal,
        } ));
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