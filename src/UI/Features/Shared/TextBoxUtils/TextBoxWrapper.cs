using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;

public class TextBoxWrapper(TextBox textBox) : INikseTextBox
{
    public string Text
    {
        get
        {
            if (textBox.Text != null)
            {
                return textBox.Text;
            }

            return string.Empty;
        }
        set => textBox.Text = value;
    }

    public string SelectedText
    {
        get => textBox.SelectedText;    
        set => textBox.SelectedText = value;
    }

    public int SelectionStart
    {
        get => textBox.SelectionStart;
        set => textBox.SelectionStart = value;
    }

    public int SelectionLength
    {
        get => textBox.SelectionEnd - textBox.SelectionStart;
        set => textBox.SelectionEnd = textBox.SelectionStart + value;
    }
    
    public int SelectionEnd
    {
        get => textBox.SelectionEnd;
        set => textBox.SelectionEnd =  value;
    }

    public void Select(int start, int length)
    {
        textBox.SelectionStart = start;
        textBox.SelectionEnd = start + length;
    }

    public int CaretIndex
    {
        get => textBox.CaretIndex;
        set => textBox.CaretIndex = value;
    }

    public void Focus()
    {
        textBox.Focus();
    }

    public Control Control => textBox;
    public bool IsFocused { get; }
    public void Cut()
    {
        textBox.Cut();
    }

    public void Copy()
    {
        textBox.Copy();
    }

    public void Paste()
    {
        textBox.Paste();
    }

    public void SelectAll()
    {
        textBox.SelectAll();
    }

    public void ClearSelection()
    {
        textBox.ClearSelection();
    }
}