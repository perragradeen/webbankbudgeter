# TODO — levande lista

## Stämning av senaste instruktioner (bekräfta om något är fel tolkat)

| Punkt | Min tolkning | Status |
|-------|----------------|--------|
| **Ingen ny feature-gren** | Arbeta på befintlig `master`, merga in saknad kod från remote om det behövs — inte skapa `cursor/...` om du uttryckligen vill undvika nya grenar i det här läget. | Väntar din OK om molnmiljön kräver `cursor/`-gren |
| **Git-historik på *denna* klon** | `master` är vid `5e1c121` (xlsx + plan/README). `origin/cursor/console-budgeter-app-1a34` har **29 commits** ovanpå gemensam bas `90a5331` och innehåller bl.a. `ConsoleBudgeter`, facit-JSON, textfacit-kedja — **saknas** på nuvarande `master`. `master` har **en** commit (`5e1c121`) som grenen inte har. | Dokumenterat |
| **`AGENTS.md`** | Bindande regler: facit = konsol `--out`, ingen Python-sidospår utan beslut; facit ändras bara vid ny källa/regel; efter verifierad build uppdateras plan/todo/README/HISTORY; agenter ska läsa faktisk branch/repo, inte gissa okända grenar. | PENDING |
| **`HISTORY.md`** | Finns inte i denna `master`; skapas/uppdateras med denna åtgärd + pekare på att `plan.md` fortfarande påstod att `TransactionHandler` saknades trots att den finns i `WebBankBudgeterService/TransactionHandler.cs`. | PENDING |
| **Textfacit 2014–2015** | Kör `ConsoleBudgeter` med alla transaktioner för båda åren, spara stdout till en namngiven fil (t.ex. `facit-2014-2015.txt`) via `--out` om projektet stödjer det — annars shell-redirect. | PENDING (efter merge) |
| **`plan.md` om TransactionHandler** | Uppdatera så M0/D6/G7/R1 inte påstår att klassen saknas (den ligger i `WebBankBudgeterService`). | PENDING |

## Planerade åtgärder (i ordning)

1. [ ] Merga `origin/cursor/console-budgeter-app-1a34` in i `master` så all kod (ConsoleBudgeter, facit, delad logik) finns på senaste `master`.
2. [ ] Lägga `AGENTS.md` i repo-roten enligt tabellen ovan.
3. [ ] Skapa/uppdatera `HISTORY.md` med: nuvarande branchläge, att console-grenen var “eftersläpning” vs xlsx-commit, och planfix om TransactionHandler.
4. [ ] Kör `dotnet build` / `dotnet test` (lämpliga projekt) och generera `facit-2014-2015.txt` via konsolappen.
5. [ ] Uppdatera `plan.md` (TransactionHandler-stämpling + kort länk till `AGENTS.md`).
6. [ ] Uppdatera `README.md` med länk till `AGENTS.md` och var textfacit-filen ligger + kommando för att regenerera.
7. [ ] Committa på `master` och pusha `master` (ingen ny gren om du inte kräver annat).

## Arkiverad / föråldrad riktning (ersätts av merge + Kvar via SnurraIgenom)

Den gamla fyra-stegs-planen (“Kvar = samma bindning som Budget Total”) var fel i förhållande till senare beslut (Kvar = IN+UT / `SnurraIgenom`). Själva implementationen finns på console-grenen; efter merge ska denna fil peka på `plan.md` och tester som sanning.

---

*Detaljer nedan behålls tillfälligt som referens till gamla problembeskrivningen — ska tas bort eller ersättas när merge är klar och dokumentationen är synkad.*

## Problem (historisk)

"Kvar"-fliken (`gv_Kvar`) var nästan tom medan "Budget Total" (`gv_budget`) visade full data — **lösningen är inte** att kopiera samma tabell till Kvar, utan korrekt IN+UT-kedja (se merge från `console-budgeter-app-1a34`).
