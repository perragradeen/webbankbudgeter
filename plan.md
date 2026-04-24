# Plan: Efterlikna `pelles-budget-slim-2014-2015.xlsx` i WebBankBudgeter

Facit: `C:\Files\Dropbox\budget\bak\pelles-budget-slim-2014-2015.xlsx`

Målet är att UI:t ska visa exakt samma struktur och data som Excel-förlagan:

- **Inkomster** (budget) i egen sektion, per kategori och månad
- **Utgifter** (utfall) kategoriserade och inlagda i respektive år och månad
- **Kvar per månad** = `IN + UT` per kategori per år/månad
  (UT är negativt → positivt resultat = under budget)

---

## 0. Beslut som behöver tas innan M2 startar

Dessa val styr facit-format, tester och normalisering. Märk varje rad med valt
alternativ direkt i denna fil innan extraktorn körs igen.

| # | Område | Alternativ | Default-förslag | Beslut |
|---|--------|-----------|-----------------|--------|
| D1 | **Beloppsprecision** i extraktor och tester | (a) `double` med tolerans ±0,01 per cell och ±0,10 för aggregat · (b) `decimal` hela vägen · (c) `double` men tolerans ±0,01 överallt (kan failas av summor) | (a) | _____ |
| D2 | **Transfers (`" -"`)** i `expected-ut-YYYY.json` | (a) Inkluderas (nuvarande prototyp) · (b) Exkluderas helt · (c) Bryts ut till egen fil `expected-transfers-YYYY.json` | (c) — egen fil; håller `expected-ut` i synk med `BudgetStructureBuilder` som ändå filtrerar bort transfers från utgifts-summan | _____ |
| D3 | **`expected-kvar` när IN saknas** för en (kat, månad) (UT-only) | (a) `BudgetAmount = 0`, raden inkluderas · (b) Hoppa raden helt · (c) Inkludera men markera `BudgetAmount = null` | (a) | _____ |
| D4 | **`expected-kvar` när UT saknas** (IN-only, ingen spend) | (a) `ActualAmount = 0`, `Remaining = BudgetAmount` · (b) Hoppa raden | (a) | _____ |
| D5 | **Januari 2014 i IN-sektionen** (faktiskt saknas i Excel: 0 rader) | (a) Behåll som tomt — dokumentera som invariant · (b) Generera januari-rader med `BudgetAmount = 0` · (c) Kopiera februari som proxy | (a) — dokumentera; spegla Excel | _____ |
| D6 | **`TransactionHandler`-strategi** | (a) Behåll nuvarande klass, verifiera inläsning mot facit · (b) Skriv om från grunden mot facit-data · (c) Ersätt med ny tunn klass som läser direkt från facit i tester och från `.xls` i prod | (a) först; (b) som plan B | _____ |
| D7 | **Group-prefix-normalisering** så `"el"` matchar `"Fast el"` | (a) Ändra `Categories.ToString()` att returnera enbart `Name` om `Group` är tom · (b) Använd befintlig `CategoryNameNoGroup` i `TableGetter.GroupOnMonthAndCategory` · (c) Skapa ny `CategoryNameNormalized` | (b) — `CategoryNameNoGroup` finns redan, minst risk | _____ |
| D8 | **Sparrader** (`spara almänt`, `spara till amortering`) | (a) Behandla som utgifter (default i koden idag) · (b) Egen sektion "sparande" i UI och facit | (a) | _____ |
| D9 | **`BudgetIns.json`-format** för 2014/2015 i prod-koden | (a) Behåll befintligt schema (`CategoryDescription`/`BudgetValue`/`YearAndMonth`) — generera dem från facit · (b) Migrera prod-koden till samma format som `budget-in-YYYY.json` | (a) — minst kodändring i InbudgetHandler | _____ |
| D10 | **Kultur i `GetMonthAsFullString`** | (a) Verifiera först, ändra bara om den inte redan är invariant · (b) Ändra till `CultureInfo.InvariantCulture` direkt i M5 | (a) | _____ |
| D11 | **Facit-mappens placering** | (a) `WebBankBudgeterUiTest/Facit/` (per nuvarande plan) · (b) Eget `WebBankBudgeterTests.Facit/` shared-projekt som både UI- och Service-tester refererar | (b) — undviker dubblering av loader och JSON-filer | _____ |
| D12 | **`Ignore`-rader i facit** | (a) Inkludera dem i `transactions-YYYY.json` med `Flag = "Ignore"`, exkludera dem ur `expected-ut` (testar filtreringen) · (b) Filtrera bort redan i extraktorn (då testas inte filtreringen) | (a) — gör filterregeln testbar | _____ |
| D13 | **Toleransstrategi i asserts** | (a) Per-cell `Math.Abs(a-b) <= 0.01` · (b) Använd FluentAssertions `BeApproximately(0.01)` · (c) `Math.Round(x, 2)`-jämförelse | (b) om FluentAssertions redan är referens, annars (a) | _____ |
| D14 | **Stabil sortering i koden** vs facit | Facit sorteras `Category` → `Year` → `Month`. `TableGetter` sorterar `OrderByDescending(CategoryText)`. (a) Ändra koden att sortera som facit · (b) Behåll kodens sortering, jämför som dictionary i tester | (b) — sorteringsskillnad bör inte fela tester | _____ |

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
              └─► WebBankBudgeter.SnurraIgenom(inBudget, utgifter)
                    └─► kvar = inBudget.Kolumner[key] + utgiftsMånad.Value
                          └─► gv_Kvar  (⚠ INTE aktiv just nu — se 2.3)
