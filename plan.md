# Plan: Efterlikna `Pelles-budget-slim-2014-2015-gform.xlsx` i WebBankBudgeter

Facit: `C:\Files\Dropbox\budget\Program\webbankbudgeter\Pelles-budget-slim-2014-2015-gform.xlsx`

Målet är att UI:t ska visa exakt samma struktur och data som Excel-förlagan:

- **Inkomster** (budget) i egen sektion, per kategori och månad
- **Utgifter** (utfall) kategoriserade och inlagda i respektive år och månad
- **Kvar per månad** = `IN + UT` per kategori per år/månad
  (UT är negativt → positivt resultat = under budget)

---

## 0. Beslut (tagna)

| # | Område | Valt alternativ | Kommentar |
|---|--------|-----------------|-----------|
| D1 | Beloppsprecision | **(b) `decimal` internt** | Pengar i SEK, 2 decimaler från Kontoutdraget. Presentation: se 0.2. |
| D2 | Transfers (`" -"`) | **(c) Egen fil** `expected-transfers-YYYY.json` | Håller `expected-ut` synkad med `BudgetStructureBuilder` som redan filtrerar bort transfers. |
| D3 | `expected-kvar` när IN saknas | **(a) `BudgetAmount = 0`** | Se 0.1 för konkreta facit-exempel. |
| D4 | `expected-kvar` när UT saknas | **(a) `ActualAmount = 0`** | Se 0.1 för konkreta facit-exempel. |
| D5 | Januari 2014 i IN | **(b) Generera januari-rader med testdata** | Efterliknar de andra månaderna. Avviker medvetet från Excel. |
| D6 | `TransactionHandler` | **(a) Behåll nuvarande klass, verifiera inläsning mot facit** | Filen finns i `WebBankBudgeterService/TransactionHandler.cs`. Bekräfta att `TransactionList.Transactions` + `TransactionList.Account.AvailableAmount` matchar vad `WebBankBudgeter.cs` förväntar, och att inläsningen levererar 1 654 rader för facit-året. |
| D7 | Group-normalisering | **(b) `CategoryNameNoGroup` i `TableGetter.GroupOnMonthAndCategory`** | Minst ingrepp; funktionen finns redan. |
| D8 | Sparrader | **(a) Utgifter (för nu)** | Kan flyttas till egen sektion senare. |
| D9 | `BudgetIns.json`-format | **(b) Migrera om det ger förbättring** | Utvärderas i M5 — se 0.4. |
| D10 | Månadskultur | **(b) Ändra direkt till `InvariantCulture`** i M5 | Datumparsning från Excel: se 0.3. |
| D11 | Facit-placering | **(b) Eget shared-projekt** | Alla testprojekt (unit, integration, referens) länkar det. |
| D12 | `Ignore`-rader i facit | **(a) Inkludera i `transactions-*.json`**, exkludera i `expected-ut-*.json` | Gör filterregeln testbar. |
| D13 | Assert-strategi | **(b) FluentAssertions** — `BeApproximately(value, 0.01m)` | Lägg till paketref i testprojektet om det saknas. |
| D14 | Sortering | **(b) Behåll kodens sortering; jämför som dictionary** | Se 0.5 — bara siffrorna behöver matcha. |
| D15 | Textfacit för hela rapporten | **(a) Committad fil + samma pipeline som ConsoleBudgeter** | `facit-2014-2015.txt` speglar `BudgetReportBuilder`, som använder **`WebBankBudgeterService`** (`FacitBudgetTextTableFactory`, `BudgetStructureBuilder`, …) och **`InbudgetHandler`** (`BudgetTableInMerger`, `KvarTextTableBuilder`, `InBudgetMath`) — samma kedja som WinForms för Ut/Kvar. Excel-extraktorn levererar endast JSON. |
| D16 | Var IN i rapporten kommer ifrån | **(b) Användarval i WinForms** | Facit-JSON (`budget-in-*.json`) kan fortsätta användas i tester/CI. I **produktions-UI** ska användaren kunna **välja källa för in-poster** (t.ex. nuvarande `BudgetIns.json` / lokal fil / annat) så att `gv_incomes` och därmed In-sektionen i rapporten speglar valet — se avsnitt **0.6**. |

### 0.1 D3/D4 förtydligade med exempel ur facit

**Kategorier som finns i `expected-ut-2014.json` men INTE i `budget-in-2014.json`**
(verifierat genom att diffa unika kategorier):

| Kategori i UT | Finns i IN? |
|---------------|-------------|
| `" -"` (transfers — flyttas till egen fil enligt D2) | Nej |
| `"+"` (inkomster — utfall av löner etc) | Nej (inkomsternas budget hanteras separat) |
| `"-"` | Nej — 1 enstaka rad, troligen skrivfel i Excel |

→ Med **D3 = a** blir exempelraden för `"+"` januari 2014 (om utfall är +22 500 kr):

```json
{ "Category": "+", "Year": 2014, "Month": 1, "MonthName": "January",
  "BudgetAmount": 0.00, "ActualAmount": 22500.00, "Remaining": 22500.00 }
```

(`Remaining = 0 + 22500 = 22500` — "22 500 kr kom in utan registrerad budget")

**Kategorier som finns i `budget-in-2014.json` men INTE i `expected-ut-2014.json`**
(kategorier med budget men inga matchande transaktioner 2014):

