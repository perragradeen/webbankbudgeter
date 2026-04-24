# TODO

## Användarönskemål (ConsoleBudgeter) — markera **KLART** när du verifierat

| Krav | Status |
|------|--------|
| Rapportordning: först **In** (`gv_incomes`), sen **Ut** / Budget Total (`gv_budget`), sen **Kvar** (`gv_Kvar`). | Väntar din verifiering |
| Under **Kvar** ska raden **"-"** (transaktions-/saldoplaceholder i facit) inte visas. | Väntar din verifiering |
| **Budget Total:** `värnamoresor+övriga` ska ligga bland övriga utgifter, inte under inkomstraden **"+"** (fix: inkomst = exakt kategori `"+"`, inte `Contains("+")`). | Väntar din verifiering |
| Kör och lita på **ConsoleBudgeter** / `dotnet test` för verifiering (inte ad hoc Python). | Väntar din verifiering |

---

## ⏳ Dela affärslogik mellan WinForms-UI och ConsoleBudgeter

**Status:** PENDING

**Problem:** `ConsoleBudgeter/Builders/BudgetTableBuilder.cs` duplicerar logiken
i `WebBankBudgeterService.Services.BudgetStructureBuilder` (summeringsrader
`=== Summa utgifter ===` etc., klassificering av inkomst/utgift/transfer).
Om produktionskoden ändras fångar console-testerna det inte.

**Mål:** Låt `ConsoleBudgeter` referera `WebBankBudgeterService` och använda
`BudgetStructureBuilder` + `BudgetRow` + `TextToTableOutPuter` direkt så att
**samma kod** testas som den WinForms-UI:t använder.

**Plan:**
1. Lägg till `ProjectReference` till `WebBankBudgeterService` i `ConsoleBudgeter.csproj`.
2. Skriv en adapter `FacitToBudgetRows` som konverterar `BudgetUtFacit`,
   `BudgetInFacit`, `BudgetKvarFacit` → `List<BudgetRow>` + `ColumnHeaders`.
3. Anropa `new BudgetStructureBuilder().BuildStructuredBudget(rows, headers)`
   i stället för `BudgetTableBuilder.BuildExpensesTable/BuildKvarTable`.
4. Behåll `TableRenderer` men låt den ta en `StructuredBudgetTable` + headers.
5. Ta bort duplikatkoden i `ConsoleBudgeter/Builders/BudgetTableBuilder.cs`
   (behåll bara helpers som inte finns i service-lagret).
6. Uppdatera snapshots (de kan ändras något).
7. Lägg till test som verifierar att console-output bygger på
   `BudgetStructureBuilder` (t.ex. via spy/typ-kontroll).

**Filer som ändras:**
- `ConsoleBudgeter/ConsoleBudgeter.csproj`
- `ConsoleBudgeter/Builders/BudgetTableBuilder.cs` (ta bort det mesta)
- `ConsoleBudgeter/BudgetReportBuilder.cs` (anropa servicen)
- `ConsoleBudgeter/Rendering/TableRenderer.cs` (acceptera service-typer)
- `ConsoleBudgeterTest/Snapshots/report-{2014,2015}.txt` (ev. regenerera)

---

## ✅ Visa samma data på "Kvar"-fliken som "Budget Total"

**Status:** SLUTFÖRT (Redan implementerat i koden)

Verifierat 2026-04-24: Alla 4 steg var redan implementerade.

## Problem (Ursprunglig)

"Kvar"-fliken (`gv_Kvar`) är nästan tom medan "Budget Total" (`gv_budget`) visar full
budgetdata med alla kategorier, medelvärden och månadskolumner.

**Orsak:** "Kvar" använder en helt annan databindnings-kedja (`InBudgetUiHandler.BindInPosterRaderTillUi`)
som förlitar sig på InBudget-data från `BudgetIns.json`, medan "Budget Total" använder
`UtgiftsHanterareUiBinder.BindToBudgetTableUi` som bygger på transaktionsdata.

## Plan — 4 steg (✅ Alla implementerade)

### Steg 1: Gör `UtgiftsHanterareUiBinder.BindToBudgetTableUi` generisk

**Fil:** `WebBankBudgeterUi/UiBinders/UtgiftsHanterareUiBinder.cs`

Lägg till en `DataGridView`-parameter så metoden kan binda till vilken grid som helst:

```csharp
// Från:
public void BindToBudgetTableUi(TextToTableOutPuter table)
{
    var grid = _gv_budget;

// Till:
public void BindToBudgetTableUi(TextToTableOutPuter table, DataGridView targetGrid = null)
{
    var grid = targetGrid ?? _gv_budget;
```

Byt alla `_gv_budget`-referenser i metoden till `grid`.

### Steg 2: Anropa bind-metoden för `gv_Kvar`

**Fil:** `WebBankBudgeterUi/WebBankBudgeterUi.cs`

I `FillTablesAsync()`, efter `BindToBudgetTableUi(table)` (rad ~78), lägg till:

```csharp
BindToBudgetTableUi(table);       // befintligt (gv_budget)
BindKvarBudgetTableUi(table);     // NYTT (gv_Kvar)
```

Ny wrapper-metod:

```csharp
private void BindKvarBudgetTableUi(TextToTableOutPuter table)
{
    _utgiftsHanterareUiBinder.BindToBudgetTableUi(table, gv_Kvar);
}
```

### Steg 3: Ta bort gammal Kvar-kolumninitiering

**Fil:** `WebBankBudgeterUi/WebBankBudgeterUi.cs`

I `InitIncomesUi()` (rad ~190), ta bort raden:

```csharp
gv_Kvar.Columns.Add("1", WebBankBudgeter.CategoryNameColumnDescription);
```

Kolumnerna skapas nu istället av `UtgiftsHanterareUiBinder`.

### Steg 4: Ersätt gammal Kvar-bindning med den nya

**Fil:** `WebBankBudgeterUi/WebBankBudgeterUi.cs`

I `FillTablesAsync()`, ersätt anropet (rad ~91-92):

```csharp
// FRÅN (gammal — ger nästan tom tabell):
await VisaKvarRader_BindInPosterRaderTillUiAsync(utgiftsRader);

// TILL (ny — samma data som Budget Total):
BindKvarBudgetTableUi(table);
```

**OBS:** "Budget Total"-fliken (`gv_budget`) ändras INTE. Den förblir exakt som idag.
Bara "Kvar"-fliken (`gv_Kvar`) får ny data — genom att använda samma
bindningslogik som redan fungerar för Budget Total.

## Förväntat resultat

- **Budget Total** — helt oförändrad
- **Kvar** — visar nu samma kolumner som Budget Total:
  `Category . Month->`, `Average`, `Average-nf`, månadskolumner, `Summa`
- Samma rader med samma gruppering (utgifter, summa utgifter, inkomster, förflyttningar, budgettotal)
- Samma formatering (fetstil och grå bakgrund på summeringsrader)

## Filer som ändras

| Fil | Ändring |
|-----|---------|
| `WebBankBudgeterUi/UiBinders/UtgiftsHanterareUiBinder.cs` | Ny parameter `DataGridView targetGrid` |
| `WebBankBudgeterUi/WebBankBudgeterUi.cs` | Ny `BindKvarBudgetTableUi`, anrop i `FillTablesAsync`, bort med gammal kvar-logik och init |
