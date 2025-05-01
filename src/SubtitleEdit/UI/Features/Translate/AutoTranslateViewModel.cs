using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel : ObservableObject
{
    private ObservableCollection<TranslateRow> _rows;
    public FlatTreeDataGridSource<TranslateRow> TranslateRowSource { get; }
    public AutoTranslateWindow Window { get; set; }

    public AutoTranslateViewModel()
    {
        _rows = new ObservableCollection<TranslateRow>();
        _rows.Add(new TranslateRow()
        {
            Number = 1,
            Text = "<UNK>",
            TranslatedText = "hej",
            Show = "01",
        });
        _rows.Add(new TranslateRow()
        {
            Number = 2,
            Text = "<UNK>",
            TranslatedText = "hej",
            Show = "02",
        });

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
                    ("translated text", x => x.TranslatedText),
            },
        };

        TranslateRowSource.RowSelection!.SingleSelect = false; // single or multi select
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