| Kategori i IN | Finns i UT? |
|---------------|-------------|
| `"hushåll, reparationer etc inköp av nya husshållsmaskiner"` | Nej |
| `"hemlagad lunch"` | Nej |
| `"hygien (disk, tvätt o tvål etc)"` | Nej |
| `"övrigt i samband med supa"` | Nej |
| `"Buss etc i samband med  supa"` | Nej |
| `"spara till dator"` | Nej |

→ Med **D4 = a** blir exempelraden för `"hemlagad lunch"` mars 2014 (budget 300 kr, inga transaktioner):

```json
{ "Category": "hemlagad lunch", "Year": 2014, "Month": 3, "MonthName": "March",
  "BudgetAmount": 300.00, "ActualAmount": 0.00, "Remaining": 300.00 }
```

(`Remaining = 300 + 0 = 300` — "hela budgeten kvar, inget spenderat")

**Sammanfattning:** `expected-kvar` täcker **unionen** av alla kategorier. Där en
sida saknas fylls 0 i. Enhetligt format utan `null`, speglar "positivt = under
budget, negativt = över budget".

### 0.2 Presentation i UI (tillhör D1)

Beloppen lagras som `decimal` i alla lager. Presentation i grid:

- **Format**: `# ##0` (tusentalsavgränsare som hård mellanslag, **inga decimaler**).
- **Negativa belopp**: minustecken före — `-1 234` (inte parenteser).
- **Kultur**: `sv-SE` för formatsträngen:
  `value.ToString("# ##0", CultureInfo.GetCultureInfo("sv-SE"))`.

Motivering: pengar lagras exakt (2 decimaler) men visas grovt för
läsbarhet i månadstabellen. Summor och toleranser gäller den exakta
`decimal`-representationen, inte den formatterade strängen.

### 0.3 Datumparsning (tillhör D10)

Excel-kontoutdraget lagrar datum som **tre separata celler** (år / månad / dag)
— t.ex. `2014 \t 1 \t 27`. Det är **inte** en kulturberoende sträng, så extraktorn
bygger `new DateTime(year, month, day)` direkt utan `Parse`.

Svensk kultur behövs **bara** för visning i UI:t (`2014-01-27`). All intern
jämförelse sker mot `int Year, Month, Day` eller `DateTime`.

`Transaction.GetMonthAsFullString` ändras i M5 till `InvariantCulture` så
`"YYYY January"` blir stabil oavsett tråd-kultur (facit använder engelska
månadsnamn; behövs för att nyckeln i `BudgetRow.AmountsForMonth` ska matcha).

### 0.4 Om D9 — migrering av `BudgetIns.json`

Nuvarande format (`WebBankBudgeterUi/TestData/BudgetIns.json`):
```json
{ "CategoryDescription": "el", "BudgetValue": 1100, "YearAndMonth": "2016-05-01T00:00:00" }
```

Facit-format (`budget-in-YYYY.json`):
```json
{ "Category": "el", "Year": 2014, "Month": 5, "MonthName": "May", "BudgetAmount": 1100.00 }
```

**Utvärderas i M5** mot:
- Kan `InbudgetHandler` läsa båda formaten via adapter (migrationsväg utan migration)?
- Är `YearAndMonth` som full `DateTime` nödvändigt för någon befintlig funktion?
- Förloras något om `MonthName` slopas?

Om svaret är "lika bra åt båda hållen" → migrera till facit-formatet för enhetlighet.
Annars → behåll nuvarande format och transformera från facit vid generering.

### 0.5 Om D14 — vad "sortering" betyder här

Facit-JSON är en **array** sorterad på `Category → Year → Month`. Koden producerar
`Dictionary<MonthColumn, decimal>` per `BudgetRow`, i en lista sorterad
`OrderByDescending(CategoryText)` (`TableGetter.cs:39`).

**Testet bryr sig bara om siffrorna.** Därför jämförs facit och kodutdata som
nycklade uppslagstabeller (`(Category, Year, Month) → Amount`). Listordningen
spelar ingen roll.

```csharp
// Pseudokod för assert med FluentAssertions
var facitDict = facit.ToDictionary(x => (x.Category, x.Year, x.Month), x => x.Amount);
var kodDict   = code .ToDictionary(x => (x.Category, x.Year, x.Month), x => x.Amount);

foreach (var key in facitDict.Keys)
    kodDict[key].Should().BeApproximately(facitDict[key], 0.01m,
        because: $"kategori {key.Item1} {key.Item2}-{key.Item3}");
```

**Ingen ändring i `TableGetter`-sorteringen behövs.**

### 0.6 Textfacit för rapport + val av in-poster i WinForms (D15 / D16)

**Textfacit (hela konsolrapporten):**
- Fil: `WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt`.
- Innehåller `ConsoleBudgeter`-utskrift för **2014 och 2015** (In, Ut, Kvar, Totals, alla transaktioner), UTF-8.
- **Uppdateringsregel:** varje gång committade JSON-facit (`transactions-*`, `budget-in-*`, `expected-*`) ändras ska filen regenereras med samma kommando som i `Facit/README.md`, så texten förblir sanningsunderlag för snapshot/integration.
- **Excel / FacitExtractor:** ska fortsätta leverera **JSON** enligt M1. Tabelltext ska **inte** härledas separat i extraktorn — kör `ConsoleBudgeter` (`BudgetReportBuilder` → service + InbudgetHandler + textrendering i konsolprojektet).

