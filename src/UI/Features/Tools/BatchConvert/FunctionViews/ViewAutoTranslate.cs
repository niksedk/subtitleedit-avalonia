using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAutoTranslate
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.AutoTranslate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var labelEngine = UiUtil.MakeLabel(Se.Language.General.Engine);
        var comboBoxEngine = UiUtil.MakeComboBox(vm.AutoTranslators, vm, nameof(vm.SelectedAutoTranslator));
        comboBoxEngine.SelectionChanged += (s, e) => vm.OnAutoTranslatorChanged();  
        var labelModel = UiUtil.MakeLabel(Se.Language.General.Model).WithBindVisible(vm, nameof(vm.AutoTranslateModelIsVisible)).WithMarginLeft(10).WithMarginRight(3);
        var textBoxModel =  UiUtil.MakeTextBox(150, vm, nameof(vm.AutoTranslateModelText), nameof(vm.AutoTranslateModelIsVisible));
        var buttonModel = UiUtil.MakeButtonBrowse(vm.AutoTranslateBrowseModelCommand, nameof(vm.AutoTranslateModelBrowseIsVisible)).WithMarginLeft(3);
        var panelEngineControls = UiUtil.MakeHorizontalPanel(
            comboBoxEngine,
            labelModel, 
            textBoxModel,
            buttonModel
            );

        var labelSourceLanguage = UiUtil.MakeLabel(Se.Language.General.From);
        var sourceLangCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage));

        var labelTargetLanguage = UiUtil.MakeLabel(Se.Language.General.To);
        var targetLangCombo = UiUtil.MakeComboBox(vm.TargetLanguages, vm, nameof(vm.SelectedTargetLanguage));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0);
        
        grid.Add(labelEngine, 1, 0);
        grid.Add(panelEngineControls, 1, 1);
        
        grid.Add(labelSourceLanguage, 2, 0);
        grid.Add(sourceLangCombo, 2, 1);
        
        grid.Add(labelTargetLanguage, 3, 0);
        grid.Add(targetLangCombo, 3, 1);

        return grid;
    }
}
