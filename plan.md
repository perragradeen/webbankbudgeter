# Plan — återstående arbete

Målbild: UI ska matcha struktur och data mot Excel-facit `Pelles-budget-slim-2014-2015-gform.xlsx` (se arkiv för bakgrund, flikar, kolumnlayout).

**Arkiv:** `todo-history-arkiv.md` innehåller tidigare full `plan.md` (beslut D1–D16, Excel-analys, dataflöde, facit-schema, integrations­test­tabeller, M1–M5 i detalj, risker) och tidigare `todo.md`.

**Rutin:** `AGENTS.md` — textfacit endast via `ConsoleBudgeter --out`; efter verifierad leverans uppdatera vid behov denna fil, `todo.md`, `README.md`, `HISTORY.md`.

---

## Verifiering

### M0 — `TransactionHandler` och transaktionsantal

1. Bekräfta att `WebBankBudgeterService/TransactionHandler.cs` exponerar det som `WebBankBudgeterUi/WebBankBudgeter.cs` förväntar (`TransactionList.Transactions`, `TransactionList.Account.AvailableAmount` m.m. — exakta rader: läs koden).
2. Läs in avsedd källfil (t.ex. `Pelles Budget.xls` / motsvarande) och verifiera **~1 654** transaktioner över 2014+2015 enligt facitunderlag.
3. Vid `MSB3021`/`MSB3027` under full build: stäng körande WinForms eller bygg med `Budgetterarn.NoWindowsUi.slnf`.

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
