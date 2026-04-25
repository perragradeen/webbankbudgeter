# Plan: Efterlikna `Pelles-budget-slim-2014-2015-gform.xlsx` i WebBankBudgeter

Facit: `C:\Files\Dropbox\budget\Program\webbankbudgeter\Pelles-budget-slim-2014-2015-gform.xlsx`

Målet är att UI:t ska visa exakt samma struktur och data som Excel-förlagan:

- **Inkomster** (budget) i egen sektion, per kategori och månad
- **Utgifter** (utfall) kategoriserade och inlagda i respektive år och månad
- **Kvar per månad** = `IN + UT` per kategori per år/månad
  (UT är negativt → positivt resultat = under budget)

### Underhåll: `plan.md`, `todo.md` och `README.md`

Efter att något **byggts, testats och verifierats** (lokalt eller i CI): uppdatera **`plan.md`** (milstolpar, risker, gap) och **`todo.md`** (öppna punkter, klart, väntar verifiering). **`README.md`** ska nämna denna rutin och peka på plan/todo. Syftet är en enda levande bild av läget — se även README under *Dokumentation och milstolpar*.

---

## 0. Beslut (loggade mot nuvarande kodbas)

Dessa val styr facit-format, tester och normalisering. Kolumnen **Beslut** speglar vad som gäller i repot *nu*; ändra vid behov om ni byter strategi.

| # | Område | Alternativ | Default-förslag | Beslut |
|---|--------|-----------|-----------------|--------|
| D1 | **Beloppsprecision** i extraktor och tester | (a) `double` med tolerans ±0,01 per cell och ±0,10 för aggregat · (b) `decimal` hela vägen · (c) `double` men tolerans ±0,01 överallt (kan failas av summor) | (a) | **(a)** — koden använder `double`; asserts med liten absolut tolerans vid facit-jämförelse |
| D2 | **Transfers (`" -"`)** i `expected-ut-YYYY.json` | (a) Inkluderas · (b) Exkluderas helt · (c) Egen fil `expected-transfers-YYYY.json` | (c) | **(c)** — när facit/JSON finns: `expected-ut` utan transfers; `BudgetStructureBuilder` separerar `" -"` |
| D3 | **`expected-kvar` när IN saknas** (UT-only) | (a) `BudgetAmount = 0`, raden inkluderas · (b) Hoppa raden · (c) `BudgetAmount = null` | (a) | **(a)** |
| D4 | **`expected-kvar` när UT saknas** (IN-only) | (a) `ActualAmount = 0`, `Remaining = BudgetAmount` · (b) Hoppa raden | (a) | **(a)** |
| D5 | **Januari 2014 i IN** (saknas i Excel) | (a) Tomt — dokumentera · (b) Nollrader · (c) Februari som proxy | (a) | **(a)** |
| D6 | **`TransactionHandler`** | (a) Git-historik · (b) Skriv om · (c) `ITransactionSource` | (a) | **(a)** — `WebBankBudgeterService/TransactionHandler.cs` finns i repot |
| D7 | **Kategori-nyckel vid tom grupp** | (a)–(c) se tabell | (b) | **Ej implementerat** — `TableGetter` grupperar fortfarande på `CategoryName` (full `"Group Name"`). `CategoryNameNoGroup` finns för framtida byte |
| D8 | **Sparrader** | (a) Utgifter · (b) Egen sektion | (a) | **(a)** |
| D9 | **`BudgetIns.json`** | (a) Befintligt schema från facit · (b) Migrera till `budget-in` | (a) | **(a)** — format oförändrat; fyllning/val av källa = produkt/todo |
| D10 | **Kultur i `GetMonthAsFullString`** | (a) Verifiera · (b) Tvinga invariant | (a) | **(a)** — redan `CultureInfo.InvariantCulture` i `Transaction.GetMonthAsFullString` |
| D11 | **Facit-mappens placering** | (a) UiTest · (b) Shared `WebBankBudgeterTests.Facit` | (b) | **Ej i denna kopia** — inga committade facit-JSON än; välj (a) eller (b) när facit läggs in |
| D12 | **`Ignore` i facit** | (a) I `transactions` med `Flag`, bort från `expected-ut` · (b) Filtrera i extraktor | (a) | **(a)** — mål när pipeline finns; produktfilter enligt `EntryType`/motsvarande utreds per implementation |
| D13 | **Assert-tolerans** | (a)–(c) | (b) | **(a)** — MSTest utan FluentAssertions i service-testprojektet |
| D14 | **Sortering kod vs facit** | (a) Kod som facit · (b) Dictionary i tester | (b) | **(b)** |

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

