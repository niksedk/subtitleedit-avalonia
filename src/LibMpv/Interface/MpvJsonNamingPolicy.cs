using System.Globalization;
using System.Text.Json;

namespace HanumanInstitute.LibMpv;

/// <summary>
/// Naming policy to convert 'PropertyName' into 'property-name'.
/// </summary>
public class MpvJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        name.CheckNotNullOrEmpty(nameof(name));

        var result = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (i == 0)
            {
                result.Append(char.ToLower(c, CultureInfo.InvariantCulture));
            }
            else
            {
                if (char.IsUpper(c))
                {
                    result.Append('-');
                    result.Append(char.ToLower(c, CultureInfo.InvariantCulture));
                }
                else
                {
                    result.Append(c);
                }
            }
        }
        return result.ToString();
    }
}