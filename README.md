# SwedBank Budgeter

Personligt budgetverktyg som läser banktransaktioner och visar dem i en kategoriserad budgetöversikt.

## Dokumentation för agenter och spårbarhet

- **[`AGENTS.md`](AGENTS.md)** — bindande regler för Cursor/agenter (facit via konsol, ingen gissning om branches, uppdatering av plan/todo).
- **[`HISTORY.md`](HISTORY.md)** — kort logg över väsentliga ändringar i *den här* repoklonen.
- **`plan.md` / `todo.md`** — ska hållas i synk med faktisk kod efter verifierad build (se `AGENTS.md`).

## Typ av projekt

- **Plattform:** Windows Forms (.NET 8.0)
- **Språk:** C#
- **Solution:** `Budgetterarn.sln` (Visual Studio 2022+)
- **Startprojekt:** `WebBankBudgeterUi`

## Vad applikationen gör

Applikationen laddar banktransaktioner från fil, kategoriserar dem och visar:
- Utgifter per kategori och månad
- Inkomster och budgeterade belopp
- Differens (kvar-budget) mellan budgeterat och faktiskt utfall
- Medelvärden och summor

## Projektstruktur

```
Budgetterarn.sln
│
├── WebBankBudgeterUi/          # WinForms-huvudapplikation (startprojekt)
│   ├── WebBankBudgeterUi.cs        # Huvudformulär, event-hantering, UI-bindning
│   ├── WebBankBudgeterUi.Designer.cs
│   ├── WebBankBudgeter.cs           # Affärslogik-brygga mellan UI och service
│   ├── UiBinders/
│   │   ├── UtgiftsHanterareUiBinder.cs  # Binder utgiftstabellen till DataGridView
│   │   └── InBudgetUiHandler.cs         # Binder inkomst/kvar-tabellen till DataGridView
│   └── TestData/BudgetIns.json      # Budgeterade inposter (inkomster per kategori)
│
├── WebBankBudgeterService/     # Tjänstelager — transaktionshantering, beräkningar
│   ├── TransactionHandler.cs        # Läser och hanterar transaktioner
│   ├── TransFilterer.cs             # Filtrerar transaktioner på år
│   ├── TransactionTransformer.cs
│   ├── Services/
│   │   ├── BudgetStructureBuilder.cs    # Bygger strukturerad budget (utgifter/inkomster/summeringar)
│   │   ├── TableGetter.cs
│   │   ├── TransactionCalcs.cs
│   │   └── Helpers/BudgetRowFactory.cs
│   ├── Model/
│   │   ├── BudgetRow.cs                 # Rad med kategori + belopp per månad
│   │   ├── Transaction.cs
│   │   ├── TransactionList.cs
│   │   └── ViewModel/TextToTableOutPuter.cs  # Tabellmodell med kolumnrubriker + budgetrader
│   └── MonthAvarages/               # Månadssnitt-beräkningar
│
├── InbudgetHandler/            # Hanterar budgeterade inposter (från JSON)
│   ├── InBudgetHandler.cs
│   ├── SkapaInPosterHanterare.cs
│   └── Model/Rad.cs                # Rad med RadNamnY + Kolumner (månad → belopp)
│
├── BudgeterCore/               # Entiteter och gemensamma modeller
│   └── Entities/InBudget.cs, BankRow.cs, KontoEntry.cs, etc.
│
├── CategoryHandler/            # Kategorisering av transaktioner
├── GeneralSettingsHandler/     # Inställningar (XML-baserat)
├── LoadTransactionsFromFile/   # Läs transaktioner från Excel
├── Utilities/                  # Hjälpfunktioner (fil, logg, Excel)
├── RefLesses/                  # Statiska hjälpfunktioner (string, datum, nummer)
├── XmlSerializer/              # XML-serialisering
│
├── BudgetterarnUi/             # Äldre WinForms-UI (parallell/legacy)
├── BudgetterarnDAL/            # Äldre DAL med WebCrawlers
├── ConsoleBudgeter/          # Konsol: textfacit / rapport (net8.0), `--out` för sparad utskrift
│
└── *Test-projekt:*
    ├── WebBankBudgeterServiceTest/
    ├── InbudgetHandlerTest/
    ├── UtilitiesTest/
    ├── GeneralSettingsTests/
    ├── FileTests/
    └── BudgetterarnUiTest/
```