**In-poster — användarval i WinForms (produkt):**
- Tester och CI kan fortsätta ladda **`budget-in-*.json`** som idag.
- I **`WebBankBudgeterUi`** ska användaren kunna **välja källa för in-poster** som matar `gv_incomes` (och därmed den budget-IN-data som ska ingå i Kvar/Budget Total när det kopplas till `InBudgetMath.SnurraIgenom` / inläsning):
  - Minst: nuvarande lokala `BudgetIns.json` (eller motsvarande sparad väg).
  - Utöka med: importera **facit-format** (`budget-in-YYYY.json`) eller annan vald fil/mapp per år.
- **M5 / UI-uppgift:** lägg inställning (t.ex. i `GeneralSettings`, eller dialog vid start / under Inställningar) som `InBudgetHandler` / `InBudgetUiHandler` respekterar vid laddning och sparande.

---

## 1. Djup analys av Excel-filen

Filen innehåller 5 flikar:

| # | Flik | Rader | Kol | Roll |
|---|------|-------|-----|------|
| 1 | `Kontoutdrag_officiella` | 1 655 | 12 | Rådata: alla bank­trans­aktioner med manuellt satt kategori |
| 2 | `Budget (2015)` | 194 | 35 | Budget/utfall/kvar-vy för år 2015 |
| 3 | `Villkor 1 år (2015)` | 824 | 15 | Mall med kategorier per månad (referens, ej data) |
| 4 | `Budget (2014)` | 194 | 35 | Budget/utfall/kvar-vy för år 2014 |
| 5 | `Villkor 1 år (2014)` | 824 | 15 | Mall 2014 |

### 1.1 `Kontoutdrag_officiella` — transaktioner

Kolumnlayout:

| Kol | Innehåll | Exempel |
|-----|----------|---------|
| 1 | År | `2014` |
| 2 | Månad (1–12) | `12` |
| 3 | Dag (1–31) | `30` |
| 4 | Beskrivning | `PILEGÅRDEN 2 &` |
| 5 | Belopp (neg = utgift) | `-7824` |
| 6–7 | Nollor (padding) | `0`, `0` |
| 8 | **Kategori** | `hyra (inkl. 1k amortering)`, `+` (inkomst), ` -` (förflyttning) |
| 9–11 | Tomma | |
| 12 | Regular/Ignore-flagga | `Regular`, `Ignore` |

Totalt **1 654 rader** (2014 + 2015).

### 1.2 `Budget (YYYY)` — den fullständiga budget/utfall/kvar-vyn

Flikarna är uppdelade i tre block vertikalt:

| Rader | Sektion | Innehåll |
|-------|---------|----------|
| 1–22 | Metadata | Saldo, kvar att spendera, buffertberäkningar m.m. |
| 23–58 | **IN (budget)** | En rad per kategori · månadsbelopp i kol F–Q · Summa i kol R |
| 58 | IN: Summa | Summa per månad över alla IN-rader |
| 71–106 | **UT (utfall)** | Samma kategorier som IN · månadsbelopp i kol F–Q |
| 106 | UT: Summa | |
| 109–144 | **KVAR** | `IN + UT` per kategori per månad (UT är negativt → positivt resultat = under budget) |
| 144 | KVAR: Summa | |

**Viktigt**: I just denna version av filen är UT- och KVAR-sektionerna fulla av `#VALUE!`
eftersom formlerna länkar till externa arbets­böcker som inte följde med. De
**rätta** UT/KVAR-värdena måste alltså rekonstrueras genom att summera
trans­aktioner från `Kontoutdrag_officiella` och räkna ut `KVAR = IN + UT`
(UT är negativt).

IN-sektionen är däremot **intakt** och innehåller riktiga budgetvärden.

**Observation om IN 2014**: I `Budget (2014)` finns IN-rader för **alla 12 månader** inklusive januari.
Data visar 28 kategorier × 12 månader = 336 rader per år för både 2014 och 2015.
Kolumn 6 = Januari, Kolumn 7 = Februari, ..., Kolumn 17 = December.

### 1.3 Kategoriexempel (2014 IN-sektion)

Kategorierna inkluderar `hyra (inkl. 1k amortering)`, `si och akassa`,
`hemförsäkring`, `liv- o sjukförsäkring etc`, `csn`, `el`, `internet`,
`telefonsamtal`, `hemlagad mat`, `lunch utemat`, `nöjes utemat`, `alkohol`,
`mat i samband med supa`, `spara almänt`, `spara till amortering`,
`kläder`, `presenter`, `hushåll, reparationer etc …` m.fl.
— **exakt samma strängar** återfinns i kolumn 8 i kontoutdragsbladet.

---

## 2. Jämförelse mot nuvarande kod

### 2.1 Nuvarande dataflöde

```
Excel (`Pelles Budget.xls`)
  └─► TransactionHandler
        └─► Transaction { DateAsDate, Description, AmountAsDouble,
                          Categorizations.Categories[0].{Group, Name} }
              └─► CategoryName = "Group Name"  (← sammanslagning!)
                    └─► TableGetter.GroupOnMonthAndCategory(...)
                          └─► BudgetRowFactory → AmountsForMonth["2014 January"]
                                └─► TextToTableOutPuter { BudgetRows, ColumnHeaders }
                                      └─► BudgetStructureBuilder → strukturerad vy
                                            └─► UtgiftsHanterareUiBinder → gv_budget / gv_Kvar
```

