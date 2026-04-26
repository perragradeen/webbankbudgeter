# SwedBank Budgeter

Personligt budgetverktyg som läser banktransaktioner och visar dem i en kategoriserad budgetöversikt.

## Agent- och dokumentationsrutiner

Läs **[AGENTS.md](AGENTS.md)** innan du ändrar kod: där står bland annat att textfacit skapas med `ConsoleBudgeter` och `--out`, att facit inte “justeras” bara för gröna tester, och att du ska utgå från **faktisk** `git`-status i arbetskopian.

- **[`HISTORY.md`](HISTORY.md)** — aktuellt viktigt vs. arkiv (gamla sessioner, agent-ID:n).
- **`plan.md` / `todo.md`** — hålls i synk med faktisk kod efter verifierad build (se `AGENTS.md`).

Efter **verifierad** build/test: uppdatera vid behov `plan.md`, `todo.md`, denna `README.md` och `HISTORY.md` i samma leverans.

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
├── ConsoleBudgeter/            # Konsolapp (net8.0) — samma budgetkedja som UI, textutskrift + `--out`
├── WebBankBudgeterTests.Facit/ # Delat facit-projekt (JSON + `Facit/facit-2014-2015.txt`)
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
| **Kvar** | `gv_Kvar` | Kvar per kategori/månad: **IN + UT** (facit `expected-kvar`; utgifter som negativa belopp; placeholder-raden **"-"** visas inte här) |
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
- `Pelles-budget-slim-2014-2015-gform.xlsx` (i repo-rot) — Excel-facit som `plan.md` refererar (kontoutdrag + budget 2014/2015), när den finns i arbetskopian

## Facit (oföränderlig testdata)

- JSON under `WebBankBudgeterTests.Facit/Facit/` och textreferens **`facit-2014-2015.txt`** (se `AGENTS.md` för exakt `--out`-sökväg) är **facit** — ändra dem bara när Excel/källan ändrats; anpassa i så fall kod och regenerera referenstext enligt `WebBankBudgeterTests.Facit/Facit/README.md`.
- **`dotnet test`** mot `ConsoleBudgeterTest` och `WebBankBudgeterServiceTest` (via `Budgetterarn.NoWindowsUi.slnf` på Linux) är det avsedda sättet att regressionstesta mot facit.

## Gemensam logik mellan WinForms och konsol

WinForms använder samma **service-** och **InbudgetHandler**-komponenter som `ConsoleBudgeter` för tabell­bygge: bland annat `FacitBudgetTextTableFactory`, `BudgetStructureBuilder`, `BudgetTableInMerger`, `KvarTextTableBuilder`, `InBudgetMath`, `TextToTableOutPuterClone`. Konsolen lägger endast till **textrendering** (`TableRenderer`, `BudgetReportBuilder`).

## Textfacit (konsol, 2014–2015)

Kör samma pipeline som tjänstelagret och skriv full utskrift till fil (UTF-8). Standardfilnamn i repot enligt `AGENTS.md`:

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- \
  --year 2014 --year 2015 --transactions 0 \
  --out WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt
```

Valfritt: `--transaction-file <sökväg>` om standardtransaktionsfilen inte innehåller rätt år.

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

**Hela lösningen (kräver Windows Desktop SDK + WinForms):**

```bash
dotnet build Budgetterarn.sln
dotnet run --project WebBankBudgeterUi
```

### ConsoleBudgeter och textfacit (2014–2015)

Full textfacit (alla transaktioner) ligger i **`WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt`**. För att regenerera efter medveten kodändring (jämför diff mot facit, committa bara om det är avsiktligt): se kommandot under *Textfacit* ovan.

Tester (enbart konsol): `dotnet test ConsoleBudgeterTest/ConsoleBudgeterTest.csproj`

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
| Full rapport till fil (facit) | samma som under *Textfacit* ovan |

**SDK:** installera **.NET 8 SDK** (`dotnet-sdk-8.0`). Utan `dotnet` i `PATH` misslyckas alla steg ovan.

**Vanliga fel:** om WinForms-appen kör samtidigt som `dotnet build Budgetterarn.sln` kan MSBuild rapportera **MSB3021/MSB3027** (filer låsta under `bin/`). Stäng appen eller använd `.slnf`.