```

### 2.2 Viktiga detaljer i nuvarande kod

- **Månadsnyckel**: `Transaction.GetYearMonthName` ger `"YYYY MMMM"` på invariant­kultur (t.ex. `"2014 January"`).
- **Kategori­nyckel**: `Transaction.CategoryName` = `$"{Group} {Name}"` — dvs grupp-prefix. `BudgetRow.CategoryText` är samma sträng.
- **Klassificering** i `BudgetStructureBuilder`:
  - Inkomst: `CategoryText.Contains("+")`
  - Förflyttning: `CategoryText.Contains(" -")` (mellanslag före minus)
  - Utgift: övrigt
- **Budget Total-fliken** (`gv_budget`) visar IN + UT + summerings­rader.
- **Kvar-fliken** (`gv_Kvar`) visade samma som Budget Total efter tidigare refactor. (`VisaKvarRader_BindInPosterRaderTillUiAsync` finns men anropas inte längre; den beräknar riktigt kvar via `SnurraIgenom`.)

### 2.3 Gap mellan facit och nuvarande kod

| # | Område | Facit kräver | Nuvarande kod | Gap |
|---|--------|-------------|---------------|-----|
| G1 | Budget Total | Tre sektioner: **IN**, **UT**, **KVAR** per kategori per månad | Inkomster = transaktioner som innehåller `"+"` i kategori­namnet; ingen riktig IN från BudgetIns.json visas per kategori | Budget Total måste hämta IN från `BudgetIns.json` och kombinera med UT från trans­aktioner |
| G2 | Kvar-fliken | `IN + UT` per kategori per månad | Visar samma som Budget Total | Kalla `SnurraIgenom` → `BindKvarBudgetTableUi` (ny sorts bindning för `Rad`-modellen) |
| G3 | Kategori-nyckel | Rent kategorinamn (t.ex. `"el"`) | `"Group Name"` (t.ex. `"Fast el"`) eller `" el"` (om Group är tom) — `Categories.ToString()` i `Model/Categories.cs:10` returnerar alltid `$"{Group} {Name}"` med ledande blanksteg | Se beslut **D7**. `CategoryNameNoGroup` finns redan i `Transaction.cs:36`; använd den i `TableGetter.GroupOnMonthAndCategory` när Group är tom/null |
| G4 | Tecken-konvention | IN ≥ 0, UT ≤ 0, KVAR = IN + UT | Samma i `SnurraIgenom` | OK |
| G5 | Auto-kategorisering | `CategoryHandler` matchar hela `InfoDescription` exakt | Case-insensitive trim-jämförelse på hela beskrivningen | Facit visar många fria­texter (`PILEG$RDENS SERVICEBUT ASKIM`) → kräver substring/regex-matchning eller manuellt angivna aliaser |
| G6 | BudgetIns.json täckning | 363 in-rader / år × 2 år = 726 rader | Testdata har 10 rader för år 2016 | Måste fyllas med riktig budget för 2014/2015 (kan genereras direkt ur facit-filen) |
| G7 | Filkälla | Flera transaktioner per dag, sve-kultur­decimaler | Läser `.xls` via `TransactionHandler` | Kontrollera att läsningen levererar exakt samma 1 654 rader |
| G8 | Regulatorflagga | `Regular` / `Ignore` i kol 12 | Oklart om den filtreras | Beslut **D12**: `Ignore`-rader inkluderas i facit men exkluderas i `expected-ut` så filterregeln blir testbar |
| G9 | Månadskultur | `"YYYY January"` (engelska) | `Transaction.GetMonthAsFullString` — kultur ej verifierad i koden | Beslut **D10**: Verifiera att `MMMM`-formatteringen är invariant; om inte, ändra i M5 |

---

## 3. Facit-format (AI-läsbart)

All facit läggs under `WebBankBudgeterUiTest/Facit/` som **UTF-8 JSON med
indentation**. JSON är AI-läsbart, går att diffa, och kan laddas i C#-tester
via `System.Text.Json`. En mapp per år hade varit OK, men en fil per år är
enklare (736 rader/år max för budget-in, 845 rader/år för transaktioner).

### 3.1 Filer

Placering styrs av beslut **D11** (default: eget shared-projekt).

```
<Facit-rot>/
├── README.md                       # Förklarar ursprung, format och regler
├── transactions-2014.json          # 809 poster
├── transactions-2015.json          # 845 poster
├── budget-in-2014.json             # 363 poster (33 kat × 11 mån, januari saknas)
├── budget-in-2015.json             # 363 poster
├── expected-ut-2014.json           # Σ transaktioner per (kat, mån) — exkl. transfers per D2
├── expected-ut-2015.json
├── expected-transfers-2014.json    # Bara " -"-transfers (om D2 = c)
├── expected-transfers-2015.json
├── expected-kvar-2014.json         # Per (kat, mån) enligt D3+D4-regler
└── expected-kvar-2015.json
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
(UT är negativt → positivt = under budget). Unionsregler styrs av besluten:

