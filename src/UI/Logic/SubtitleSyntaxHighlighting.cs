using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic;

public static class SubtitleSyntaxHighlighting
{
    public static IHighlightingDefinition CreateHighlightingDefinition()
    {
        var xshdString = @"<?xml version=""1.0""?>
<SyntaxDefinition name=""Subtitle"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
    <Color name=""HtmlTag"" foreground=""#008000"" fontWeight=""bold"" />
    <Color name=""AssaTag"" foreground=""#800080"" fontWeight=""bold"" />
    
    <RuleSet>
        <!-- ASSA tags: {\...} -->
        <Rule color=""AssaTag"">
            \{\\[^\}]*\}
        </Rule>
        
        <!-- HTML tags: <i>, <b>, <u>, <s>, </...> -->
        <Rule color=""HtmlTag"">
            &lt;/?[ibus]&gt;
        </Rule>
        
        <!-- Font tags -->
        <Rule color=""HtmlTag"">
            &lt;font[^&gt;]*&gt;
        </Rule>
        
        <Rule color=""HtmlTag"">
            &lt;/font&gt;
        </Rule>
    </RuleSet>
</SyntaxDefinition>";

        using var reader = new StringReader(xshdString);
        using var xmlReader = XmlReader.Create(reader);
        return HighlightingLoader.Load(xmlReader, HighlightingManager.Instance);
    }
}
