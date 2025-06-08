using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.EncodingSettings;

public class BatchConvertSettingsWindow : Window
{
    private BatchConvertSettingsViewModel _vm;
    
    public BatchConvertSettingsWindow(BatchConvertSettingsViewModel vm)
    {
        Icon = UiUtil.GetSeIcon();
        Title = "Batch convert - output settings";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        
        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var checkBoxUseSourceFolder = new CheckBox
        {
            Content = "Use source folder",
            IsChecked = vm.UseSourceFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.UseSourceFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
        };

        var checkBoxUseOutputFolder = new CheckBox
        {
            Content = "Use output folder",
            IsChecked = vm.UseOutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.UseOutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
        };

        var textBoxOutputFolder = new TextBox
        {
            Text = vm.OutputFolder,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.OutputFolder)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
            IsEnabled = vm.UseOutputFolder,
        };

        var buttonBrowse = new Button
        {
            Content = "Browse...",
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = vm.UseOutputFolder,
        };

        var checkBoxOverwrite = new CheckBox
        {
            Content = "Overwrite existing files",
            IsChecked = vm.Overwrite,
            VerticalAlignment = VerticalAlignment.Center,
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.Overwrite)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },  
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        //grid.Add(label, 0, 0);
        //grid.Add(comboEncoding, 0, 1);
        //grid.Add(checkBoxStereo, 1, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;
        
        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