`InBudget`-sidan:

```
TestData/BudgetIns.json
  └─► List<InBudget> { CategoryDescription, BudgetValue, YearAndMonth }
        └─► InBudgetHandler → List<Rad> { RadNamnY, Kolumner["2014 January"] = ... }
              └─► InBudgetMath.SnurraIgenom(inBudget, utgifter)
                    └─► kvar = inBudget.Kolumner[key] + utgiftsMånad.Value
                          └─► gv_Kvar  (⚠ INTE aktiv just nu — se 2.3)
```

### 2.2 Viktiga detaljer i nuvarande kod

- **Månadsnyckel**: `Transaction.GetYearMonthName` ger `"YYYY MMMM"` på invariant­kultur (t.ex. `"2014 January"`).
- **Kategori­nyckel**: `Transaction.CategoryName` = `$"{Group} {Name}"` — dvs grupp-prefix. För budgettabell / facit-jämförelse (D7) används `BudgetTableCategoryKey`: rent `Name` när `Group` är tom, annars samma som `CategoryName`. `TableGetter.GroupOnMonthAndCategory` och `BudgetRowFactory` använder den nyckeln.
- **Klassificering** i `BudgetStructureBuilder`:
  - Inkomst: kategorinamn trimmat är exakt `"+"` (inte `Contains("+")` — undviker t.ex. `värnamoresor+övriga`)
  - Förflyttning: `CategoryText.Contains(" -")` (mellanslag före minus)
  - Utgift: övrigt
- **Budget Total-fliken** (`gv_budget`) visar IN + UT + summerings­rader.
- **Kvar-fliken** (`gv_Kvar`): `KvarTextTableBuilder` + `InBudgetMath.SnurraIgenom` (samma som `ConsoleBudgeter` från facit).

### 2.3 Gap mellan facit och nuvarande kod

| # | Område | Facit kräver | Nuvarande kod | Gap |
|---|--------|-------------|---------------|-----|
| G1 | Budget Total | **IN + UT** per kategori i samma tabell (summerat per månad) | **Delvis åtgärdat:** IN från `BudgetIns` adderas till `BudgetRow` före struktur/summering | Full IN/UT/KVAR-sektion som i Excel (tre block) kan fortfarande kräva UI-uppdelning |
| G2 | Kvar-fliken | `IN + UT` per kategori per månad | **Åtgärdat:** `KvarTextTableBuilder` / `InBudgetMath.SnurraIgenom` + `TextToTableOutPuter` till `gv_Kvar` (delat med konsol) | Kategorier som bara finns i IN utan UT-rad får tomma kvarceller tills logiken utökas |
| G3 | Kategori-nyckel | Rent kategorinamn (t.ex. `"el"`) vid tom grupp | **Delvis åtgärdat:** `Transaction.BudgetTableCategoryKey` + `TableGetter` / `BudgetRowFactory` använder rent namn när `Group` är tom; icke-tom grupp behåller `CategoryName` | Facit-jämförelse för rader med riktig grupp i XML kan fortfarande kräva uppföljning |
| G4 | Tecken-konvention | IN ≥ 0, UT ≤ 0, KVAR = IN + UT | Samma i `InBudgetMath.SnurraIgenom` | OK |
| G5 | Auto-kategorisering | `CategoryHandler` matchar hela `InfoDescription` exakt | Case-insensitive trim-jämförelse på hela beskrivningen | Facit visar många fria­texter (`PILEG$RDENS SERVICEBUT ASKIM`) → kräver substring/regex-matchning eller manuellt angivna aliaser |
| G6 | BudgetIns.json täckning | 363 in-rader / år × 2 år = 726 rader | Testdata har 10 rader för år 2016 | Måste fyllas med riktig budget för 2014/2015 (kan genereras direkt ur facit-filen) |
| G7 | Filkälla | Flera transaktioner per dag, sve-kultur­decimaler | Läser `.xls` via `TransactionHandler` | Kontrollera att läsningen levererar exakt samma 1 654 rader |
| G8 | Regulatorflagga | `Regular` / `Ignore` i kol 12 | **Åtgärdat för budgettabell:** `TransactionTransformer` sätter `SourceEntryType` från `KontoEntry.EntryType`; `TableGetter.GroupOnMonthAndCategory` exkluderar `Ignore` från aggregering (D12). Transaktionslistan i UI kan fortfarande visa alla rader. |
| G9 | Månadskultur | `"YYYY January"` (engelska) | `GetMonthAsFullString` använder redan `InvariantCulture` | **Verifierat** i `TableGetterCategoryKeyTests` (D10) |

---

## 3. Facit-format (AI-läsbart)

All facit läggs i ett **eget shared-projekt** (D11 = b). UTF-8 JSON med
indentation. JSON är AI-läsbart, går att diffa, och kan laddas i C#-tester
via `System.Text.Json`.

### 3.1 Filer

