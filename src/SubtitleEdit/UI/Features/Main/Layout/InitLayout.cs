using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public static class InitLayout
{
    public static int MakeLayout(MainView mainPage, MainViewModel vm, int layoutNumber)
    {
        return layoutNumber switch
        {
            2 => MakeLayout2(mainPage, vm),
            3 => MakeLayout3(mainPage, vm),
            4 => MakeLayout4(mainPage, vm),
            5 => MakeLayout5(mainPage, vm),
            6 => MakeLayout6(mainPage, vm),
            7 => MakeLayout7(mainPage, vm),
            8 => MakeLayout8(mainPage, vm),
            9 => MakeLayout9(mainPage, vm),
            10 => MakeLayout10(mainPage, vm),
            11 => MakeLayout11(mainPage, vm),
            12 => MakeLayout12(mainPage, vm),
            _ => MakeLayout1(mainPage, vm)
        };
    }

    private static int MakeLayout1(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border
        {
            Background = Brushes.OrangeRed
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Video player",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Background = Brushes.MediumPurple,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Waveform",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 1;
    }

    private static int MakeLayout2(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border
        {
            Background = Brushes.OrangeRed
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Video player",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Background = Brushes.MediumPurple,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Waveform",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 2;
    }

    private static int MakeLayout3(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left content (will hold the nested grid)
        var leftContent = new Border
        {
            Background = Brushes.CornflowerBlue
        };
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Create a nested grid with rows (for horizontal split in left section)
        var nestedGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top part of nested grid
        var nestedTop = new Border
        {
            Background = Brushes.OrangeRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(nestedTop, 0);
        nestedGrid.Children.Add(nestedTop);

        // Horizontal GridSplitter in left section
        var nestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Bottom part of nested grid
        var nestedBottom = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Video player",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(nestedBottom, 2);
        nestedGrid.Children.Add(nestedBottom);

        // Add nested grid to left content
        leftContent.Child = nestedGrid;

        // Main GridSplitter (vertical)
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Right content
        var rightContent = new Border
        {
            Background = Brushes.MediumPurple,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Waveform",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 3;
    }

    private static int MakeLayout4(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left content (single panel, no splitter)
        var leftContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Main GridSplitter (vertical between left and right sections)
        var mainSplitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(mainSplitter, 1);
        contentGrid.Children.Add(mainSplitter);

        // Right content (will hold the nested grid for right section)
        var rightContent = new Border
        {
            Background = Brushes.MediumPurple
        };
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        // Create a nested grid with rows for right section (horizontal split)
        var rightNestedGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top part of right nested grid
        var rightNestedTop = new Border
        {
            Background = Brushes.LightSeaGreen,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Right Top",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(rightNestedTop, 0);
        rightNestedGrid.Children.Add(rightNestedTop);

        // Horizontal GridSplitter in right section
        var rightNestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(rightNestedSplitter, 1);
        rightNestedGrid.Children.Add(rightNestedSplitter);

        // Bottom part of right nested grid
        var rightNestedBottom = new Border
        {
            Background = Brushes.Gold,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Right Bottom",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(rightNestedBottom, 2);
        rightNestedGrid.Children.Add(rightNestedBottom);

        // Add right nested grid to right content
        rightContent.Child = rightNestedGrid;

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 4;
    }

    private static int MakeLayout5(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // First horizontal splitter (between top and middle)
        var firstSplitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(firstSplitter, 1);
        contentGrid.Children.Add(firstSplitter);

        // Middle section
        var middleContent = new Border
        {
            Background = Brushes.LightSeaGreen,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Middle Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(middleContent, 2);
        contentGrid.Children.Add(middleContent);

        // Second horizontal splitter (between middle and bottom)
        var secondSplitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(secondSplitter, 3);
        contentGrid.Children.Add(secondSplitter);

        // Bottom section
        var bottomContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Bottom Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 4);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 5;
    }

    private static int MakeLayout6(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Bottom Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 6;
    }

    private static int MakeLayout7(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left section
        var leftContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetColumn(leftContent, 0);
        contentGrid.Children.Add(leftContent);

        // Vertical splitter
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Right section
        var rightContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Right Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(rightContent, 2);
        contentGrid.Children.Add(rightContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 7;
    }

    private static int MakeLayout8(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Bottom Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 8;
    }

    private static int MakeLayout9(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Horizontal splitter
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom section
        var bottomContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Bottom Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 9;
    }

    private static int MakeLayout10(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top content (will hold the nested grid)
        var topContent = new Border
        {
            Background = Brushes.OrangeRed
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // Create a nested grid with columns (for vertical split)
        var nestedGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Left part of nested grid
        var nestedLeft = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedLeft, 0);
        nestedGrid.Children.Add(nestedLeft);

        // Vertical GridSplitter
        var nestedSplitter = new GridSplitter
        {
            Background = Brushes.Black,
            Width = 5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(nestedSplitter, 1);
        nestedGrid.Children.Add(nestedSplitter);

        // Right part of nested grid
        var nestedRight = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Video player",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetColumn(nestedRight, 2);
        nestedGrid.Children.Add(nestedRight);

        // Add nested grid to top content
        topContent.Child = nestedGrid;

        // Main GridSplitter (horizontal)
        var splitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(splitter, 1);
        contentGrid.Children.Add(splitter);

        // Bottom content
        var bottomContent = new Border
        {
            Background = Brushes.MediumPurple,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Waveform",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 2);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 10;
    }

    private static int MakeLayout11(MainView mainPage, MainViewModel vm)
    {
        var contentGrid = new Grid
        {
            Background = Brushes.White,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        // Top section
        var topContent = new Border
        {
            Background = Brushes.CornflowerBlue,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            MakeLayoutListViewAndEditBox(mainPage, vm)
                        }
                    }
                }
            }
        };
        Grid.SetRow(topContent, 0);
        contentGrid.Children.Add(topContent);

        // First horizontal splitter (between top and middle)
        var firstSplitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(firstSplitter, 1);
        contentGrid.Children.Add(firstSplitter);

        // Middle section
        var middleContent = new Border
        {
            Background = Brushes.LightSeaGreen,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Middle Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(middleContent, 2);
        contentGrid.Children.Add(middleContent);

        // Second horizontal splitter (between middle and bottom)
        var secondSplitter = new GridSplitter
        {
            Background = Brushes.DimGray,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(secondSplitter, 3);
        contentGrid.Children.Add(secondSplitter);

        // Bottom section
        var bottomContent = new Border
        {
            Background = Brushes.PaleVioletRed,
            Child = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Bottom Section",
                                FontSize = 50,
                                FontWeight = FontWeight.Bold,
                            },
                        }
                    }
                }
            }
        };
        Grid.SetRow(bottomContent, 4);
        contentGrid.Children.Add(bottomContent);

        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(contentGrid);

        return 11;
    }
    
    private static int MakeLayout12(MainView mainPage, MainViewModel vm)
    {
        vm.ContentGrid.Children.Clear();
        vm.ContentGrid.Children.Add(MakeLayoutListViewAndEditBox(mainPage, vm));

        return 12;
    }

    private static Grid MakeLayoutListViewAndEditBox(MainView mainPage, MainViewModel vm)
    {
        mainPage.DataContext = vm;

        // Create layout
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto")
        };

        // Header
        var headerText = new TextBlock
        {
            Text = "Subtitle Editor",
            FontSize = 24,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(10)
        };
        Grid.SetRow(headerText, 0);
        mainGrid.Children.Add(headerText);

        // DataGrid for subtitles
        vm.SubtitleGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true,
            IsReadOnly = false,
            SelectionMode = DataGridSelectionMode.Extended,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(10)
        };

        // Columns
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "#",
            Binding = new Binding("Number"),
            Width = new DataGridLength(50)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "Visible",
            Binding = new Binding("IsVisible"),
            Width = new DataGridLength(70)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Start Time",
            Binding = new Binding("StartTime"),
            Width = new DataGridLength(120)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "End Time",
            Binding = new Binding("EndTime"),
            Width = new DataGridLength(120)
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Duration",
            Binding = new Binding("Duration"),
            Width = new DataGridLength(120),
            IsReadOnly = true
        });
        vm.SubtitleGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Text",
            Binding = new Binding("Text"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        // Bind data
        vm.SubtitleGrid.ItemsSource = vm.Subtitles;
        vm.SubtitleGrid.SelectionChanged += vm.SubtitleGrid_SelectionChanged;

        Grid.SetRow(vm.SubtitleGrid, 1);
        mainGrid.Children.Add(vm.SubtitleGrid);

        // Text edit area
        var editPanel = new StackPanel
        {
            Margin = new Thickness(10),
            Spacing = 5
        };

        var editLabel = new TextBlock
        {
            Text = "Edit Selected Subtitle:",
            FontWeight = FontWeight.Bold
        };
        editPanel.Children.Add(editLabel);

        vm.EditTextBox = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 80
        };
        vm.EditTextBox.Bind(TextBox.TextProperty, new Binding("EditText") { Mode = BindingMode.TwoWay });
        vm.EditTextBox.Bind(InputElement.IsEnabledProperty, new Binding("IsSubtitleSelected"));
        //TODO: vm.EditTextBox.KeyDown += EditTextBox_KeyDown;

        editPanel.Children.Add(vm.EditTextBox);

        // Add save button
        var saveButton = new Button
        {
            Content = "Save Changes",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 5, 0, 0)
        };
        saveButton.Bind(Button.CommandProperty, new Binding("SaveChangesCommand"));
        saveButton.Bind(InputElement.IsEnabledProperty, new Binding("IsSubtitleSelected"));
        editPanel.Children.Add(saveButton);

        Grid.SetRow(editPanel, 2);
        mainGrid.Children.Add(editPanel);

        return mainGrid;
    }
}