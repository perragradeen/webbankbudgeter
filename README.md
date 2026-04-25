# SwedBank Budgeter

Personligt budgetverktyg som läser banktransaktioner och visar dem i en kategoriserad budgetöversikt.

**Historik och lärdomar:** se [`HISTORY.md`](HISTORY.md) — där skiljer vi **aktuellt viktigt** från **arkiv** (gamla sessioner, agent-ID:n, metrics som snabbt blir inaktuella).

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
├── ConsoleBudgeter/           # Textbaserad rapport (Linux/CI) — samma tabell­logik som WinForms
├── WebBankBudgeterTests.Facit/  # Delad facit-JSON + FacitLoader (sanning för tester)
│
└── *Test-projekt:*
    ├── ConsoleBudgeterTest/
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
| **Kvar** | `gv_Kvar` | Kvar per kategori/månad: **IN + UT** (facit `expected-kvar`; placeholder-raden **"-"** visas inte här) |
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

- `WebBankBudgeterUi/Data/GeneralSettings.xml` — sökväg till transaktionsfil, kategorifil, samt **`InPosterSource`** (`BudgetIns` \| `FacitJson`) och **`FacitBudgetInDirectory`** när IN ska läsas från `budget-in-{år}.json`
- `WebBankBudgeterUi/TestData/BudgetIns.json` — budgeterade in-poster (kan regenereras från facit: `dotnet run --project tools/FacitBudgetInsExport`)

## Facit (oföränderlig testdata)

- JSON under `WebBankBudgeterTests.Facit/Facit/` och textreferens `console-report-facit-reference.txt` är **facit** — ändra dem bara när Excel/källan ändrats; anpassa i så fall kod och regenerera referenstext enligt `Facit/README.md`.
- **`dotnet test`** mot `ConsoleBudgeterTest` och `WebBankBudgeterServiceTest` (via `.slnf` på Linux) är det avsedda sättet att regressionstesta mot facit.

## Gemensam logik mellan WinForms och konsol

WinForms använder samma **service-** och **InbudgetHandler**-komponenter som `ConsoleBudgeter` för tabell­bygge: bland annat `FacitBudgetTextTableFactory`, `BudgetStructureBuilder`, `BudgetTableInMerger`, `KvarTextTableBuilder`, `InBudgetMath`, `TextToTableOutPuterClone`. Konsolen lägger endast till **textrendering** (`TableRenderer`, `BudgetReportBuilder`).

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

**På Linux utan Windows Desktop SDK kan du ändå:**

| Åtgärd | Kommando / notis |
|--------|-------------------|
| Bygga kärna + tester | `dotnet build Budgetterarn.NoWindowsUi.slnf` |
| Köra facit- och servicetester | `dotnet test Budgetterarn.NoWindowsUi.slnf` |
| Kör enbart konsol­snapshots | `dotnet test ConsoleBudgeterTest/ConsoleBudgeterTest.csproj` |
| Full rapport till fil (facit) | `dotnet run --project ConsoleBudgeter -- --year 2014 --year 2015 --transactions 0 --out sökväg/rapport.txt` |

**SDK:** installera **.NET 8 SDK** (`dotnet-sdk-8.0`). Utan `dotnet` finns inte i `PATH` misslyckas alla steg ovan.

**Vanliga fel:** om WinForms-appen kör samtidigt som `dotnet build Budgetterarn.sln` kan MSBuild rapportera **MSB3021/MSB3027** (filer låsta under `bin/`). Stäng appen eller använd `.slnf`.
