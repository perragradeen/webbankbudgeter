# SwedBank Budgeter

Personligt budgetverktyg som läser banktransaktioner och visar dem i en kategoriserad budgetöversikt.

## Agent- och dokumentationsrutiner

Läs **[AGENTS.md](AGENTS.md)** innan du ändrar kod: där står bland annat att textfacit skapas med `ConsoleBudgeter` och `--out`, att facit inte “justeras” bara för gröna tester, och att du ska utgå från **faktisk** `git`-status i arbetskopian.

- **`plan.md`** — endast **återstående** arbete (M0 status pekar på arkiv för längre checklista; M3/M4; regenerering av facit; öppna gap).
- **`todo.md`** — korta öppna punkter.
- **[`todo-history-arkiv.md`](todo-history-arkiv.md)** — fryst snapshot av tidigare full `plan.md` + `todo.md` (beslut, Excel-analys, facit-schema, gamla milstolpar) **samt nya avsnitt för avklarade milstolpar** (sök nedan).
- **[`HISTORY.md`](HISTORY.md)** — kort aktuellt (beslut, Linux, facit-kedja).
- **[`HISTORY_ARCHIVE.md`](HISTORY_ARCHIVE.md)** — längre bakgrund (gamla sessioner, agent-ID:n).

**Rutin:** När något har **byggts, testats och verifierats**, uppdatera **`plan.md`**, **`todo.md`**, denna **`README.md`** och **`HISTORY.md`**. Lägg genomförda checklistor/konkret leverans i **`todo-history-arkiv.md`** med tydlig rubrik (se *plan-arkiv* nedan).

### Nyckelord: plan-arkiv, rensa plan, arkivera klart

Använd detta när du vill **flytta färdigt arbete ur `plan.md`** så filen bara listar det som återstår — utan att tappa spårbarhet.

| Sök / säg till agent | Meny |
|---------------------|------|
| **plan-arkiv** | Arbetsflödet nedan + filen **`todo-history-arkiv.md`**. |
| **rensa plan** | Korta `plan.md`; detaljer ska inte dupliceras om de bara var “klar-koll”. |
| **arkivera klart** | Klipp in genomförd text eller ny leveranspost **överst i arkivet** (ny `##`-rubrik med datum). |
| **M0 verifiering** | Checklista för manuell M0 ligger i arkiv under *M0 verifiering — flyttad från plan*. |
| **In Ut Kvar** | Kort leveransbeskrivning för flik- och rapportmodellen i arkiv. |

**Steg (upprepa vid behov):**

1. I **`plan.md`**: markera vad som är **klart** vs **kvar**. Ta bort långa avsnitt som inte längre styr arbete (t.ex. punktlistor som bara bekräftar redan automatiserat/testat).
2. I **`todo-history-arkiv.md`**: lägg en **ny sektion** med dagens datum och klistra in flyttad text, eller en kort **leveranspost** (vad som gjorts, vilka filer/tester som är sanning).
3. **Uppdatera pekare:** om `todo.md` eller `plan.md` refererade till borttagen §, ändra till **arkivrubrik** eller *«se arkiv § …»*.
4. **`HISTORY.md`:** en rad eller stycke om leveransen var större (beslut, facit-regenerering, ny flikmodell).
5. **`README.md`:** bara om processen eller pekare ändrats (denna undersektion behöver sällan ändras; uppdatera tabellen om du inför **nya** nyckelord).

Arkivfilen **`todo-history-arkiv.md`** ska **inte** overskrivas helt — den innehåller redan den stora frysta **«Tidigare plan.md (fullständig)»**; nya arkivdelar läggs **ovanför** den gamla snapshotten eller som egna `##`-block efter introt.

## Typ av projekt

- **Plattform:** Windows Forms (.NET 8.0)
- **Språk:** C#
- **Solution:** `Budgetterarn.sln` (Visual Studio 2022+)
- **Startprojekt:** `WebBankBudgeterUi`

## Utveckling med Cursor (multi-agent)

Arbetet i repot sker ofta via **Cursor** med flera sätt att få hjälp av en modell samtidigt:

- **Flera agenter / parallella uppdrag:** Olika konversationer eller bakgrundsjobb (t.ex. *Cloud Agent*) kan arbeta i samma repo **samtidigt**. De skapar då ofta **egna grenar** (prefix `cursor/…`) och **pull requests** mot `master`.
- **Samordning:** Innan merge, kontrollera att grenar inte krockar i samma filer och att tester fortfarande är gröna. Historik om tidigare agent-sessioner kan finnas i `HISTORY.md` om den filen finns i grenen — den är **bakgrund**, inte facit; **committad facit + tester** är den bindande referensen.
- **Headless / Linux:** WinForms-projektet kräver Windows Desktop SDK. För CI eller Linux: `Budgetterarn.NoWindowsUi.slnf` — bygg/test utan WinForms; konsol-/servicetester.

Kort sagt: *multi-agent* här betyder **flera automatiserade eller parallella kodvägar** — planera merges och lita på **test + facit**, inte bara på en enskild agents utskrift i chatten.

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

**Linux / headless:** Bygg och testa bibliotek som `WebBankBudgeterService` och `WebBankBudgeterServiceTest` med `dotnet` på Linux. **`WebBankBudgeterUi`** kräver **Windows Desktop**-SDK (`net8.0-windows`) — kör UI-bygge på en Windows-maskin om full lösning ska valideras.

## UI-flikar i huvudformuläret

| Flik | DataGridView | Beskrivning |
|------|-------------|-------------|
| **In** | `gv_incomes` | Budget-in per kategori och månad (facit `budget-in-*.json` / `BudgetIns.json`) |
| **Ut - Utgifter** | `gv_budget` | Transaktionssummering per kategori och månad med summeringsrader (**samma siffror som facit `expected-ut`**; ingen ihopslagning med budget-in) |
| **Kvar** | `gv_Kvar` | Kvar per kategori/månad: **IN + UT** (facit `expected-kvar`; utgifter som negativa belopp; placeholder-raden **"-"** visas inte här) |
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
TextToTableOutPuter { ColumnHeaders, BudgetRows } (en klon används till Kvar-bygge)
    ↓
BudgetStructureBuilder.BuildStructuredBudget()
    ↓
UtgiftsHanterareUiBinder → gv_budget (**Ut - Utgifter**, transaktioner endast)
    ↓
KvarTextTableBuilder + InBudgetMath.SnurraIgenom → gv_Kvar (**Kvar**, IN+UT)
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

**Service-tester (Linux utan WinForms, enbart det projektet):**

```bash
dotnet test WebBankBudgeterServiceTest/WebBankBudgeterServiceTest.csproj
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
| Full rapport till fil (facit) | samma som under *Textfacit* ovan |

**SDK:** installera **.NET 8 SDK** (`dotnet-sdk-8.0`). Utan `dotnet` i `PATH` misslyckas alla steg ovan.

**Vanliga fel:** om WinForms-appen kör samtidigt som `dotnet build Budgetterarn.sln` kan MSBuild rapportera **MSB3021/MSB3027** (filer låsta under `bin/`). Stäng appen eller använd `.slnf`.

### ConsoleBudgeter utan .NET på målmaskinen (Linux x64)

Med **självmantlad** publicering följer .NET-runtime med i utdata. På en annan Linux behöver målmaskinen då **inte** installera SDK eller runtime — kopiera publiceringsmappen och kör binären.

Bygg (kräver .NET SDK **på byggmaskinen**, t.ex. CI eller din dev-dator):

```bash
./scripts/publish-console-budgeter-linux.sh
```

Alternativt utan skript:

```bash
dotnet publish ConsoleBudgeter/ConsoleBudgeter.csproj -c Release -p:PublishProfile=Linux-x64-SelfContained
```

Utdata hamnar under **`artifacts/ConsoleBudgeter/linux-x64/`** (mappen är ignorerad av git). Kör t.ex.:

```bash
./artifacts/ConsoleBudgeter/linux-x64/ConsoleBudgeter --help
```

För textfacit (samma som i `AGENTS.md`):

```bash
./artifacts/ConsoleBudgeter/linux-x64/ConsoleBudgeter -- \
  --year 2014 --year 2015 --transactions 0 \
  --out WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt
```

För Windows eller andra RID:er kan du lägga en egen `PublishProfiles/*.pubxml` med annat `RuntimeIdentifier` (t.ex. `win-x64`).
