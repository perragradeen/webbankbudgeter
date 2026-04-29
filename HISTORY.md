# Projekt­historik — WebBankBudgeter

> **Syfte:** Dokumentera **vad som spelar roll idag** (beslut, buggar, verktyg, Linux) och korta **sessionsfakta**.  
> **Underhåll:** Lägg nya upptäckter under *Del A*. Långa bakgrundsposter ligger i **`HISTORY_ARCHIVE.md`**.

## 2026-04-29 — M0: `TransactionHandler` + två Excel-källor

**Syfte:** Verifiera inläsning mot `pelles budget.xls` och mot facit-filen `Pelles-budget-slim-2014-2015-gform.xlsx`.

**Utfall:** Repo-roten `pelles budget.xls` innehåller **2018–2023** (arbetskopia), inte 2014–2015. Facit-antalen **809 + 845** valideras mot **`.gform.xlsx`** i `TransactionHandlerM0Tests`. I `KontoEntry` för XLS: år **1–99** normaliseras med **+2000** så tvåsiffriga år inte blir år 14/15 e.Kr.

---

**Syfte:** `dotnet test Budgetterarn.NoWindowsUi.slnf` grönt. Uppdaterade `ConsoleBudgeterTest/Snapshots/report-2014.txt` och `report-2015.txt` till aktuell rapport (radordning `" -"` i budgettabell, borttagen förflyttnings-summeringsblock). `SnapshotTests.Normalize` strippar UTF-8 BOM. `GetAllVisibleEntriesFromWebBrowserTests.Check_If_SomeThingLoaded_Privata` sökte med `DateTime.Today.ToShortDateString()` som nyckel — stämmer inte med `KontoEntry.KeyForThis` (`yyyy-MM-dd`); bytt till uppslag via `Info`.

---

## 2026-04-27 — `plan.md` / `todo.md` rensade; snapshot i `todo-history-arkiv.md`

**Syfte:** Lämna endast **återstående** arbete i `plan.md` och `todo.md`. All tidigare full plan (beslut D1–D16, Excel-analys, dataflöde, facit-schema, M1–M5 i detalj, risker) och den dåvarande `todo.md` ligger fryst i **`todo-history-arkiv.md`**. `README.md` och `HISTORY_ARCHIVE.md` pekar på arkivet där längre text behövs.

---

## 2026-04-26 — Dokumentstäd: `plan` / `todo` vs genomförd kod (gren `ai`)

**Syfte:** Ta bort dubbletter där samma arbete stod som både “klart i kod” och “väntar verifiering” i `todo.md`, synka `plan.md` (dataflöde §2.1–2.3, M1-status, G6) med repot, och flytta arkivdelen till `HISTORY_ARCHIVE.md` så `HISTORY.md` bara bär aktuellt innehåll.

---

## 2026-04-26 — Gren `ai`: sammanslagning av remote-grenar från `master`

**Syfte:** En arbetsgren `ai` skapad från `master` där relevanta `origin/*`-grenar mergats in så att en enda gren bär samlad kod och dokumentation. Inkluderat: `cursor/agents-history-console-facit-d200`, `cursor/console-budgeter-app-1a34` (redan up to date), `cursor/m5-kvar-snurra-budgetins-23ef`, `cursor/mitigate-plan-risks-23ef`, `cursor/readme-multi-agent-a7a6`, `cursor/plan-todo-readme-c93a`, `cursor/m0-m5-plan-facit-a56f` (**tom merge** `-s ours` — innehåll fanns redan i `ai`; undvek massiva add/add-konflikter i facit-JSON), `feature/facit-implementation` (redan up to date), dependabot-grenar för CefSharp (behöll **140.1.140** på `HEAD`) respektive `System.Text.Encodings.Web` (behöll borttagen `packages.config` i `SwedbankSharp-master`). Första innehållsmerge (`agents-history…`) gav konflikter mot `master`; lösningen prioriterade `master`-konsolflödet (`BudgetReportBuilder` + `WebBankBudgeterTests.Facit`), behöll `TransFilterer`-förbättring, tog bort duplicerad `InBudgetKvarCalculator` (samma logik som `InBudgetMath`), och skärpte `BudgetStructureBuilder` så förflyttning matchar exakt trimmat `" -"`.

---

## 2026-04-26 — `master` synkad med `origin/cursor/console-budgeter-app-1a34`

**Git-fakta (denna klon, innan merge):** `master` låg på `5e1c121` (Excel + plan/README). `origin/cursor/console-budgeter-app-1a34` hade **29 commits** ovanför gemensam ancestor `90a5331` och bar därmed `ConsoleBudgeter`, `WebBankBudgeterTests.Facit`, `InbudgetHandler`-delar (Kvar/IN-merge), m.m. Det förklarar “gren på gren” i `git log --graph`: en lång serie `cursor/`-commits på samma funktionella kedja, medan `master` hade en avstickare (`5e1c121`) som inte fanns på console-grenen. **Åtgärd:** merge av `origin/cursor/console-budgeter-app-1a34` in i `master` (ingen ny `cursor/…`-gren för just denna leverans).

**Användarens krav:** `AGENTS.md` med bindande regler (facit = `ConsoleBudgeter --out`, ingen Python-sidospår utan beslut, facit ändras bara vid ny källa/regel, uppdatera plan/todo/README/HISTORY efter verifierad build, läs faktisk repo — inte gissa “okända” grenar). Textfacit-fil i repot heter **`WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt`** (samma innehåll som tidigare `console-report-facit-reference.txt`; gamla namnet borttaget för tydlighet).

**Varför “tjatas” om TransactionHandler i äldre svar:** en äldre version av `plan.md` på `master` påstod att klassen saknades trots att den alltid funnits under `WebBankBudgeterService/TransactionHandler.cs` — det var **dokumentationsfel**, inte kodfel. Efter merge stämmer `plan.md` med verkligheten; `AGENTS.md` påminner om att inte upprepa myten.

---

## Del A — Aktuellt (viktigt att komma ihåg)

### Facit och sanning

- **Facit-JSON och referenstext** (`WebBankBudgeterTests.Facit/Facit/`, `facit-2014-2015.txt`) är **sanning**: koden och testerna ska anpassas till dem, inte tvärtom.
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

- **Regenerering av facit:** checklista och invariant­er finns i **`todo-history-arkiv.md`** (tidigare `plan.md` §5 M1, §3.4); kort pekare i nuvarande **`plan.md`**. **Faktiska filantal** i committad facit: `WebBankBudgeterTests.Facit/Facit/README.md`.
- **Tidigare risktabell (R1–R8)** ligger i arkivet; mitigeringar (`.slnf`, årsfilter i `TransFilterer`, Kvar-kedja) finns kvar i kod och tester.

### Tester som är “källan till sanning” på Linux

- `ConsoleBudgeterTest` — snapshot av full rapport, facit-aggregation.
- `WebBankBudgeterServiceTest` — bl.a. `TableGetter`, `BudgetStructureBuilder`, `BudgetTableInMerger`, `InBudgetMath` / `SnurraIgenom` mot facit, `TransFilterer`, `FacitBudgetInLoader`.

---

*Senast uppdaterad: 2026-04-28 (testfix snapshots + Swedbank HTML-test).*