```
WebBankBudgeterTests.Facit/
├── Facit/
│   ├── README.md                       # Förklarar ursprung, format och regler
│   ├── transactions-2014.json          # 809 poster (inkl. Ignore-rader per D12)
│   ├── transactions-2015.json          # 845 poster
│   ├── budget-in-2014.json             # 396 poster (33 kat × 12 mån — januari genererad per D5)
│   ├── budget-in-2015.json             # 396 poster
│   ├── expected-ut-2014.json           # Σ transaktioner per (kat, mån) — exkl. transfers (D2) och Ignore (D12)
│   ├── expected-ut-2015.json
│   ├── expected-transfers-2014.json    # Bara " -"-transfers (D2 = c)
│   ├── expected-transfers-2015.json
│   ├── expected-kvar-2014.json         # Union av IN ∪ UT, 0 där sida saknas (D3/D4)
│   └── expected-kvar-2015.json
├── FacitLoader.cs
└── WebBankBudgeterTests.Facit.csproj
```

### 3.2 Schema per fil

**`transactions-YYYY.json`** — en rad per bank­transaktion. `Flag` är obligatorisk
(`"Regular"` eller `"Ignore"`); extraktorn måste skriva fältet (se M1-status):

```json
[
  {
    "Year": 2014,
    "Month": 1,
    "Day": 1,
    "Description": "Vasttrafik AB",
    "Amount": -500.00,
    "Category": "Småresor ej supa",
    "Flag": "Regular"
  }
]
```

**`budget-in-YYYY.json`** — en rad per (kategori, månad):

```json
[
  {
    "Category": "alkohol",
    "Year": 2014,
    "Month": 2,
    "MonthName": "February",
    "BudgetAmount": 247.50
  }
]
```

**`expected-ut-YYYY.json`** — summan av trans­aktioner per (kategori, månad). Negativa belopp:

```json
[
  {
    "Category": " -",
    "Year": 2014,
    "Month": 1,
    "MonthName": "January",
    "ActualAmount": -12257.06
  }
]
```

**`expected-kvar-YYYY.json`** — per (kategori, månad): `Remaining = BudgetAmount + ActualAmount`
(UT är negativt → positivt = under budget). Unionsregler:

- **D3** — om kategori finns i UT men inte i IN: `BudgetAmount = 0`.
- **D4** — om kategori finns i IN men inte i UT: `ActualAmount = 0`, `Remaining = BudgetAmount`.
- **D5** — januari saknas i IN för **både 2014 och 2015** i Excel (verifierat: 0 rader med
  `"MonthName": "January"` i `budget-in-2014.json` och `budget-in-2015.json`; alla andra
  månader har 33 rader). Extraktorn genererar januari med testdata likt övriga månader
  (se 0.1 M1-steg nedan).

```json
[
  {
    "Category": "el",
    "Year": 2014,
    "Month": 3,
    "MonthName": "March",
    "BudgetAmount": 200.00,
    "ActualAmount": -178.50,
    "Remaining": 21.50
  }
]
```

### 3.3 Principer för AI-läsbarhet

1. **Platta fält** — inga nästlade objekt utöver det helt nödvändiga. Gör sök­ning och diff lätt.
2. **Stabil sortering** — alltid `Category` → `Year` → `Month` stigande. Gör diff mellan körningar deterministisk.
3. **Explicita månadsnamn** (`MonthName`) — gör det lätt att läsa utan att räkna mappning `int → str`.
4. **Belopp som `decimal` med två decimaler** (D1) — JSON-tal serialiseras med 2 decimaler (`"BudgetAmount": 247.50`).
5. **En fil per dimension × år** — lätt att byta ut / utöka utan att röra allt.
6. **Indentation** — human readable och git-diff-vänligt (radbrytnings­diffar).
7. **Kultur­oberoende** — alla tal med punkt som decimalavskiljare, JSON enligt RFC 8259.

### 3.4 Exempel på `Facit/README.md`

```markdown
# Facit-data (utdragen ur Pelles-budget-slim-2014-2015-gform.xlsx)

## Ursprung
- Källa: `C:\Files\Dropbox\budget\Program\webbankbudgeter\Pelles-budget-slim-2014-2015-gform.xlsx`
- Filen är ett fryst snapshot av användarens riktiga budget 2014–2015.
- Extrakt gjort av `tools/FacitExtractor/` (engångs­körning, inte en del av bygget).

## Filer
| Fil | Innehåll | Källrad i Excel |
|-----|----------|-----------------|
| transactions-YYYY.json | En rad per transaktion | `Kontoutdrag_officiella` rad 2+ |
| budget-in-YYYY.json    | Budget per kategori per månad | `Budget (YYYY)` rad 25–57 |
| expected-ut-YYYY.json  | Summa transaktioner per (kat, mån) | Beräknat ur transaktioner |
| expected-kvar-YYYY.json| Budget + utfall per (kat, mån) | Beräknat (IN + UT) |

## Invarianter som testas
1. `sum(transactions.amount where Flag != "Ignore") per kategori per månad == expected-ut` (FluentAssertions `BeApproximately(0.01m)`)
2. `budget-in + expected-ut == expected-kvar` (per kategori per månad, unionsregler per D3/D4)
3. Transaktioner med `Flag == "Ignore"` räknas **inte** med i UT.
4. Antal transaktioner per år: 2014 = 809, 2015 = 845 (inkluderar Ignore-rader, D12).
5. IN 2014 har **396 rader** (33 kategorier × 12 månader — januari genererad per D5).
6. IN 2015 har **396 rader** (33 kategorier × 12 månader — januari genererad per D5).
7. Transfers (`" -"`) ligger i `expected-transfers-YYYY.json`, **ej** i `expected-ut-YYYY.json` (D2).
```

---

## 4. Integrationstest­plan

