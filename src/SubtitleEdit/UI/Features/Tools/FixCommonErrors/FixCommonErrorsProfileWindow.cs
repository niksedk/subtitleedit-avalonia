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
    private FixCommonErrorsProfileViewModel _vm;

    public FixCommonErrorsProfileWindow(FixCommonErrorsProfileViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Fix common errors profile";
        Width = 510;
        Height = 440;
        CanResize = false;

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
        Grid.SetRow(header, 0);
        Grid.SetColumnSpan(header, 2);
        grid.Children.Add(header);

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
                    Command = vm.NewProfileCommand,
                },
                new ListBox
                {
                    [!ItemsControl.ItemsSourceProperty] = new Binding("Profiles"),
                    [!SelectingItemsControl.SelectedItemProperty] = new Binding("SelectedProfile"),
                    ItemTemplate = new FuncDataTemplate<ProfileDisplayItem>((profile, _) =>
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            Children =
                            {
                                new TextBlock
                                {
                                    [!TextBlock.TextProperty] = new Binding("Name"),
                                    VerticalAlignment = VerticalAlignment.Center
                                },
                                new Button
                                {
                                    Content = "🗑",
                                    Width = 24,
                                    Height = 24,
                                    Margin = new Thickness(5, 0),
                                    Command = vm.DeleteCommand,
                                    [!Button.CommandParameterProperty] = new Binding(".")
                                }
                            }
                        })
                }
            }
        };
        Grid.SetRow(profileListPanel, 1);
        Grid.SetColumn(profileListPanel, 0);
        grid.Children.Add(profileListPanel);

        // Right column: Profile editor
        var editorPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 10
        };

        editorPanel.Children.Add(new TextBlock
        {
            Text = "Edit Profile",
            FontSize = 20
        });

        editorPanel.Children.Add(new TextBox
        {
            Watermark = "Profile Name",
            [!TextBox.TextProperty] = new Binding("SelectedProfile.Name")
        });

        editorPanel.Children.Add(new TextBlock
        {
            Text = "Rules",
            FontSize = 16
        });

        editorPanel.Children.Add(new ItemsControl
        {
            [!ItemsControl.ItemsSourceProperty] = new Binding("SelectedProfile.FixRules"),
            ItemTemplate = new FuncDataTemplate<FixRuleDisplayItem>((rule, _) =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*,*"),
                    Margin = new Thickness(0, 2)
                };

                var checkbox = new CheckBox
                {
                    [!ToggleButton.IsCheckedProperty] = new Binding("IsSelected")
                };
                Grid.SetColumn(checkbox, 0);

                var nameText = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("Name"),
                    FontWeight = FontWeight.Bold
                };
                Grid.SetColumn(nameText, 1);

                var exampleText = new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("Example"),
                    FontStyle = FontStyle.Italic
                };
                Grid.SetColumn(exampleText, 2);

                grid.Children.Add(checkbox);
                grid.Children.Add(nameText);
                grid.Children.Add(exampleText);

                return grid;
            })
        });

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
                    Command = vm.OkCommand,
                },
                new Button
                {
                    Content = "Cancel",
                    Command = vm.CancelCommand,
                }
            }
        };

        editorPanel.Children.Add(buttonRow);

        // Visibility binding (optional)
        editorPanel.Bind(IsVisibleProperty, new Binding("IsProfileSelected"));

        Grid.SetRow(editorPanel, 1);
        Grid.SetColumn(editorPanel, 1);
        grid.Children.Add(editorPanel);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}