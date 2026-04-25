# Projekt­historik — WebBankBudgeter

> **Syfte:** Dokumentera **vad som spelar roll idag** (beslut, buggar, verktyg, Linux) och **bakgrund** (hur vi kom dit).  
> **Underhåll:** Lägg nya upptäckter under *Aktuellt — viktigt att komma ihåg*. Flytta äldre sessionshit till *Arkiv* när de mest är tidsstämplar och agent-ID:n.

---

## Del A — Aktuellt (viktigt att komma ihåg)

### Facit och sanning

- **Facit-JSON och referenstext** (`WebBankBudgeterTests.Facit/Facit/`, `console-report-facit-reference.txt`) är **sanning**: koden och testerna ska anpassas till dem, inte tvärtom.
- **`ConsoleBudgeter`** kör samma kedja som WinForms för tabell­logik: `WebBankBudgeterService` (`FacitBudgetTextTableFactory`, `BudgetStructureBuilder`, …) + `InbudgetHandler` (`BudgetTableInMerger`, `KvarTextTableBuilder`, `InBudgetMath`). Endast **textrendering** ligger i konsolprojektet.
- **Inkomstkategori i budgetstruktur:** endast **exakt** kategorinamn `"+"` räknas som inkomst — **inte** `Contains("+")` (annars hamnar t.ex. `värnamoresor+övriga` fel).
- **Kvar:** `IN + UT` per kategori/månad; `KvarTextTableBuilder` använder **alla** platta `BudgetRow` *före* IN-merge (union som `expected-kvar`). Raden **"-"** (placeholder) visas **inte** i Kvar-vyn.
- **`BudgetIns.json`** kan fyllas från facit: `tools/FacitBudgetInsExport` → samma budget som `budget-in-*.json` i UI-format (672 rader = 336×2 år när båda åren exporteras).

### Linux, CI och bygge

- **`WebBankBudgeterUi`** och **`WebBankBudgeterUiTest`** kräver **Windows Desktop SDK** (`net8.0-windows`) — de bygger **inte** på ren Linux-SDK.
- **`Budgetterarn.NoWindowsUi.slnf`:** bygg/test **utan** WinForms-projekt → CI och Linux utan `Microsoft.NET.Sdk.WindowsDesktop`. Kommandon: `dotnet build Budgetterarn.NoWindowsUi.slnf`, `dotnet test Budgetterarn.NoWindowsUi.slnf`. **Uppdatera filtret** om nya `net8.0-windows`-projekt läggs till i lösningen.
- **MSB3021/MSB3027:** uppstår när en **körande** WinForms-app låser DLL:er under `dotnet build` på hela `Budgetterarn.sln` — stäng appen eller använd `.slnf` för headless-bygge.
- **`.NET SDK`:** `dotnet-sdk-8.0` behövs för bygge/test (t.ex. `apt`-installation i headless-miljöer).

### Transaktionsfilter (år)

- **`TransFilterer.FilterTransactions(list, selectedYear)`** kräver `DateAsDate.Year == selectedYear` utöver 1 jan–31 dec, så t.ex. **december 2013** inte följer med vid filter **2014** (plan R5).

### WinForms — var IN kommer ifrån

- **`GeneralSettings.xml`:** `InPosterSource` = `BudgetIns` (standard, `TestData/BudgetIns.json`) eller `FacitJson` (`Facit/budget-in-{år}.json` under `FacitBudgetInDirectory`). Vid facit-källa är **Spara in-poster** inaktiverat (meddelande till användaren).
- Projektet kan kopiera `budget-in-2014.json` / `2015.json` till output under `Facit/` (se `WebBankBudgeterUi.csproj`).

### Tekniska detaljer som återkommer

- **Månadsnycklar** i tabeller: `YYYY MMMM` med **`InvariantCulture`** (engelska månadsnamn) så facit och kod matchar oberoende av trådkultur.
- **Visning av belopp i UI:** `sv-SE` och format enligt plan (t.ex. `# ##0` / `N0` där det är implementerat).
- **`ColumnHeaders`** på `TextToTableOutPuter` är **read-only** — använd `ColumnHeaders.AddRange`, inte tilldelning till propertyn.
- **`BudgetRow.AmountsForMonth`** är get-only dictionary — initiera värden med `row.AmountsForMonth[key] = …`, inte objektinitierare som sätter hela dictionaryt.
- **Cirkulär referens:** `BudgetTableInMerger` ligger i **`InbudgetHandler`** (inte i `WebBankBudgeterService`), för att undvika cykel `Service` ↔ `InbudgetHandler`.