## UI-flikar i huvudformuläret

| Flik | DataGridView | Beskrivning |
|------|-------------|-------------|
| **Kvar** | `gv_Kvar` | I nuvarande `FillTablesAsync`: samma tabellbindning som Budget Total (`BindKvarBudgetTableUi`). För IN+UT per rad finns `SnurraIgenom` + `VisaKvarRader_…` (se `todo.md`). |
| **Incomes** | `gv_incomes` | Budgeterade inkomster per kategori och månad |
| **Budget Total** | `gv_budget` | Alla utgifter/inkomster per kategori och månad med summor |
| **Totals** | `gv_Totals` | Sammanfattande siffror (snitt, diff) |
| **Transactions** | `dg_Transactions` | Alla individuella transaktioner |
| **Reccuring Costs** | — | Återkommande kostnader (ej implementerad) |
| **Non Reccuring Costs** | — | Engångskostnader (ej implementerad) |
| **No category** | — | Okategoriserade transaktioner (ej implementerad) |

## Dataflöde

```
Transaktionsfil (Excel/CSV)
    ↓
TransactionHandler.GetTransactionsAsync()
    ↓
FilterTransactions() → TransformToTextTableFromTransactions()
    ↓
TextToTableOutPuter { ColumnHeaders, BudgetRows }
    ↓
BudgetStructureBuilder.BuildStructuredBudget()
    ↓
UtgiftsHanterareUiBinder → gv_budget (Budget Total-fliken)
```

## Konfiguration

- `WebBankBudgeterUi/Data/GeneralSettings.xml` — sökväg till transaktionsfil, kategorifil
- `WebBankBudgeterUi/TestData/BudgetIns.json` — budgeterade belopp per kategori/månad
- `ConsoleBudgeter/Data/GeneralSettings.xml` — relativa sökvägar till `pelles budget.xls` och `BudgetterarnUi/Data/Categories.xml` (för textfacit-körning)
- `Pelles-budget-slim-2014-2015-gform.xlsx` (i repo-rot) — Excel-facit som `plan.md` refererar (kontoutdrag + budget 2014/2015)

## Textfacit (konsol, 2014–2015)

Kör samma pipeline som tjänstelagret och skriv full utskrift till fil (UTF-8). Skapa gärna mappen `Facit/` först.

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- --year 2014 --year 2015 --out Facit/facit-2014-2015-console.txt
```

Valfritt: `--transaction-file <sökväg>` om `pelles budget.xls` i repo-roten inte är samma export som innehåller alla år (konsolutskriften börjar med diagnostik: antal rader per år).

Om `dotnet` saknas i miljön (t.ex. vissa sandlådor), kör samma kommando lokalt med .NET 8 SDK installerat.

## Teckenkodning

Filerna i projektet har **blandad teckenkodning**:

| Fil | Kodning |
|-----|---------|
| `WebBankBudgeterUi/WebBankBudgeterUi.cs` | **Latin-1 (ISO-8859-1)**, utan BOM |
| `WebBankBudgeterUi/UiBinders/UtgiftsHanterareUiBinder.cs` | **UTF-8 med BOM** |
| `WebBankBudgeterUi/UiBinders/InBudgetUiHandler.cs` | **UTF-8 med BOM** |

**OBS:** `git diff` visar Latin-1-filer med `�` i stället för ö/ä/å — det är ett visningsproblem i git (som antar UTF-8), inte korrupta tecken. Filens bytes är korrekta.

Vid redigering av Latin-1-filer (t.ex. med script eller verktyg):
- Läs och skriv med `Encoding.GetEncoding("iso-8859-1")`, **inte** UTF-8.
- Använd inte verktyg som automatiskt konverterar till UTF-8 — då förstörs svenska tecken (ö → `�`).

## Bygga och köra

```bash
dotnet build Budgetterarn.sln
dotnet run --project WebBankBudgeterUi
```