- **D3** — om kategori finns i UT men inte i IN: `BudgetAmount = 0` (default).
- **D4** — om kategori finns i IN men inte i UT: `ActualAmount = 0`, `Remaining = BudgetAmount`.
- ~~**D5** — januari 2014 saknas~~ RÄTTELSE: Januari finns för både 2014 och 2015.

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
4. **Belopp som `double` (två decimaler)** — matchar svensk redovisnings­konvention och Excel.
5. **En fil per dimension × år** — lätt att byta ut / utöka utan att röra allt.
6. **Indentation** — human readable och git-diff-vänligt (radbrytnings­diffar).
7. **Kultur­oberoende** — alla tal med punkt som decimalavskiljare, JSON enligt RFC 8259.

### 3.4 Exempel på `Facit/README.md`

```markdown
# Facit-data (utdragen ur pelles-budget-slim-2014-2015.xlsx)

## Ursprung
- Källa: `C:\Files\Dropbox\budget\bak\pelles-budget-slim-2014-2015.xlsx`
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
1. `sum(transactions.amount where Flag != "Ignore") per kategori per månad == expected-ut` (tolerans per D1)
2. `budget-in + expected-ut == expected-kvar` (per kategori per månad, unionsregler per D3/D4)
3. Transaktioner med `Flag == "Ignore"` räknas **inte** med i UT.
4. Antal transaktioner per år: 2014 = 809, 2015 = 845.
5. IN 2014 har 336 rader (28 kategorier × 12 månader inklusive januari).
6. IN 2015 har 336 rader (28 kategorier × 12 månader).
7. Transfers (`" -"`) ingår enligt D2 (default: egen fil, ej i `expected-ut`).
```

---

## 4. Integrationstest­plan

Tester placeras i befintliga `WebBankBudgeterUiTest`-projektet, och i ett
nytt **`WebBankBudgeterServiceTest/FacitIntegrationTests.cs`** för service-nivå.

### 4.1 Hjälpinfrastruktur

