using System.Globalization;
using System.Text;
using WebBankBudgeterService.Model;
using WebBankBudgeterService.Model.ViewModel;
using WebBankBudgeterService.Services;

namespace ConsoleBudgeter.Rendering;

/// <summary>
/// Renderar <see cref="TextToTableOutPuter"/> som text via <see cref="BudgetStructureBuilder"/>
/// (samma struktur som <see cref="UiBinders.UtgiftsHanterareUiBinder"/>).
/// </summary>
public static class TableRenderer
{
    public const string SummaColumnDescription = "Summa";

    private static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("sv-SE");

    public static string Render(TextToTableOutPuter source, string? title = null)
    {
        var builder = new BudgetStructureBuilder();
        var structured = builder.BuildStructuredBudget(
            source.BudgetRows?.ToList() ?? new List<BudgetRow>(),
            source.ColumnHeaders);

        var headers = new List<string>(source.ColumnHeaders);
        var hasSumma = headers.Count > 1 && !headers.Contains(SummaColumnDescription);
        if (hasSumma) headers.Add(SummaColumnDescription);

        var columnIsAverageNf = headers
            .Select(h => h == TextToTableOutPuter.AverageColumnDescriptionNotFormatted)
            .ToArray();

        var monthColumns = BudgetStructureBuilder.MonthColumnKeys(headers);

        var rowCells = new List<string[]>();
        foreach (var row in structured.Rows)
        {
            var cells = new string[headers.Count];
            double rowTotal = 0;
            for (var i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                switch (header)
                {
                    case TextToTableOutPuter.CategoryNameColumnDescription:
                        cells[i] = row.CategoryText ?? string.Empty;
                        break;
                    case TextToTableOutPuter.AverageColumnDescription:
                    {
                        var (sum, count) = AverageOverMonths(row, monthColumns);
                        var avg = count > 0 ? sum / count : 0;
                        cells[i] = IsSeparator(row) ? string.Empty : FormatN0(avg);
                        break;
                    }
                    case TextToTableOutPuter.AverageColumnDescriptionNotFormatted:
                    {
                        var (sum, count) = AverageOverMonths(row, monthColumns);
                        var avg = count > 0 ? sum / count : 0;
                        cells[i] = IsSeparator(row) ? string.Empty : avg.ToString("R", CultureInfo.InvariantCulture);
                        break;
                    }
                    case SummaColumnDescription:
                        cells[i] = IsSeparator(row) ? string.Empty : FormatN0(rowTotal);
                        break;
                    default:
                        if (monthColumns.Contains(header))
                        {
                            if (row.AmountsForMonth.TryGetValue(header, out var v))
                            {
                                rowTotal += v;
                                cells[i] = IsSeparator(row) ? string.Empty : FormatN0(v);
                            }
                            else
                            {
                                cells[i] = IsSeparator(row) ? string.Empty : FormatN0(0);
                            }
                        }
                        else
                        {
                            cells[i] = string.Empty;
                        }

                        break;
                }
            }

            rowCells.Add(cells);
        }

        return RenderGrid(headers, rowCells, title, columnIsAverageNf, structured.Rows);
    }

    private static (double sum, int count) AverageOverMonths(BudgetRow row, List<string> monthColumns)
    {
        double sum = 0;
        var count = 0;
        foreach (var col in monthColumns)
        {
            if (row.AmountsForMonth.TryGetValue(col, out var v))
            {
                sum += v;
                count++;
            }
        }

        return (sum, count);
    }

    private static bool IsSeparator(BudgetRow row) => string.IsNullOrEmpty(row.CategoryText);

    private static string FormatN0(double value) => value.ToString("N0", NumberCulture);

    private static string RenderGrid(List<string> headers, List<string[]> rows, string? title,
        bool[] columnIsAverageNf, IReadOnlyList<BudgetRow> rowMeta)
    {
        var widths = new int[headers.Count];
        for (var i = 0; i < headers.Count; i++) widths[i] = headers[i].Length;
        foreach (var row in rows)
        {
            for (var i = 0; i < headers.Count; i++)
            {
                if (row[i].Length > widths[i]) widths[i] = row[i].Length;
            }
        }

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(title))
        {
            sb.AppendLine(title);
            sb.AppendLine(new string('=', title.Length));
        }

        sb.AppendLine(BorderLine(widths));
        sb.AppendLine(RowLine(headers.ToArray(), widths, isHeader: true, columnIsAverageNf));
        sb.AppendLine(BorderLine(widths));

        for (var r = 0; r < rows.Count; r++)
        {
            var cells = rows[r];
            var meta = rowMeta[r];
            var isSummary = meta.CategoryText.Contains("===", StringComparison.Ordinal);
            var isSeparator = string.IsNullOrEmpty(meta.CategoryText);

            if (isSeparator)
            {
                sb.AppendLine(BorderLine(widths, '-'));
                continue;
            }

            if (isSummary) sb.AppendLine(BorderLine(widths, '-'));
            sb.AppendLine(RowLine(cells, widths, isHeader: false, columnIsAverageNf));
            if (isSummary) sb.AppendLine(BorderLine(widths, '-'));
        }

        sb.AppendLine(BorderLine(widths));
        return sb.ToString();
    }

    private static string BorderLine(int[] widths, char fill = '=')
    {
        var sb = new StringBuilder();
        sb.Append('+');
        foreach (var w in widths)
        {
            sb.Append(new string(fill, w + 2));
            sb.Append('+');
        }

        return sb.ToString();
    }

    private static string RowLine(string[] cells, int[] widths, bool isHeader, bool[] columnIsAverageNf)
    {
        var sb = new StringBuilder();
        sb.Append('|');
        for (var i = 0; i < cells.Length; i++)
        {
            var value = cells[i] ?? string.Empty;
            if (columnIsAverageNf[i] && !isHeader)
            {
                value = TrimAverageNf(value);
            }

            var leftAlign = i == 0;
            sb.Append(' ');
            if (leftAlign)
            {
                sb.Append(value.PadRight(widths[i]));
            }
            else
            {
                sb.Append(value.PadLeft(widths[i]));
            }

            sb.Append(' ');
            sb.Append('|');
        }

        return sb.ToString();
    }

    private static string TrimAverageNf(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return raw;
        return v.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
