# TODO

## Användarönskemål (ConsoleBudgeter) — markera **KLART** när du verifierat

| Krav | Status |
|------|--------|
| Rapportordning: först **In** (`gv_incomes`), sen **Ut** / Budget Total (`gv_budget`), sen **Kvar** (`gv_Kvar`). | Väntar din verifiering |
| Under **Kvar** ska raden **"-"** (transaktions-/saldoplaceholder i facit) inte visas. | Väntar din verifiering |
| **Budget Total:** `värnamoresor+övriga` ska ligga bland övriga utgifter, inte under inkomstraden **"+"** (fix: inkomst = exakt kategori `"+"`, inte `Contains("+")`). | Väntar din verifiering |
| Kör och lita på **ConsoleBudgeter** / `dotnet test` för verifiering (inte ad hoc Python). | Väntar din verifiering |

---

## ⏳ Textfacit + Excel-pipeline (plan D15 / D16)

| Uppgift | Status |
|---------|--------|
| Committad **`WebBankBudgeterTests.Facit/Facit/console-report-facit-reference.txt`** (full rapport 2014+2015) som facit för utskrift; kopieras till test-output via `.csproj`. | Väntar din verifiering |
| **`Facit/README.md`** + **`plan.md`** (0.6, D15/D16, M1-notis): JSON från Excel-extraktorn; textfacit = `ConsoleBudgeter` `--out`, inte duplicerad layout i extraktorn. | Väntar din verifiering |
| **WinForms:** val av in-källa via `GeneralSettings.xml` (`InPosterSource` = `BudgetIns` \| `FacitJson`, `FacitBudgetInDirectory`). | Väntar din verifiering |

---

## ✅ Plan M5 — service (D7, D10, D12 delar)

| Punkt | Status |
|-------|--------|
| D7: `BudgetTableCategoryKey` + `TableGetter` / `BudgetRowFactory` (tom grupp → rent kategorinamn) | Klart i kod + `TableGetterCategoryKeyTests` |
| D10: `GetMonthAsFullString` invariant | Verifierat i test (redan `InvariantCulture` i kod) |
| D12: `Ignore` exkluderas från budgettabellaggregering (`SourceEntryType` från Excel-rad) | Klart i kod + test |

**Klart i kod:** M5.1 (IN slås in i Budget Total-tabell), M5.2 (Kvar via `SnurraIgenom`), M5.7 (`sv-SE` i `UtgiftsHanterareUiBinder`), `UtgiftsHanterareUiBinder` rensar kolumner/rader vid ombindning.

**Klart i kod (M5.3 / D9-del):** `InBudgetMath.SnurraIgenom` räknar **union** av IN- och UT-kategorier (samma som facit `expected-kvar`); `KvarTextTableBuilder` använder alla platta `BudgetRow` från tabellen före IN-merge och **filtrerar bort** Kvar-raden **"-"**. `BudgetIns.json` (UI + testdata) fylld med **672 rader** (336×2 år) från facit via `tools/FacitBudgetInsExport`.

**Nästa:** M5.4–M5.6, ev. grafiskt val av in-källa i UI (nu: XML).

---

## ✅ Dela affärslogik mellan WinForms-UI och ConsoleBudgeter

**Status:** KLART (2026-04-24)

- `ConsoleBudgeter` refererar **`WebBankBudgeterService`** och **`InbudgetHandler`**.
- Gemensamt: `FacitBudgetTextTableFactory`, `BudgetStructureBuilder`, `BudgetTableInMerger`, `KvarTextTableBuilder`, `InBudgetMath`, `TextToTableOutPuterClone`.
- Konsolen mappar facit med `FacitInBudgetRows`; `TableRenderer` skriver text från `TextToTableOutPuter`.
- `BudgetTableBuilder.cs` borttagen. Snapshot-tester: `BuildReport(..., transactionLimit: null)`.

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
