using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class FixCommonErrorsProfileWindow : Window
{
    private readonly FixCommonErrorsProfileViewModel _vm;

    public FixCommonErrorsProfileWindow(FixCommonErrorsProfileViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Fix common errors profile";
        Width = 910;
        Height = 640;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("2*,3*"),
                RowDefinitions = new RowDefinitions("Auto,*")
            };

            // Header
            var header = new TextBlock
            {
                Text = "Profile Manager",
                FontSize = 24,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            grid.Children.Add(header);
            Grid.SetRow(header, 0);
            Grid.SetColumnSpan(header, 2);

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
                        SelectedItem = vm.SelectedProfile,
                        ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
                        {
                            var row = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 5
                            };

                            row.Children.Add(new TextBlock
                            {
                                Text = profile.Name,
                                VerticalAlignment = VerticalAlignment.Center
                            });

                            row.Children.Add(new Button
                            {
                                Content = "🗑",
                                Width = 24,
                                Height = 24,
                                Margin = new Thickness(5, 0),
                                Command = vm.DeleteCommand,
                                CommandParameter = profile
                            });

                            return row;
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
                Text = "Edit Profile",
                FontSize = 20
            };
            editorGrid.Children.Add(heading);
            Grid.SetRow(heading, 0);

            // Name textbox
            var nameBox = new TextBox
            {
                Watermark = "Profile Name"
            };
            nameBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.SelectedProfile.Name)));
            editorGrid.Children.Add(nameBox);
            Grid.SetRow(nameBox, 1);

            // "Rules" label
            var rulesLabel = new TextBlock
            {
                Text = "Rules",
                FontSize = 16
            };
            editorGrid.Children.Add(rulesLabel);
            Grid.SetRow(rulesLabel, 2);

            // Scrollable rule list
            var scrollViewer = new ScrollViewer
            {
                Content = new ItemsControl
                {
                    DataContext = vm,
                    ItemsSource = vm.SelectedProfile?.FixRules,
                    ItemTemplate = new FuncDataTemplate<FixRuleDisplayItem>((rule, _) =>
                    {
                        var ruleRow = new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitions("Auto,*,*"),
                            Margin = new Thickness(0, 2)
                        };

                        var checkbox = new CheckBox { IsChecked = rule.IsSelected };
                        ruleRow.Children.Add(checkbox);
                        Grid.SetColumn(checkbox, 0);

                        var nameText = new TextBlock
                        {
                            Text = rule.Name,
                            FontWeight = FontWeight.Bold
                        };
                        ruleRow.Children.Add(nameText);
                        Grid.SetColumn(nameText, 1);

                        var exampleText = new TextBlock
                        {
                            Text = rule.Example,
                            FontStyle = FontStyle.Italic
                        };
                        ruleRow.Children.Add(exampleText);
                        Grid.SetColumn(exampleText, 2);

                        return ruleRow;
                    })
                }
            };
            editorGrid.Children.Add(scrollViewer);
            Grid.SetRow(scrollViewer, 3);

            // Save/Cancel buttons
            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Children =
                {
                    new Button
                    {
                        Content = "OK",
                        Command = vm.OkCommand
                    },
                    new Button
                    {
                        Content = "Cancel",
                        Command = vm.CancelCommand
                    }
                }
            };
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