Tester placeras i befintliga `WebBankBudgeterUiTest`-projektet, och i ett
nytt **`WebBankBudgeterServiceTest/FacitIntegrationTests.cs`** för service-nivå.

### 4.1 Hjälpinfrastruktur

Shared-projekt `WebBankBudgeterTests.Facit/` (D11 = b). Båda testprojekten
(`WebBankBudgeterServiceTest`, `WebBankBudgeterUiTest`) lägger till en
`ProjectReference` till detta.

**`WebBankBudgeterTests.Facit/FacitLoader.cs`**:

```csharp
public static class FacitLoader
{
    private static string FacitDir =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Facit");

    public static List<TransactionFacit> LoadTransactions(int year) =>
        Load<List<TransactionFacit>>($"transactions-{year}.json");

    public static List<BudgetInFacit> LoadBudgetIn(int year) =>
        Load<List<BudgetInFacit>>($"budget-in-{year}.json");

    public static List<BudgetUtFacit> LoadExpectedUt(int year) =>
        Load<List<BudgetUtFacit>>($"expected-ut-{year}.json");

    public static List<BudgetUtFacit> LoadExpectedTransfers(int year) =>
        Load<List<BudgetUtFacit>>($"expected-transfers-{year}.json");

    public static List<BudgetKvarFacit> LoadExpectedKvar(int year) =>
        Load<List<BudgetKvarFacit>>($"expected-kvar-{year}.json");

    private static T Load<T>(string name) =>
        JsonSerializer.Deserialize<T>(
            File.ReadAllText(Path.Combine(FacitDir, name)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
}

// Belopp är decimal överallt (D1).
public record TransactionFacit(int Year, int Month, int Day,
    string Description, decimal Amount, string Category, string Flag);
public record BudgetInFacit(string Category, int Year, int Month,
    string MonthName, decimal BudgetAmount);
public record BudgetUtFacit(string Category, int Year, int Month,
    string MonthName, decimal ActualAmount);
public record BudgetKvarFacit(string Category, int Year, int Month,
    string MonthName, decimal BudgetAmount, decimal ActualAmount, decimal Remaining);
```

`WebBankBudgeterTests.Facit.csproj` (SDK-style):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <None Update="Facit\**\*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

(`<Content Update="...">` fungerar **inte** här eftersom JSON inte är `Content`-item
per default i SDK-style-projekt.)

### 4.2 Tester på service-nivå (snabba, isolerade)

Fil: `WebBankBudgeterServiceTest/FacitBudgetTests.cs` (ny)

| Test | Verifierar |
|------|------------|
| `AggregationFromTransactions_MatchesExpectedUt_2014` | Mata `TableGetter.GetTextTableFromTransactions` med `transactions-2014.json` (filtrerade på `Flag != "Ignore"`, exkl. transfers per D2) → BudgetRows ska matcha `expected-ut-2014.json` (tolerans per D1) |
| `AggregationFromTransactions_MatchesExpectedUt_2015` | Samma för 2015 |
| `BudgetStructureBuilder_TotalRow_EqualsIncomeMinusExpenses` | Månad för månad: Budget-totalrad = Σ(+) + Σ(Ut), exkl. ` -`-förflyttningar (matchar `BudgetStructureBuilder.cs:28-29`) |
| `IgnoreFlag_IsExcludedFromAggregation` | Transaktion med `Flag == "Ignore"` får **inte** påverka summan |
| `KvarCalculation_InPlusUt_EqualsExpectedKvar` | `InBudgetMath.SnurraIgenom(budget-in-2014, expected-ut-2014) == expected-kvar-2014` (unionsregler per D3/D4) |
| `MonthKey_MatchesFacitFormat` | `Transaction.GetYearMonthName(new DateTime(2014,1,1))` → `"2014 January"` på en `sv-SE`-tråd (verifierar D10) |
| `CategoryNormalization_MatchesFacit` | Hydrerad `Transaction` med `Group=""`, `Name="el"` → grupperingsnyckel = `"el"` (verifierar D7) |

### 4.3 Tester på UI-nivå (redan påbörjat)

Fil: `WebBankBudgeterUiTest/BudgetIntegrationTests.cs` (ny)

| Test | Verifierar |
|------|------------|
| `FullFlow_2014_FillsBudgetGridWithIn_Ut_Kvar` | Skapa `UtgiftsHanterareUiBinder`, mata in fejk-`webBankBudgeter` som returnerar `transactions-2014.json`-data, `budget-in-2014.json` → grid ska innehålla rader för varje kategori och varje månads­kolumn ska matcha `expected-kvar-2014.json` |
| `GridRowCount_MatchesDistinctCategories_2014` | Antal unika kategorier i facit == antal kategorirader i grid (exklusive summerings­rader) |
| `SummaColumn_PerRow_EqualsSumOfMonths` | För varje kategorirad: `Summa-cell == sum(12 månads­celler)` |
| `SummaryRows_AreBold_AndGray` | Rader som börjar `===` ska vara feta och grå (redan täckt) |

### 4.4 Regressionstest

| Test | Verifierar |
|------|------------|
| `Facit_FileCounts_AreStable` | Antal poster i varje facit-fil är fasta: `transactions-2014.json` = 809, `-2015.json` = 845, `budget-in-{2014,2015}.json` = 396 (33 kat × 12 mån efter D5-genereringen) — om extrakten regenereras och antal ändras slår testet |
| `FacitSum_TotalIn_2014_EqualsExpected` | Σ av alla `BudgetAmount` i `budget-in-2014.json` matchar Excel-summa-raden (konstanten fastställs vid första körning av extraktorn, verifieras mot cell i `Budget (2014)`) |