**`WebBankBudgeterUiTest/Facit/FacitLoader.cs`** (ny):

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

    public static List<BudgetKvarFacit> LoadExpectedKvar(int year) =>
        Load<List<BudgetKvarFacit>>($"expected-kvar-{year}.json");

    private static T Load<T>(string name) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(FacitDir, name)))!;
}

public record TransactionFacit(int Year, int Month, int Day,
    string Description, double Amount, string Category, string Flag);
public record BudgetInFacit(string Category, int Year, int Month,
    string MonthName, double BudgetAmount);
public record BudgetUtFacit(string Category, int Year, int Month,
    string MonthName, double ActualAmount);
public record BudgetKvarFacit(string Category, int Year, int Month,
    string MonthName, double BudgetAmount, double ActualAmount, double Remaining);
```

`.csproj`-tillägg (rätt syntax för SDK-style xUnit-projekt):

```xml
<ItemGroup>
  <None Update="Facit\**\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

(`<Content Update="...">` fungerar **inte** om filen inte redan är `Content`-item;
i SDK-style-projekt är JSON inte det per default.)

### 4.2 Tester på service-nivå (snabba, isolerade)

Fil: `WebBankBudgeterServiceTest/FacitBudgetTests.cs` (ny)

| Test | Verifierar |
|------|------------|
| `AggregationFromTransactions_MatchesExpectedUt_2014` | Mata `TableGetter.GetTextTableFromTransactions` med `transactions-2014.json` (filtrerade på `Flag != "Ignore"`, exkl. transfers per D2) → BudgetRows ska matcha `expected-ut-2014.json` (tolerans per D1) |
| `AggregationFromTransactions_MatchesExpectedUt_2015` | Samma för 2015 |
| `BudgetStructureBuilder_TotalRow_EqualsIncomeMinusExpenses` | Månad för månad: Budget-totalrad = Σ(+) + Σ(Ut), exkl. ` -`-förflyttningar (matchar `BudgetStructureBuilder.cs:28-29`) |
| `IgnoreFlag_IsExcludedFromAggregation` | Transaktion med `Flag == "Ignore"` får **inte** påverka summan |
| `KvarCalculation_InPlusUt_EqualsExpectedKvar` | `SnurraIgenom(budget-in-2014, expected-ut-2014) == expected-kvar-2014` (unionsregler per D3/D4) |
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
| `Facit_FileCounts_AreStable` | Antal rader i varje facit-fil är fasta (809 / 845 / 363 / 363 / …) — om extrakten regenereras och antal ändras slår testet |
| `FacitSum_TotalIn_2014_EqualsExpected` | Σ av alla `BudgetAmount` i `budget-in-2014.json` == 349 193,00 (från Excel-summa-raden) |

### 4.5 Toleranser

- Belopps­jämförelser: absolut­tolerans **0,01 kr** (kapabel att fånga ören men
  robust mot dubbelkonverteringar).
- Negativa belopp: både `expected-ut` och `remaining` får vara negativa → inga
  absolut­värden i asserts.

---

## 5. Implementations­plan — 6 milstolpar

### M0 — Stabil bygg/test-miljö + verifiera `TransactionHandler` (förkrav för M5)

`TransactionHandler` finns i kodbasen (`WebBankBudgeterService/TransactionHandler.cs`).
Det som blockerar repeterbar build är i stället fil­låsning när `WebBankBudgeterUi`
kör samtidigt (MSBuild `MSB3021`/`MSB3027` vid copy till `WebBankBudgeterUi/bin/...`).
Beslut **D6** styr fortsatt väg:

- **D6 = a** (default): Behåll nuvarande klass. Verifiera att den kompilerar och att
  `TransactionList.Account.AvailableAmount` + `TransactionList.Transactions`
  exponeras som `WebBankBudgeter.cs:222` förväntar, samt att inlästa data matchar facit.
- **D6 = b**: Skriv om mot facit-data först, sedan generalisera till `.xls` när M3 är grön.
- **D6 = c**: Inför ett `ITransactionSource`-interface; två impl: `FacitTransactionSource` (för
  tester) och `XlsTransactionSource` (för prod). `WebBankBudgeter` får interfacet via constructor.

Denna milstolpe blockerar inte M1–M4 (facit + tester körs utan att ladda riktiga `.xls`).
Måste vara klar innan M5.