### Plan vs. repo

- **`plan.md` avsnitt M1** kan fortfarande beskriva **äldre** extraktor-prototyp (antal rader, januari) — **verifiera alltid mot committad facit** och `Facit/README.md` innan du litar på M1-texten ordagrant.
- **`plan.md` §6 (risker)** har uppdaterats med mitigeringar (R1 `.slnf`, R5 årsfilter, R6 utfasad Kvar).

### Tester som är “källan till sanning” på Linux

- `ConsoleBudgeterTest` — snapshot av full rapport, facit-aggregation.
- `WebBankBudgeterServiceTest` — bl.a. `TableGetter`, `BudgetStructureBuilder`, `BudgetTableInMerger`, `InBudgetMath` / `SnurraIgenom` mot facit, `TransFilterer`, `FacitBudgetInLoader`.

---

## Del B — Arkiv (bakgrund — mindre relevant för dagens kod)

> Här ligger **process**, gamla branch-namn, agent-ID:n, tidiga iterationsfel och metrics. Läs om du undrar *hur* vi kom fram hit — inte för att veta *vad* som gäller nu.

### Session 2026-04-24 — Multi-agent, ConsoleBudgeter, facit M1/M2

- **Branch då:** `feature/facit-implementation` (nutida arbete kan ligga på `cursor/*`-grenar).
- **Multi-agent:** tre parallella `explore`-agenter (service / UI / test) gav snabb kartläggning; lärdom: en agent per subsystem med tydlig prompt.
- **ConsoleBudgeter:** skapades för att testa UI-liknande output på Linux; `sv-SE` i rendering; snapshot-normalisering CRLF/LF.
- **Encoding-historik:** tidiga ISO-8859-1-problem i vissa filer → konvertering till UTF-8; **nuvarande UI-filer** kan fortfarande vara **blandad** kodning (se `README.md` — Latin-1 i vissa WinForms-filer).
- **FacitExtractor:** först ClosedXML (bara `.xlsx`) → **ExcelDataReader** för `.xls`/`.xlsx` + CodePages för svenska tecken. Stora zip-uppladdningar via webb misslyckades — **Git push** av filer är mer pålitligt.
- **Extraktor-bugg (fixad):** fel kolumn för kategori i `ExtractBudgetIn` → gav `"category": "-11506.74"`; rättades till kolumn 1 + månader 6–17.
- **Plan D5 / januari:** plan påstod först att januari 2014 saknades — **verifiering visade att januari finns**; plan och invariant (28×12 = 336 rader per år) korrigerades.
- **M2:** `WebBankBudgeterTests.Facit` + `FacitLoader` + records + JSON kopieras till output.
- **Gamla “Nästa steg” i denna fil** nämnde M0/M3/M4/M5 som pending — **status har ändrats** (mycket av M5 och delar av facit-kedjan är implementerat); se `plan.md` och `todo.md` för aktuell kö.

### Agent-ID:n (endast spårbarhet i gamla loggar)

- Service: `2ed4e278-fbe0-44f8-91be-956b7cb74253`
- UI: `54220857-98f4-4a03-98e2-24b2d78f16e2`
- Test: `087b2bcc-7ec8-41d3-a633-25b5936e7d7b`

### Äldre metrics (ungefärliga — repo har växt)

- Tidiga commits nämnde t.ex. `148fff2` encoding, `7bb28a2` M1, `eba1bab` M2 — använd `git log` för exakt historia.
- “24/24 tester på Linux” i gamla text avser **dåvarande** uppsättning; kör `dotnet test Budgetterarn.NoWindowsUi.slnf` för aktuellt tal.

### Övriga gamla “problem och lösningar”

- Windows-sökvägar i test → `Path.Combine`.
- `GeneralSettingsHandler`: normalisera `\` till `Path.DirectorySeparatorChar`.
- Tusentalsavskiljare i tester → explicit `sv-SE` där det behövs.

---

## Arkivering (valfri framtida rutin)

Om filen blir för lång: flytta **Del B** till `HISTORY_ARCHIVE.md` och behåll **Del A** + senaste session i `HISTORY.md`. Det finns ännu ingen `HISTORY_ARCHIVE.md` i repo — skapa vid första arkivering.

---

*Senast uppdaterad: 2026-04-24 (omstrukturering + sammanslagning av senare upptäckter).*
