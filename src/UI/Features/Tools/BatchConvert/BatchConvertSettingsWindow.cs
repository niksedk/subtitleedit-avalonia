using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertSettingsWindow : Window
{
    private readonly Tools.BatchConvert.BatchConvertSettingsViewModel _vm;
    
    public BatchConvertSettingsWindow(Tools.BatchConvert.BatchConvertSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Batch convert - output settings";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelTargetEncoding = UiUtil.MakeLabel("Target encoding").WithMarginLeft(5);
        var comboBoxTargetEncoding = UiUtil.MakeComboBox(vm.TargetEncodings, vm, nameof(vm.SelectedTargetEncoding));
        var panelTargetEncoding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { labelTargetEncoding, comboBoxTargetEncoding  }
        };

        var checkBoxOverwrite = new CheckBox
        {
            Content = "Overwrite existing files",
            IsChecked = vm.Overwrite,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.Overwrite)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
        };

        var checkBoxUseSourceFolder = new RadioButton
        {
            Content = "Use source folder",
            IsChecked = vm.UseSourceFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseSourceFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
        };

        var checkBoxUseOutputFolder = new RadioButton
        {
            Content = "Use output folder",
            IsChecked = vm.UseOutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
        };

        var textBoxOutputFolder = new TextBox
        {
            Text = vm.OutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
            IsEnabled = vm.UseOutputFolder,
            Width = 400,
        };

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseOutputFolderCommand);

        var panelOutputFolder = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                textBoxOutputFolder,
                buttonBrowse
            }
        };  

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelTargetEncoding, 0, 0);
        grid.Add(checkBoxOverwrite, 1, 0);
        grid.Add(checkBoxUseSourceFolder, 2, 0);
        grid.Add(checkBoxUseOutputFolder, 3, 0);
        grid.Add(panelOutputFolder, 4, 0);
        grid.Add(panelButtons, 5, 0);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
