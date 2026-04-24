using System.Text.Json;

namespace WebBankBudgeterTests.Facit;

public static class FacitLoader
{
    private static string FacitDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Facit");

    public static List<TransactionFacit> LoadTransactions(int year) =>
        Load<List<TransactionFacit>>($"transactions-{year}.json");

    public static List<BudgetInFacit> LoadBudgetIn(int year) =>
        Load<List<BudgetInFacit>>($"budget-in-{year}.json");

    public static List<BudgetUtFacit> LoadExpectedUt(int year) =>
        Load<List<BudgetUtFacit>>($"expected-ut-{year}.json");

    public static List<BudgetUtFacit> LoadExpectedTransfers(int year) =>
        Load<List<BudgetUtFacit>>($"expected-transfers-{year}.json");

    public static List<BudgetKvarFacit> LoadExpectedKvar(int year) =>
        Load<List<BudgetKvarFacit>>($"expected-kvar-{year}.json");

    private static T Load<T>(string name)
    {
        var path = Path.Combine(FacitDir, name);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Facit file not found: {path}");
        }

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<T>(json, options) 
            ?? throw new InvalidOperationException($"Failed to deserialize {name}");
    }
}

public record TransactionFacit(int Year, int Month, int Day,
    string Description, double Amount, string Category, string Flag);

public record BudgetInFacit(string Category, int Year, int Month,
    string MonthName, double BudgetAmount);

public record BudgetUtFacit(string Category, int Year, int Month,
    string MonthName, double ActualAmount);

public record BudgetKvarFacit(string Category, int Year, int Month,
    string MonthName, double BudgetAmount, double ActualAmount, double Remaining);
