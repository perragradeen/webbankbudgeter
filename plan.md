# Plan — återstående arbete

Målbild: UI ska matcha struktur och data mot Excel-facit `Pelles-budget-slim-2014-2015-gform.xlsx` (bakgrund och gamla beslut: **`todo-history-arkiv.md`**).

**Arkiv:** Där ligger fryst lång `plan`/`todo`, beslut D1–D16 — och **avklarade checklistor** som flyttats hit från denna fil (sök t.ex. *«M0 verifiering»*, *«In Ut Kvar»*).

**Rutin:** `AGENTS.md` (textfacit via `ConsoleBudgeter --out`). När arbete är klart: **arkivera** enligt `README.md` → **Nyckelord: plan-arkiv**.

---

## M0 — `TransactionHandler` (status)

**Klart (automatiskt + dokumenterat):** antal enligt facit-JSON (`FacitTransactionCountTests`); slim `.gform.xlsx` **2014** 809 / **2015** 845 (`TransactionHandlerM0Tests`); `KontoEntry` +2000 för tvåsiffrigt år; relativa transaktionsvägar (`ResolvedTransactionFilePath`). **Utökad punktlista** (två filtyper, manuell `.xls`, bygglås): **`todo-history-arkiv.md` → avsnitt *M0 verifiering — flyttad från plan***.

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