**Observation om IN 2014**: I `Budget (2014)` finns inga IN-rader för **januari** —
första månadskolumnen med data är februari. Facit ärver denna lucka (33 kategorier
× 11 månader = 363 rader). Se beslut D5 ovan för hur januari ska representeras i
`expected-kvar`.

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
Excel (`Pelles Budget.xls` eller motsvarande sökväg i inställningar)
  └─► TransactionHandler (WebBankBudgeterService)
        └─► Transaction { DateAsDate, Description, AmountAsDouble,
                          Categorizations.Categories[0].{Group, Name} }
              └─► CategoryName = "Group Name" (Categories.ToString)
                    └─► TableGetter.GroupOnMonthAndCategory(...)  [nyckel = CategoryName idag]
                          └─► BudgetRowFactory → AmountsForMonth["2014 January"]
                                └─► TextToTableOutPuter { BudgetRows, ColumnHeaders }
                                      └─► BudgetStructureBuilder → strukturerad vy
                                            └─► UtgiftsHanterareUiBinder → gv_budget
```

`InBudget` / inkomst-grid:

```
TestData/BudgetIns.json (WebBankBudgeterUi)
  └─► InBudgetHandler → List<Rad> …
        └─► InBudgetUiHandler → gv_incomes
```

**Kvar** (`gv_Kvar`): i `WebBankBudgeterUi.FillTablesAsync` anropas `BindKvarBudgetTableUi(table)` — samma `TextToTableOutPuter` som Budget Total (transaktionsbaserad tabell), **inte** `VisaKvarRader_BindInPosterRaderTillUiAsync` (som använder `SnurraIgenom` med IN-poster). För facit-korrekt **IN+UT** i Kvar krävs att den senare kedjan kopplas in igen eller motsvarande logik delas med rapport/console — se `todo.md`.

### 2.2 Viktiga detaljer i nuvarande kod

- **Månadsnyckel**: `Transaction.GetYearMonthName` / `GetMonthAsFullString` använder **InvariantCulture** för månadsnamn (`"2014 January"`).
- **Kategori­nyckel i tabell**: `BudgetRow.CategoryText` följer transaktionens `CategoryName` (grupp + namn) så länge D7 inte är implementerat.
- **Klassificering** i `BudgetStructureBuilder`:
  - Inkomst: kategori **trimmat lika med** `"+"` (inte `Contains`, så t.ex. `värnamoresor+övriga` räknas som utgift).
  - Förflyttning: `CategoryText.Contains(" -")` (mellanslag före minus).
  - Utgift: övrigt.
- **`TransFilterer.FilterTransactions(list, year)`**: efter datumintervall filtreras rader så **`DateAsDate.Year == year`** (R5 — inget läckage från grannår).

### 2.3 Gap mellan facit och nuvarande kod

När committad facit-JSON (`transactions-*`, `budget-in-*`, `expected-*`) och extraktor finns ska tabellen nedan fyllas i igen. **Nu:** ingen facit-mapp i denna arbetskopia — punkterna beskriver kvarvarande **produkt**-gap mot målbilden.

| # | Område | Facit / mål | Nuvarande kod | Gap |
|---|--------|-------------|---------------|-----|
| G1 | Budget Total + IN | IN per kategori i samma vy som UT | UT från transaktioner; IN i separat grid (`gv_incomes`) från `BudgetIns.json` | Sammanföra enligt mål när facit driver implementation |
| G2 | Kvar | `IN + UT` per kategori | `gv_Kvar` = samma tabell som Budget Total (`BindKvarBudgetTableUi`) | Anropa IN+UT-bindning (`VisaKvarRader_…` / delad `SnurraIgenom`-väg) eller motsvarande |
| G3 | Kategori-nyckel | t.ex. `"el"` mot facit | Gruppering på `CategoryName` | **D7** ej implementerad — använd `CategoryNameNoGroup` där det behövs |
| G4 | Tecken-konvention | IN ≥ 0, UT ≤ 0, KVAR = IN + UT | `SnurraIgenom` i `WebBankBudgeter` | OK för den vägen; UI använder den inte för `gv_Kvar` idag |
| G5 | Auto-kategorisering | Fri text i kontoutdrag | `CategoryHandler` / exakt matchning | Förbättringar vid behov mot riktig fil |
| G6 | `BudgetIns.json` | Full täckning för testår | Testdata begränsad | Fyll/sync mot facit när JSON finns |
| G7 | Transaktionsantal | ~1 654 rader 2014+2015 | Verifiera mot riktig `.xls` / prod-fil | **M0** — manuell räkning |
| G8 | `Ignore` | Filtreras från UT-aggregation | Verifiera i `TransactionTransformer` / tabellväg | Bekräfta mot facit-tester när de finns |
| G9 | Månadskultur | Invariant engelska månader | Implementerat | **Klart** |

---

## 3. Facit-format (AI-läsbart)

**Status i denna kopia:** ingen committad facit-mapp än — avsnittet är **målformat** när ni lägger in JSON.

Facit läggs som **UTF-8 JSON med indentation** under valt rot (beslut **D11**), t.ex. `WebBankBudgeterTests.Facit/Facit/` eller `WebBankBudgeterUiTest/Facit/`. En fil per år är enklare att diffa.

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
- **D5** — januari 2014 saknas helt i IN; raden för januari får `BudgetAmount = 0` om
  motsvarande UT finns för den månaden.

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
1. `sum(transactions.amount where Flag != "Ignore") per kategori per månad == expected-ut` (tolerans per D1)
2. `budget-in + expected-ut == expected-kvar` (per kategori per månad, unionsregler per D3/D4)
3. Transaktioner med `Flag == "Ignore"` räknas **inte** med i UT.
4. Antal transaktioner per år: 2014 = 809, 2015 = 845.
5. IN 2014 har 363 rader (33 kategorier × 11 månader; januari saknas, se D5).
6. IN 2015 har 363 rader.
7. Transfers (`" -"`) ingår enligt D2 (default: egen fil, ej i `expected-ut`).
```

