using System.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

public class TextEditorBindingCoordinator
{
    private readonly MainViewModel _vm;
    private readonly TextEditorBindingHelper? _textEditorHelper;
    private readonly TextEditorBindingHelper? _originalTextEditorHelper;

    public TextEditorBindingCoordinator(
        MainViewModel vm,
        TextEditorBindingHelper? textEditorHelper,
        TextEditorBindingHelper? originalTextEditorHelper)
    {
        _vm = vm;
        _textEditorHelper = textEditorHelper;
        _originalTextEditorHelper = originalTextEditorHelper;
    }

    public void Initialize()
    {
        _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void DeInitialize()
    {
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_vm.SelectedSubtitle))
        {
            _textEditorHelper?.OnSelectedSubtitleChanged();
            _originalTextEditorHelper?.OnSelectedSubtitleChanged();
        }
    }
}
