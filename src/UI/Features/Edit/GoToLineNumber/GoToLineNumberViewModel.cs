using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Edit.GoToLineNumber;

public partial class GoToLineNumberViewModel : ObservableObject
{
    [ObservableProperty] private int _lineNumber;
    [ObservableProperty] private int _maxLineNumber;
    
    public Window? Window { get; set; }
    public NumericUpDown UpDown { get; set; }

    public bool OkPressed { get; private set; }

    public GoToLineNumberViewModel()
    {
        LineNumber = 1;
        MaxLineNumber = 100;
        UpDown = new NumericUpDown();   
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

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }

    public void Activated()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var textBox = UpDown.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault();
            textBox?.SelectAll();
            UpDown.Focus(); 
        });
    }
}