---

## 4. Integrationstest­plan

**Nu:** `WebBankBudgeterServiceTest` har enhets-/integrationstester utan facit-JSON. När facit finns: lägg `FacitLoader` + kopiera JSON till output enligt D11, och inför `FacitBudgetTests.cs` (eller motsvarande namn) enligt tabellen nedan.

Tester kan även läggas i `WebBankBudgeterUiTest` för UI-nära scenarion.

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

### M0 — `TransactionHandler` och riktig Excel

**Kod:** `TransactionHandler` finns i `WebBankBudgeterService/TransactionHandler.cs` och används från `WebBankBudgeter`.

**Kvar:** Verifiera mot **riktig** `Pelles Budget.xls` (eller motsvarande) att antal transaktioner och saldo stämmer med förväntat (~**1 654** rader för 2014+2015 enligt Excel-analysen i §1.1). Dokumentera resultat i `todo.md` när det är gjort.

### M1 — Facit-mapp och extraktor

**Status:** Ej påbörjad i denna kopia (inga `tools/FacitExtractor`, inga JSON-filer).

**Mål:** Verktyg som läser `Pelles-budget-slim-2014-2015-gform.xlsx` och skriver filer enligt §3. JSON committas; extraktorn körs manuellt vid behov.

**M1-checklista (när verktyget finns):**
1. `Flag` ("Regular"/"Ignore") i `transactions-*.json`.
2. `expected-ut-*.json` enligt **D2** (utan transfers om D2 = c).
3. `expected-transfers-*.json` om D2 = c.
4. `expected-kvar-*.json` enligt D3/D4/D5.
5. Om **D1 = b**: använd `decimal` i extraktor; annars följ **D1 = a** i tester.