### 4.5 Toleranser (D1 + D13)

- Alla belopp är `decimal`. Jämförelse: `value.Should().BeApproximately(expected, 0.01m)`.
- Tolerans behövs i praktiken inte (decimal är exakt) men lämnas för robusthet mot
  eventuella avrundningar under aggregering.
- Negativa belopp: både `expected-ut` och `Remaining` får vara negativa → inga
  `Math.Abs`-anrop i asserts.

---

## 5. Implementations­plan — 6 milstolpar

### M0 — Verifiera `TransactionHandler` + stabil bygg (förkrav för M5)

`TransactionHandler` **finns** i kodbasen (`WebBankBudgeterService/TransactionHandler.cs`)
— ingen återställning från git-historik behövs. D6 = a (verifiera nuvarande klass).

**Att göra i M0:**
1. Läs `WebBankBudgeterService/TransactionHandler.cs` och bekräfta att
   `TransactionList.Account.AvailableAmount` + `TransactionList.Transactions`
   exponeras som `WebBankBudgeter.cs:222` förväntar.
2. Kör inläsning mot `pelles-budget-slim-2014-2015.xlsx` eller motsvarande och bekräfta
   antal transaktioner (≈ 1 654 rader över 2014 + 2015).
3. Åtgärda filhands-konflikten som gör att bygget failar när `WebBankBudgeterUi`
   kör samtidigt (MSBuild `MSB3021`/`MSB3027` vid kopiering till `WebBankBudgeterUi/bin/...`).
   Detta blockerar CI men inte facit-test­körningen.

M0 blockerar **inte** M1–M4 (facit-tester körs utan att ladda riktiga `.xls`).
Måste vara klar innan M5.

### M1 — Skapa facit-mappen och extraktor-verktyget

**Filer:**

```
tools/FacitExtractor/
├── FacitExtractor.csproj                    # net8.0, ClosedXML + System.Text.Json
└── Program.cs                               # Extraherar .xlsx → Facit/*.json
WebBankBudgeterTests.Facit/Facit/            # Shared-projekt (D11 = b), commitas in
│   ├── README.md
│   ├── transactions-2014.json               # 809 poster inkl. Ignore (D12)
│   ├── transactions-2015.json               # 845 poster
│   ├── budget-in-2014.json                  # 396 poster (33 × 12, januari genererad per D5)
│   ├── budget-in-2015.json                  # 396 poster
│   ├── expected-ut-2014.json                # exkl. transfers (D2) och Ignore (D12)
│   ├── expected-ut-2015.json
│   ├── expected-transfers-2014.json         # endast " -"-transfers
│   ├── expected-transfers-2015.json
│   ├── expected-kvar-2014.json              # union IN ∪ UT, 0 där sida saknas (D3/D4)
│   └── expected-kvar-2015.json
```

Extraktorn körs **en gång**, genererar JSON, och resultatet committas. Verktyget
är inte en del av bygget — bara körbart manuellt om facit behöver uppdateras.
Efter lyckad extraktion: kör `ConsoleBudgeter` med `--out` mot
`Facit/facit-2014-2015.txt` (se 0.6 / `Facit/README.md`) så **textfacit**
följer samma kod som appen.

