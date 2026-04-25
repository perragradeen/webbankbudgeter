# TODO — WebBankBudgeter (levande lista)

Uppdatera denna fil **efter** att något byggts, testats och verifierats (lokalt eller CI), i linje med `plan.md` och `README.md`.

## Verifierat i kod (denna branch)

| Punkt | Status | Notis |
|--------|--------|--------|
| Inkomst i strukturerad budget = **endast** kategori trimmat lika med `"+"` (inte `Contains`) | Klart | `BudgetStructureBuilder` — undviker fel på t.ex. `värnamoresor+övriga` |
| `TransFilterer.FilterTransactions(list, year)` — strikt **`DateAsDate.Year == year`** | Klart | `TransFiltererTests` |
| `Transaction.GetMonthAsFullString` invariant (D10) | Klart | befintlig implementation |

## Öppet / väntar arbete eller filer

| Punkt | Status | Notis |
|--------|--------|--------|
| Committad **facit-JSON** + **FacitExtractor** (M1) | Öppen | Saknas i repot — se `plan.md` §3, M1 |
| **FacitLoader** + service-tester mot JSON (M2–M3) | Öppen | Efter M1 |
| **gv_Kvar** = **IN + UT** (`SnurraIgenom` / `VisaKvarRader_BindInPosterRaderTillUiAsync`) | Öppen | Idag: `BindKvarBudgetTableUi` = samma tabell som Budget Total |
| **D7** — gruppering på `CategoryNameNoGroup` när grupp saknas | Öppen | `TableGetter` använder fortfarande `CategoryName` |
| **M0** — räkna/verifiera transaktioner mot riktig `.xls` (~1 654 för 2014+2015) | Öppen | Manuell verifiering |
| **M4** — WinForms-integrationstester | Öppen | Kräver Windows |
| **BudgetIns.json** fylld/sync från facit när JSON finns | Öppen | D9 |
| Valfritt: grafiskt val av **in-källa** (facit vs fil) | Öppen | Se plan tidigare D16-idé |

## Arkiverad idé (historik — använd inte som nuvarande mål)

Den gamla fyra-stegs-planen som sa att **Kvar** skulle visa *samma* data som Budget Total (`BindKvarBudgetTableUi`) är **ersatt**: facit-målet är **Kvar = IN + UT**. Den gamla texten fanns i tidigare version av `todo.md` och motsvarar inte längre önskat beteende.
