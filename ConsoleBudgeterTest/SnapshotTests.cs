using ConsoleBudgeter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleBudgeterTest;

/// <summary>
/// Snapshot-tester: jämför den fulla renderade rapporten mot en committad
/// text-fil. Vid avsiktlig ändring: kör testet, kopiera den nya actual-filen
/// över snapshotten, och commita. Actuals skrivs alltid till test-outputen
/// för enkel diff.
/// </summary>
[TestClass]
public class SnapshotTests
{
    [DataTestMethod]
    [DataRow(2014)]
    [DataRow(2015)]
    public void FullReport_MatchesSnapshot(int year)
    {
        var report = BudgetReportBuilder.BuildReport(year, transactionLimit: 5);
        var snapshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "Snapshots", $"report-{year}.txt");
        var actualPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "Snapshots", $"report-{year}.actual.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(actualPath)!);
        File.WriteAllText(actualPath, report);

        if (!File.Exists(snapshotPath))
        {
            Assert.Fail($"Saknar snapshot: {snapshotPath}. Actual skriven till {actualPath}. " +
                        "Kopiera actual till snapshot och kör testet igen.");
        }

        var expected = File.ReadAllText(snapshotPath);
        var normExpected = Normalize(expected);
        var normActual = Normalize(report);

        Assert.AreEqual(normExpected, normActual,
            $"Rapporten skiljer sig från snapshot. Actual: {actualPath}");
    }

    private static string Normalize(string s) =>
        s.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
}
