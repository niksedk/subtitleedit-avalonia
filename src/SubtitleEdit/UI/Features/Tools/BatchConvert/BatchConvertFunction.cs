using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public class BatchConvertFunction
{
    public BatchConvertFunctionType Type { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public Control View { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public BatchConvertFunction(BatchConvertFunctionType type, string name, bool isSelected, Control view)
    {
        Type = type;
        Name = name;
        IsSelected = isSelected;
        View = view;
    }
}