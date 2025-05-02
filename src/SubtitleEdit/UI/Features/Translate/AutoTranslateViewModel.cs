using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel : ObservableObject
{
    private ObservableCollection<TranslateRow> _rows;
    public FlatTreeDataGridSource<TranslateRow> TranslateRowSource { get;   }
    public AutoTranslateWindow Window { get; set; }
    public bool OkPressed { get; set; }

    public AutoTranslateViewModel()
    {
        _rows = new ObservableCollection<TranslateRow>();
        TranslateRowSource = new FlatTreeDataGridSource<TranslateRow>(_rows)
        {
            Columns =
            {
                new TextColumn<TranslateRow, int>
                    ("#", x => x.Number),
                new TextColumn<TranslateRow, string>
                    ("Show", x => x.Show),
                new TextColumn<TranslateRow, string>
                    ("Text", x => x.Text),
                new TextColumn<TranslateRow, string>
                    ("Translated text", x => x.TranslatedText),
            },
        };

        TranslateRowSource.RowSelection!.SingleSelect = false; // Multi select
     }

    public void Initialize(Subtitle subtitle)
    {
        var rows = subtitle.Paragraphs.Select(p => new TranslateRow
        {
             Number = p.Number,
             Show = p.StartTime.ToDisplayString(),
             Duration = p.Duration.ToDisplayString(),
             Text = p.Text,
        });
        _rows.AddRange(rows);
    }
}

public class TranslateRow
{
    public int Number { get; set; }
    public string Show { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    public string TranslatedText { get; set; }
}