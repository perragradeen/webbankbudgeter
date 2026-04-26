# Historik (webbankbudgeter)

## A. Aktuellt — vad som gäller i denna arbetskopia

### A.1 Gren `master` (2026-04-26)

- **`git log -3 --oneline`:** `5e1c121` (facit-xlsx + plan/README), `90a5331` (plan + Kvar/Budget-bindning), `e125952` (UI-refactor).
- **`git merge-base master HEAD`:** samma som `HEAD` när du står på `master` — det finns ingen lokal “kedja” av `cursor/…`-grenar i denna clone; tidigare Slack-sessioner kan ha jobbat i andra grenar på GitHub.

### A.2 Denna commit (gren `cursor/agents-history-console-facit-d200`)

- **`AGENTS.md`:** regler för agenter (facit via `ConsoleBudgeter`, ingen gissning om branches, uppdatering av plan/todo/README/HISTORY).
- **`HISTORY.md`:** denna fil — fylls på vid väsentliga åtgärder.
- **`ConsoleBudgeter`:** nytt `net8.0`-konsolprojekt i lösningen; skriver textfacit med `--out` och `--transaction-file` vid behov. Inställningar: `ConsoleBudgeter/Data/GeneralSettings.xml` (relativa sökvägar till `pelles budget.xls` och `BudgetterarnUi/Data/Categories.xml`). *Verifiering:* den incheckade `pelles budget.xls` i denna clone gav **0** transaktioner för 2014/2015 vid körning här — använd full export + diagnostikraderna i utskriften innan textfacit committas.
- **`InbudgetHandler/InBudgetKvarCalculator.cs`:** `SnurraIgenom` flyttad hit så konsol och UI delar samma kvar-matematik; `WebBankBudgeter` delegerar dit.
- **`BudgetStructureBuilder`:** inkomst = exakt trimmat `"+"`, förflyttning = exakt trimmat `" -"` (undviker felklassning av t.ex. `värnamoresor+övriga`).
- **`plan.md` / `todo.md` / `README.md`:** städade mot faktisk kod (TransactionHandler finns; Kvar-läge dokumenterat).

## B. Arkiv — äldre resonemang (behålls som bakgrund)

- **`todo.md` (före 2026-04-26):** beskrev en fyra-stegs-plan där Kvar skulle vara en kopia av Budget Total. I koden finns i stället `BindKvarBudgetTableUi` som duplicerar budget-tabellen till `gv_Kvar`; separat `SnurraIgenom`-flöde finns men är inte kopplat i `FillTablesAsync` på samma sätt som i vissa Slack-versioner. Se aktuell `WebBankBudgeterUi.cs` för sanning.
- Slack-tråden om flera `cursor/…`-PR:er i kedja gällde **andra clones**; jämför alltid med `git` i **din** arbetsyta.

När Del A växer: flytta äldre punkter hit eller till `HISTORY_ARCHIVE.md`.
