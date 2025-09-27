using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class PromptUnknownWordWindow : Window
{
    public PromptUnknownWordWindow(PromptUnknownWordViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.SpellCheck.SpellCheck;
        CanResize = true;
        Width = 500;
        Height = 500;
        MinWidth = 400;
        MinHeight = 200;
        vm.Window = this;
        DataContext = vm;

        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Abort);
        var panelButtons = UiUtil.MakeButtonBar(buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // image
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // whole text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // word + suggestions
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons (abort)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var image = new Image
        {
            Stretch = Avalonia.Media.Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 200,
            DataContext = vm,
        };  
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.Bitmap)));

        grid.Add(image, 0);
        grid.Add(MakeWholeTextView(vm), 1);
        grid.Add(MakeWordView(vm), 2);
        grid.Add(MakeWordSuggestionsView(vm), 2, 1, 1);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonCancel.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private Control MakeWordSuggestionsView(PromptUnknownWordViewModel vm)
    {
        return UiUtil.MakeLabel("Not implemented: MakeWordSuggestionsView");
    }

    private Control MakeWordView(PromptUnknownWordViewModel vm)
    {
        return UiUtil.MakeLabel("Not implemented: MakeWordView");
    }

    private Control MakeWholeTextView(PromptUnknownWordViewModel vm)
    {
        return UiUtil.MakeLabel("Not implemented: MakeWholeTextView");
    }
}
