using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Projektanker.Icons.Avalonia;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsProfileWindow : Window
{
    private readonly FixCommonErrorsProfileViewModel _vm;

    public FixCommonErrorsProfileWindow(FixCommonErrorsProfileViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Fix common errors profile";
        Width = 1000;
        Height = 740;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,3*"),
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = UiUtil.MakeWindowMargin(),
        };

        // Left column: Profile list
        var profileListPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10,
            Children =
            {
                new Button
                {
                    Content = "New Profile",
                    Command = vm.NewProfileCommand
                },
                new ListBox
                {
                    DataContext = vm,
                    ItemsSource = vm.Profiles,
                    [!ListBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedProfile)) { Mode = BindingMode.TwoWay },
                    ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
                    {
                       var grid = new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                            Margin = new Thickness(2)
                        };

                        // Name text (left-aligned)
                        var nameText = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            [!TextBlock.TextProperty] = new Binding(nameof(ProfileDisplayItem.Name))
                        };
                        Grid.SetColumn(nameText, 0);
                        grid.Children.Add(nameText);

                        // Delete button (right-aligned)
                        var deleteButton = new Button
                        {
                            Command = vm.DeleteCommand,
                            CommandParameter = profile,
                        };
                        Attached.SetIcon(deleteButton, "fa-solid fa-trash");
                        Grid.SetColumn(deleteButton, 1);
                        grid.Children.Add(deleteButton);

                        return grid;
                    })
                }
            }
        };
        grid.Children.Add(profileListPanel);
        Grid.SetRow(profileListPanel, 1);
        Grid.SetColumn(profileListPanel, 0);

        // Right column: Profile editor
        var editorGrid = new Grid
        {
            Margin = new Thickness(10),
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*,Auto"),
        };

        // "Edit Profile" heading
        var heading = new TextBlock
        {
            Text = "Profile name",
            FontSize = 20
        };
        editorGrid.Children.Add(heading);
        Grid.SetRow(heading, 0);

        // Name textbox
        var nameBox = new TextBox
        {
            Watermark = "Enter profile name"
        };
        nameBox.Bind(TextBox.TextProperty, new Binding($"{nameof(vm.SelectedProfile)}.{nameof(ProfileDisplayItem.Name)}"));
        editorGrid.Children.Add(nameBox);
        Grid.SetRow(nameBox, 1);
        vm.ProfileNameTextBox = nameBox;

        // "Rules" label
        var rulesLabel = new TextBlock
        {
            Text = "Rules",
            FontSize = 16,
            Padding = new Thickness(0, 20, 0, 0),
        };
        editorGrid.Children.Add(rulesLabel);
        Grid.SetRow(rulesLabel, 2);

        // Scrollable rule list
        var scrollViewer = new ScrollViewer
        {
            Content = new ItemsControl
            {
                DataContext = vm,
                // ItemsSource = vm.SelectedProfile?.FixRules,
                [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.SelectedProfile) + "." + nameof(ProfileDisplayItem.FixRules)) { Mode = BindingMode.TwoWay },
                ItemTemplate = new FuncDataTemplate<FixRuleDisplayItem>((rule, _) =>
                {
                    var ruleRow = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("Auto,*,*"),
                        Margin = new Thickness(0, 2)
                    };

                    var checkbox = new CheckBox
                    {
                        [!CheckBox.IsCheckedProperty] = new Binding(nameof(FixRuleDisplayItem.IsSelected)),
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    ruleRow.Children.Add(checkbox);
                    Grid.SetColumn(checkbox, 0);

                    var nameText = new TextBlock
                    {
                        Text = rule.Name,
                        FontWeight = FontWeight.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    ruleRow.Children.Add(nameText);
                    Grid.SetColumn(nameText, 1);

                    var exampleText = new TextBlock
                    {
                        Text = rule.Example,
                        FontStyle = FontStyle.Italic,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    ruleRow.Children.Add(exampleText);
                    Grid.SetColumn(exampleText, 2);

                    return ruleRow;
                })
            }
        };
        editorGrid.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, 3);

        var buttonRow = UiUtil.MakeButtonBar(
            UiUtil.MakeButton("OK", vm.OkCommand),
            UiUtil.MakeButton("Cancel", vm.CancelCommand)
        );
        Grid.SetRow(buttonRow, 4);
        editorGrid.Children.Add(buttonRow);

        // Hide if no profile is selected
        editorGrid.Bind(IsVisibleProperty, new Binding(nameof(vm.IsProfileSelected)));

        Grid.SetRow(editorGrid, 1);
        Grid.SetColumn(editorGrid, 1);
        grid.Children.Add(editorGrid);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}