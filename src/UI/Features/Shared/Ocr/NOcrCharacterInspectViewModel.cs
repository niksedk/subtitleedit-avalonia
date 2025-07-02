using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.Ocr;

public partial class NOcrCharacterInspectViewModel : ObservableObject
{
    public NOcrCharacterInspectWindow? Window { get; internal set; }
}
