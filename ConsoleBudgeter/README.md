# ConsoleBudgeter

Text-baserad motsvarighet till `WebBankBudgeterUi` (WinForms) som fungerar på
Linux/macOS/Windows. Byggd för att verifiera – via automatiska tester – att
den data som appen skulle visa i sina `DataGridView`-flikar är korrekt jämfört
med **facit** (JSON-filerna i `WebBankBudgeterTests.Facit/Facit/`).

## Varför

`WebBankBudgeterUi` kräver WinForms och är Windows-bundet. För att kunna
verifiera att beräkningar/layout är rätt utan Windows behöver vi en
plattformsoberoende rendering av samma tabeller – och ett sätt att jämföra
dem mot facit.

## Vad det gör

Rapporten innehåller en sektion per flik i WinForms-UI:t:

| Flik             | Källa i koden                        | Console-motsvarighet |
|------------------|--------------------------------------|-----------------------|
| Budget Total     | `UtgiftsHanterareUiBinder`           | `TableRenderer`       |
| Kvar             | Samma bindning, data: `expected-kvar`| `TableRenderer`       |
| Incomes          | `InBudgetUiHandler`                  | `IncomesRenderer`     |
| Totals           | `BindMonthAvaragesToUi`              | `TotalsRenderer`      |
| Transactions     | `BindTransactionListToUi`            | `TransactionsRenderer`|

Kolumner, summeringsrader (`=== Summa utgifter ===`, `=== BUDGET ... ===`),
och talformat följer `BudgetStructureBuilder` + `UtgiftsHanterareUiBinder`.
Tal formateras med `sv-SE` (tusentalsmellanslag, inga decimaler) för att
matcha `ToString("N0")` i UI:t – men oberoende av systemkultur.

## Kör

```bash
dotnet run --project ConsoleBudgeter -- --year 2014 --transactions 10
dotnet run --project ConsoleBudgeter -- --out report.txt
```

Flaggor:
- `--year YYYY` – ett eller flera år (default: 2014 och 2015).
- `--transactions N` – max antal transaktioner att visa (0 = alla).
- `--out FIL` – skriv till fil istället för stdout.

## Tester

`ConsoleBudgeterTest` innehåller:

1. **Aggregations-tester** – `budget-in + expected-ut == expected-kvar` per
   `(kategori, år, månad)` med ±0,01 tolerans; summeringsrader == summa av
   utgiftsrader; facit-antal är stabilt (2014 = 809, 2015 = 845).
2. **Snapshot-tester** – renderad rapport jämförs mot `Snapshots/report-YYYY.txt`.

Uppdatera en snapshot efter medveten ändring:

```bash
dotnet test ConsoleBudgeterTest
cp ConsoleBudgeterTest/bin/Debug/net8.0/Snapshots/report-2014.actual.txt \
   ConsoleBudgeterTest/Snapshots/report-2014.txt
```