_Status:_ Prototyp finns i `C:\Users\nisse\AppData\Local\Temp\xlsx-reader\`.
Genererar `transactions-*.json` (809/845 poster **utan `Flag`**) och `budget-in-*.json`
(363 poster — januari saknas). `expected-ut-*.json` genereras från transaktioner
men **inkluderar** transfers (` -`). `budget-kvar-*.json` är 2 byte (tomma).

**M1 är inte klar förrän:**
1. Extraktorn använder `decimal` genomgående (D1).
2. `Flag`-kolumnen (`"Regular"` / `"Ignore"` från Excel kol 12) skrivs ut i `transactions-*.json`
   (D12 — Ignore-rader inkluderas, men exkluderas senare i `expected-ut`).
3. `expected-ut-*.json` exkluderar både transfers och Ignore-rader (D2 + D12).
4. `expected-transfers-*.json` produceras för ` -`-kategorin (D2 = c).
5. `budget-in-*.json` fylls på med januari-rader med testvärden likt övriga månader
   (D5 — resulterar i 396 poster istället för 363).
6. `expected-kvar-*.json` genereras som union av IN ∪ UT enligt D3/D4.
7. Extraktorn skriver `Facit/README.md` med de 7 invarianterna från 3.4.
8. Efter lyckad körning: regenerera `facit-2014-2015.txt` via `ConsoleBudgeter --out` (se 0.6).

### M2 — Facit-infrastruktur i shared-projekt

D11 = b (eget shared-projekt `WebBankBudgeterTests.Facit/`):

1. Skapa projektet med `.csproj` enligt 4.1 (net8.0, `None Update` för JSON).
2. Flytta facit-filerna från extraktor-outputen in i `WebBankBudgeterTests.Facit/Facit/`.
3. Lägg till `FacitLoader.cs` med records enligt 4.1 (alla belopp som `decimal`).
4. Lägg till `ProjectReference` från `WebBankBudgeterServiceTest` och
   `WebBankBudgeterUiTest` till det nya projektet.
5. Lägg till `FluentAssertions` som paketref i båda testprojekten (D13) om det saknas.

### M3 — Service-integrations­tester

Implementera `FacitBudgetTests.cs` med de 6 testerna i 4.2. Förväntat resultat:
- `AggregationFromTransactions_MatchesExpectedUt_*` kan **felas** i första iterationen om kategorinyckel eller kultur inte matchar — det är poängen, testerna driver fram rätt beteende.
- `MonthKey_MatchesFacitFormat` är en invariant­check.

### M4 — UI-integrations­tester

Implementera `BudgetIntegrationTests.cs` med testerna i 4.3. Här krävs en
liten **test-fake** som ersätter `WebBankBudgeter`-fasaden med förladdad
facit-data (för att undvika läsning av riktig `.xls`). Mönster:

```csharp
var table = BuildTableFromFacit(transactions: FacitLoader.LoadTransactions(2014));
var grid = new DataGridView();
var binder = new UtgiftsHanterareUiBinder(grid);
binder.BindToBudgetTableUi(table);
AssertGridMatchesExpectedUt(grid, FacitLoader.LoadExpectedUt(2014));
```

### M5 — Driv in koden mot facit

Förkrav: M0 är klar.

1. **Budget Total**: `FillTablesAsync` slår in IN-rader (`HämtaInDataRaderFiltrerat`) i transaktionstabellen via `InbudgetHandler.BudgetTableInMerger` innan `BindToBudgetTableUi` (M5.1 / G1).
2. **Kvar**: `BuildKvarTextTable` delegerar till `KvarTextTableBuilder` (samma som konsolen från facit): `InBudgetMath.SnurraIgenom` mot utgiftsrader före IN-merge, sedan bindning via `UtgiftsHanterareUiBinder`.
3. **BudgetIns.json** (D9): Generera `BudgetIns.json` för 2014/2015 ur facit. Utvärdera
   enligt 0.4 om befintligt schema ska behållas eller migreras till facit-formatet.
4. **Kategori-normalisering** (D7): **Klart** i service — `BudgetTableCategoryKey` + `TableGetter` / `BudgetRowFactory`.
5. **Ignore-flagga** (D12): **Klart** i `TableGetter` (exkludera `KontoEntryType.Ignore` vid budgetaggregering) + `SourceEntryType` från `TransactionTransformer`.
6. **Månadskultur** (D10): **Klart** — `GetMonthAsFullString` använder redan `InvariantCulture`; test tillagt.
7. **UI-presentation** (0.2): `UtgiftsHanterareUiBinder.DoubleTo1000SeparatedNoDecimals` använder `N0` med **`sv-SE`** (tusentalsavgränsare enligt plan). Påverkar bara displayen, inte modellen.

---

## 6. Risker och oklarheter

| # | Risk | Mitigering |
|---|------|------------|
| R1 | `dotnet build` kan fallera med `MSB3021/MSB3027` när `WebBankBudgeterUi` kör och låser `bin`-DLL:er | M0: stoppa UI-processen före full build/test (eller kör build utan UI-projekt) |
| R2 | UT/KVAR är `#VALUE!` i Excel-filen | Accepteras — vi räknar dem själva ur transaktionerna; det är ju precis det appen ska göra |
| R3 | Kategori­namn i Excel har specialtecken (`ö`, `å`, `£`) | JSON/UTF-8 hanterar det; kontrollera att `.csproj` använder `utf-8` BOM eller explicit encoding |
| R4 | Svensk kultur i Excel vs invariant i koden | Extraktorn använder `sv-SE` på Excel-sidan, skriver punkt-decimaler i JSON; koden använder invariant → match OK |
| R5 | Transaktioner för 2013-december finns i filen (Allkortsfaktura) | Filtrera strikt på `Year == 2014` resp `Year == 2015` |
| R6 | Pågående ändring: `gv_Kvar` visar just nu Budget Total-data (TODO.md-arbetet utfört, se `WebBankBudgeterUi.cs:230-233`) | M5.2 avgör om tillståndet behålls eller om riktig Kvar-vy återinförs |
| R7 | Floating-point-drift vid summering av >100 rader om D1 = c | Beslut D1 (default a) hanterar detta med olika toleranser för cell vs aggregat |
| R8 | Sorteringsskillnad: kod sorterar `OrderByDescending(CategoryText)` (`TableGetter.cs:39`), facit sorterar stigande | Beslut D14 (default b): jämför som dictionary i tester |

---

## 7. Leverans­ordning (rekommenderad)

0. **Beslutslistan i sektion 0** — fyll i alla 14 valen.
1. **M1** (extraktor + facit-filer, klar enligt M1-checklistan) — ger sanningsunderlag.
2. **M2** (FacitLoader) — möjliggör tester.
3. **M3** (service­tester) — validerar tran­saktions­aggregering och Kvar-matte.
4. **Commit** — allt ovan kan committas utan att ändra produktions­kod.
5. **M4** (UI-tester som failar) — dokumenterar exakt vilka gap som finns.
6. **M0** (stabil byggmiljö + verifierad `TransactionHandler`) — kan göras parallellt med M3/M4.
7. **M5** (driv in koden) — en punkt i taget tills alla tester grönar.

Varje milstolpe ska vara committable på egen hand och ha alla tester gröna.
