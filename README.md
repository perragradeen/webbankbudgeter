# SwedBank Budgeter

Personligt budgetverktyg som läser banktransaktioner och visar dem i en kategoriserad budgetöversikt.

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
| **Kvar** | `gv_Kvar` | Kvarvarande budget (budgeterat - faktisk utgift) |
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
- `Pelles-budget-slim-2014-2015-gform.xlsx` (i repo-rot) — Excel-facit som `plan.md` refererar (kontoutdrag + budget 2014/2015)

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

**Hela lösningen (kräver Windows Desktop SDK + WinForms):**

```bash
dotnet build Budgetterarn.sln
dotnet run --project WebBankBudgeterUi
```

**Linux / CI / när WinForms-appen kör och låser `bin` (MSB3021/MSB3027):**  
Bygg och testa utan Windows-UI-projekt med solution filter:

```bash
dotnet build Budgetterarn.NoWindowsUi.slnf
dotnet test Budgetterarn.NoWindowsUi.slnf
```

Filtret exkluderar `WebBankBudgeterUi`, `BudgetterarnUi` och `WebBankBudgeterUiTest`. Uppdatera listan i `Budgetterarn.NoWindowsUi.slnf` om nya `net8.0-windows`-projekt läggs till i lösningen.
