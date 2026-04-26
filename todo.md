# TODO (levande lista)

Uppdatera denna fil när något **byggts, testats och verifierats** (samma rutin som `AGENTS.md` / `README.md`).

## Klart i kod (denna arbetskopia)

- **`TransactionHandler`:** finns i `WebBankBudgeterService/TransactionHandler.cs` (planens gamla “saknas” är inaktuellt — se `plan.md` D6/M0).
- **`BudgetStructureBuilder`:** inkomst/förflyttning klassas med **exakt** trimmat `"+"` resp. `" -"`.
- **`TransFilterer`:** vid filter på ett helt kalenderår krävs `DateAsDate.Year == valt år` (plan R5).
- **`ConsoleBudgeter`:** konsolprojekt som kan skriva textfacit med `--out` (se `README.md`).
- **`InBudgetKvarCalculator.SnurraIgenom`:** delad implementation i `InbudgetHandler` (används från `WebBankBudgeter.SnurraIgenom`).

## Öppet / nästa

- **`gv_Kvar` i WinForms:** `FillTablesAsync` använder `BindKvarBudgetTableUi` (samma data som Budget Total). Koppla in `VisaKvarRader_BindInPosterRaderTillUiAsync` om fliken ska visa **IN+UT-kvar** enligt `SnurraIgenom`.
- **Facit JSON + extraktor:** enligt `plan.md` M1–M3 (filer under t.ex. shared test-projekt) — **inte** implementerat i denna clone utöver Excel i roten + plan.
- **Textfacit-fil:** generera med `ConsoleBudgeter` och committa när innehållet är granskat (`Facit/facit-2014-2015-console.txt` eller enad sökväg).

---

## Arkiv: gammal fyra-stegs-plan (Kvar = kopia av Budget Total)

Den gamla `todo.md` beskrev medvetet att låta `gv_Kvar` använda samma bindning som `gv_budget`. Det är fortfarande så i `WebBankBudgeterUi.FillTablesAsync` (`BindKvarBudgetTableUi`). Om målet ändras till **SnurraIgenom**-Kvar, ersätts det av punkten “Öppet” ovan — inte av den arkiverade listan.
