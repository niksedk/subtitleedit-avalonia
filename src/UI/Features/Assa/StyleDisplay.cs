using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class StyleDisplay : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _fontName = string.Empty;
    [ObservableProperty] private decimal _fontSize;
    [ObservableProperty] private int _usageCount;
    [ObservableProperty] private Color _colorPrimary;
    [ObservableProperty] private Color _colorSecondary;
    [ObservableProperty] private Color _colorOutline;
    [ObservableProperty] private Color _colorShadow;
    [ObservableProperty] private decimal _outlineWidth;
    [ObservableProperty] private decimal _shadowWidth;
    [ObservableProperty] private bool _bold;
    [ObservableProperty] private bool _italic;
    [ObservableProperty] private bool _underline;
    [ObservableProperty] private bool _strikeout;
    [ObservableProperty] private decimal _scaleX;
    [ObservableProperty] private decimal _scaleY;
    [ObservableProperty] private decimal _spacing;
    [ObservableProperty] private decimal _angle;
    [ObservableProperty] private bool _alignmentAn1;
    [ObservableProperty] private bool _alignmentAn2;
    [ObservableProperty] private bool _alignmentAn3;
    [ObservableProperty] private bool _alignmentAn4;
    [ObservableProperty] private bool _alignmentAn5;
    [ObservableProperty] private bool _alignmentAn6;
    [ObservableProperty] private bool _alignmentAn7;
    [ObservableProperty] private bool _alignmentAn8;
    [ObservableProperty] private bool _alignmentAn9;
    [ObservableProperty] private decimal _marginLeft;
    [ObservableProperty] private decimal _marginRight;
    [ObservableProperty] private decimal _marginVertical;
    [ObservableProperty] private bool _useOpaqueBox;
    [ObservableProperty] private bool _useOpaqueBoxPerLine;

    public StyleDisplay()
    {
        _name = string.Empty;
    }
}