### M2 — Facit-infrastruktur i test-projektet

**Status:** Väntar på M1 + val av D11.

Skapa `FacitLoader` + records och kopiera JSON till test-output (`<None Update="Facit\**\*.json">` … `CopyToOutputDirectory`), se §4.1.

### M3 — Service-tester mot facit

**Status:** Väntar på M2.

Implementera tester enligt §4.2 (`FacitBudgetTests.cs` eller liknande).

### M4 — UI-integrations­tester

**Status:** Kräver **Windows** / `net8.0-windows` och WinForms-testhost — lämpligen utanför ren Linux-CI.

Implementera `BudgetIntegrationTests.cs` enligt §4.3 med förladdad facit-data. Mönster:

```csharp
var table = BuildTableFromFacit(transactions: FacitLoader.LoadTransactions(2014));
var grid = new DataGridView();
var binder = new UtgiftsHanterareUiBinder(grid);
binder.BindToBudgetTableUi(table);
AssertGridMatchesExpectedUt(grid, FacitLoader.LoadExpectedUt(2014));
```

### M5 — Driv in koden mot facit

Förkrav: M2–M3 (minst) ger mätbart facit.

| # | Punkt | Status i denna kopia |
|---|--------|----------------------|
| 1 | IN + UT i Budget Total / samma vy som mål | **Öppen** — separata flöden idag |
| 2 | Kvar = IN+UT i UI | **Öppen** — `gv_Kvar` duplicerar transaktionstabellen; `VisaKvarRader_BindInPosterRaderTillUiAsync` finns men anropas inte |
| 3 | `BudgetIns.json` / täckning från facit | **Öppen** — väntar M1 |
| 4 | D7 `CategoryNameNoGroup` i `TableGetter` | **Öppen** |
| 5 | `Ignore` filtreras i tabellpipeline | **Verifiera** mot kod + M3 när facit finns |
| 6 | D10 månadsnamn | **Klart** (invariant) |
| 7 | Inkomstklassificering exakt `"+"` | **Klart** — `BudgetStructureBuilder` |
| 8 | Årsfilter R5 | **Klart** — `TransFilterer.FilterTransactions(..., year)` |

---

## 6. Risker och oklarheter

| # | Risk | Mitigering / status |
|---|------|---------------------|
| R1 | WinForms / `WebBankBudgeterUi` bygger inte på ren Linux-CI utan Windows SDK | Bygg/testa UI på Windows; dokumentera i README |
| R2 | UT/KVAR är `#VALUE!` i Excel-filen | Accepterat — appen räknar från transaktioner + IN |
| R3 | Specialtecken i kategorinamn | UTF-8 i källfiler/JSON; se README om blandad kodning i äldre UI-filer |
| R4 | Svensk kultur i Excel vs invariant i kod | Extraktor skriver JSON med punktdecimal; koden använder invariant för månadsnyckel |
| R5 | Transaktioner nära årsskifte (t.ex. dec 2013 i 2014-fil) | **`TransFilterer.FilterTransactions(list, selectedYear)`** kräver `DateAsDate.Year == selectedYear` efter datumintervall |
| R6 | `gv_Kvar` visar samma data som Budget Total | **Känd produktgap** — åtgärd M5 punkt 2 / `todo.md` |
| R7 | Flyttal vid stora summor (D1) | Använd tolerans (D1 = a) i facit-tester |
| R8 | Sortering kod vs facit | D14 (b): jämför som dictionary/set i tester |

---

## 7. Leverans­ordning (rekommenderad)

0. Håll **beslut D1–D14** uppdaterade i sektion 0 när ni ändrar strategi.
1. **M1** → **M2** → **M3** (facit + automatiserade service-tester).
2. **M0** (verifiering mot riktig Excel) kan köras parallellt när fil finns.
3. **M5** drivs av gröna M3-tester; **M4** på Windows när tid finns.
4. Efter varje leverans: uppdatera **`plan.md`**, **`todo.md`**, vid behov **`README.md`** (se rubrik överst).

Varje milstolpe bör vara committable med **gröna tester** för den plattform som målet gäller (Linux: service-testprojekt; Windows: + UI).
