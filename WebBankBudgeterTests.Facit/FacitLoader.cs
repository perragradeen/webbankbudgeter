using System.Text.Json;

namespace WebBankBudgeterTests.Facit;

public static class FacitLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static string FacitDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Facit");

    public static string GetFacitDirectory() => FacitDir;

    public static List<TransactionFacit> LoadTransactions(int year) =>
        Load<List<TransactionFacit>>($"transactions-{year}.json");

    public static List<BudgetInFacit> LoadBudgetIn(int year) =>
        Load<List<BudgetInFacit>>($"budget-in-{year}.json");

    public static List<BudgetUtFacit> LoadExpectedUt(int year) =>
        Load<List<BudgetUtFacit>>($"expected-ut-{year}.json");

    public static List<BudgetTransferFacit> LoadExpectedTransfers(int year) =>
        Load<List<BudgetTransferFacit>>($"expected-transfers-{year}.json");

    public static List<BudgetKvarFacit> LoadExpectedKvar(int year) =>
        Load<List<BudgetKvarFacit>>($"expected-kvar-{year}.json");

    private static T Load<T>(string name)
    {
        var path = Path.Combine(FacitDir, name);
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Tom eller ogiltig JSON: {path}");
    }
}

public record TransactionFacit(
    int Year,
    int Month,
    int Day,
    string Description,
    double Amount,
    string Category,
    string Flag);

public record BudgetInFacit(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double BudgetAmount);

public record BudgetUtFacit(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double ActualAmount);

public record BudgetTransferFacit(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double ActualAmount);

public record BudgetKvarFacit(
    string Category,
    int Year,
    int Month,
    string MonthName,
    double BudgetAmount,
    double ActualAmount,
    double Remaining);
