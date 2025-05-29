using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.SpellCheck.GetDictionaries;

public partial class SpellCheckDictionaryDisplay : ObservableObject
{
    [ObservableProperty] private string _englishName;
    [ObservableProperty] private string _nativeName;
    [ObservableProperty] private string _downloadLink;
    [ObservableProperty] private string _description;

    public SpellCheckDictionaryDisplay()
    {
        EnglishName = string.Empty;
        NativeName = string.Empty;
        DownloadLink = string.Empty;
        Description = string.Empty;
    }

    public override string ToString()
    {
        if (EnglishName == NativeName || string.IsNullOrEmpty(NativeName) || NativeName.Length > 30)
        {
            return EnglishName;
        }

        return $"{EnglishName} ({NativeName})";
    }
}