### M1 — Skapa facit-mappen och extraktor-verktyget

**Filer:**

```
tools/FacitExtractor/
├── FacitExtractor.csproj        # net8.0, ClosedXML + System.Text.Json
└── Program.cs                   # Extraherar .xlsx → Facit/*.json
WebBankBudgeterUiTest/Facit/     # Generered output, commitas in
│   ├── README.md
│   ├── transactions-2014.json   (≈ 115 KB)
│   ├── transactions-2015.json   (≈ 126 KB)
│   ├── budget-in-2014.json      (≈ 52 KB)
│   ├── budget-in-2015.json      (≈ 52 KB)
│   ├── expected-ut-2014.json    (genereras)
│   ├── expected-ut-2015.json    (genereras)
│   ├── expected-kvar-2014.json  (genereras)
│   └── expected-kvar-2015.json  (genereras)
```

Extraktorn körs **en gång**, genererar JSON, och resultatet committas. Verktyget
behöver inte vara en del av bygget — bara körbart manuellt om facit behöver
uppdateras.

_Status:_ Prototyp finns i `C:\Users\nisse\AppData\Local\Temp\xlsx-reader\`.
Genererar korrekt `transactions-*.json` (809/845 poster) och `budget-in-*.json`
(363 poster per år). `expected-ut-*.json` genereras från transaktioner men
inkluderar transfers (` -`) — se D2. `budget-kvar-*.json` är 2 byte (tomma).

**M1 är inte klar förrän:**
1. `Flag`-kolumnen ("Regular"/"Ignore" från Excel kol 12) skrivs ut i `transactions-*.json`.
2. `expected-ut-*.json` följer beslut D2 (default: filtrera bort transfers).
3. `expected-transfers-*.json` produceras om D2 = c.
4. `expected-kvar-*.json` genereras enligt D3/D4/D5.
5. Extraktorn använder `decimal` om D1 = b.

### M2 — Facit-infrastruktur i test-projektet

Placering styrs av D11.

- **D11 = a**: `WebBankBudgeterUiTest/Facit/FacitLoader.cs` + records, dupliceras i
  `WebBankBudgeterServiceTest/Facit/`.
- **D11 = b** (default): Eget projekt `WebBankBudgeterTests.Facit/` med loader, records,
  och JSON-filer som `Content`/`None`-items. Båda testprojekten refererar projektet.

I bägge fallen:
1. Loader-funktioner per dimension (`LoadTransactions(year)` etc).
2. `.csproj` använder `<None Update="...">`-syntaxen (se 4.1).
3. JSON-deserialisering använder `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.

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

1. **Budget Total**: Ändra `FillTablesAsync` (`WebBankBudgeterUi.cs:55`) så IN-data
   (från `BudgetIns.json`) visas i Budget Total — inte bara utgift-härledda inkomster.
2. **Kvar**: TODO.md-arbetet är redan utfört (`BindKvarBudgetTableUi` finns på rad 230
   och anropas på rad 88). Antingen behåll så, eller — för riktig "kvar"-vy — återinför
   `SnurraIgenom`-flödet och skicka resultatet via `UtgiftsHanterareUiBinder` så
   formateringen blir enhetlig mellan flikarna.
3. **BudgetIns.json**: Generera en realistisk `BudgetIns.json` för 2014/2015 direkt ur
   facit. Format styrs av D9 (default: behåll befintligt `CategoryDescription`/
   `BudgetValue`/`YearAndMonth`-schema, transformera från `budget-in-YYYY.json`).
4. **Kategori-normalisering** (D7): Default-vägen är att uppdatera
   `TableGetter.GroupOnMonthAndCategory` (`Services/TableGetter.cs:42-55`) att använda
   `t.CategoryNameNoGroup` när `Group` är tom/null. Berör **inte** `Categories.ToString()`
   så övrig kod påverkas inte.
5. **Ignore-flagga**: Säkerställ att `Ignore`-markerade transaktioner filtreras bort
  där transaktioner laddas (exakt plats beror på D6).
6. **Månadskultur** (D10): Kontrollera att `Transaction.GetMonthAsFullString` använder
   `CultureInfo.InvariantCulture` när `MMMM` formatteras. Annars rätta.

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
