# TODO: Visa samma data på "Kvar"-fliken som "Budget Total"

## Problem

"Kvar"-fliken (`gv_Kvar`) är nästan tom medan "Budget Total" (`gv_budget`) visar full
budgetdata med alla kategorier, medelvärden och månadskolumner.

**Orsak:** "Kvar" använder en helt annan databindnings-kedja (`InBudgetUiHandler.BindInPosterRaderTillUi`)
som förlitar sig på InBudget-data från `BudgetIns.json`, medan "Budget Total" använder
`UtgiftsHanterareUiBinder.BindToBudgetTableUi` som bygger på transaktionsdata.

## Plan — 4 steg

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
