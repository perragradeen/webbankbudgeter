# Plan — återstående arbete

Målbild: UI ska matcha struktur och data mot Excel-facit `Pelles-budget-slim-2014-2015-gform.xlsx` (se arkiv för bakgrund, flikar, kolumnlayout).

**Arkiv:** `todo-history-arkiv.md` innehåller tidigare full `plan.md` (beslut D1–D16, Excel-analys, dataflöde, facit-schema, integrations­test­tabeller, M1–M5 i detalj, risker) och tidigare `todo.md`.

**Rutin:** Se **`AGENTS.md`**: textfacit endast via `ConsoleBudgeter --out`; efter verifierad leverans uppdatera vid behov denna fil, `todo.md`, `README.md`, `HISTORY.md`. För att **köra** `ConsoleBudgeter` på Linux utan installerad .NET-runtime på målmaskinen: självmantlad publicering (`ConsoleBudgeter/Properties/PublishProfiles/Linux-x64-SelfContained.pubxml`, skript `scripts/publish-console-budgeter-linux.sh`) — se `README.md`.

---

## Verifiering

### M0 — `TransactionHandler` och transaktionsantal

1. Bekräfta att `WebBankBudgeterService/TransactionHandler.cs` exponerar det som `WebBankBudgeterUi/WebBankBudgeter.cs` förväntar (`TransactionList.Transactions`, `TransactionList.Account.AvailableAmount` m.m. — exakta rader: läs koden).
2. **Facit-JSON (CI):** antal transaktioner per år enligt `WebBankBudgeterTests.Facit/Facit/README.md` (2014 = 809, 2015 = 845, summa 1654) täcks av `WebBankBudgeterServiceTest/FacitTransactionCountTests.cs`.
3. **Två olika filer i repot:**
   - `pelles budget.xls` (repo-root) är en **arbetskopia** med bl.a. **2018–2023** (ej 2014–2015). Där ska du inte förvänta dig ~1 654 rader för 2014+2015.
   - **`Pelles-budget-slim-2014-2015-gform.xlsx`** är **facit-källan** (samma som JSON under `WebBankBudgeterTests.Facit`). `TransactionHandler` ska ge **809** transaktioner 2014 och **845** 2015 — automatiserat i `WebBankBudgeterServiceTest/TransactionHandlerM0Tests.cs` (`M0_SlimGformXlsx_MatchesFacitTransactionCounts`).
4. **Riktig `.xls` som facit 2014–2015 kom från (manuellt / lokal maskin):** ladda via `TransactionHandler` och jämför volym/struktur med facit-JSON — kräver att filen finns på sökväg i `GeneralSettings.xml` (`TransactionTestFilePath`).
5. Vid gamla `.xls` med **tvåsiffrigt år** i kolumn (t.ex. 14→2014) normaliserar `BudgeterCore/Entities/KontoEntry.cs` året till 2000+ vid inläsning.
6. Vid `MSB3021`/`MSB3027` under full build: stäng körande WinForms eller bygg med `Budgetterarn.NoWindowsUi.slnf`.

---

## Implementation / tester (ej klara)

### M3 — Service (valfritt)

Utöka eller byt namn på tester så de speglar intent i arkiv **§4.2** (aggregation mot `expected-ut`, Kvar mot `expected-kvar`, m.m.). Dagens täckning ligger bl.a. i `ConsoleBudgeterTest`, `InBudgetMathSnurraIgenomTests`, övriga `WebBankBudgeterServiceTest`.

### M4 — WinForms

Implementera `WebBankBudgeterUiTest/BudgetIntegrationTests.cs` enligt arkiv **§4.3** (fejk-`WebBankBudgeter`, `FacitLoader`, grid-asserts). Kräver Windows Desktop SDK.

---

## Vid ändrad Excel eller extraktionsregler (M1)

När källa eller extraktionsregler ändras: kör `tools/FacitExtractor/`, granska diff, uppdatera JSON under `WebBankBudgeterTests.Facit/Facit/`, regenerera textfacit:

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- \
  --year 2014 --year 2015 --transactions 0 \
  --out WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt
```

Checklista för extraktorn och invariant­er: arkiv **§5 M1** och **§3.4**.

---

## Öppna gap (produkt / kvalitet)

Följ upp mot facit och verkliga banktexter:

- **G5** — `CategoryHandler` matchar hela beskrivningen; många kontoutdrag kräver rikare matchning (substring/alias).
- **G7** — säkerställ att produktionsinläsning ger samma volym/struktur som facit förväntar.
- **G2 / G3** — ev. tomma kvarceller för IN-only-kategorier; facit-jämförelse där `Group` inte är tom i XML.

---

## Valfri produkt­förbättring

- Grafisk inställning för in-källa (`InPosterSource`, `FacitBudgetInDirectory`) i stället för XML.
