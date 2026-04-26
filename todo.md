# TODO

## Stämning av senaste instruktioner (bekräfta om något är fel tolkat)

| Punkt | Min tolkning | Status |
|-------|----------------|--------|
| **Ingen ny feature-gren** | Arbeta och pusha på `master` när det går; merga in remote-grenar i `master` i stället för att starta nya `cursor/…`-grenar om du uttryckligen vill undvika det. Molnmiljön kan ändå kräva `cursor/`-prefix i andra körningar. | Öppen |
| **Git-historik (denna klon, före merge)** | `master` hade `5e1c121` (xlsx + plan) medan `origin/cursor/console-budgeter-app-1a34` hade **29 commits** ovanför gemensam bas `90a5331` (ConsoleBudgeter, facit-JSON, Kvar/IN-kedja). Det förklarade “gren på gren” i grafer: serien av `cursor/`-jobb på samma kedja, plus en commit på `master` som inte fanns på console-grenen — **löst genom merge** in i `master`. | Klart (dokumenterat i `HISTORY.md`) |
| **`AGENTS.md`** | Bindande regler för agenter: facit = `ConsoleBudgeter` + `--out`; ingen parallell Python-pipeline utan uttryckligt beslut; facit ändras bara vid ny Excel-källa eller medveten extraktionsändring; efter verifierad build uppdateras plan/todo/README/HISTORY; läs alltid faktisk branch — spekulera inte om “okända” grenar utan `git branch -a` / `git log`. | Se checklist nedan |
| **`HISTORY.md`** | Logga varje större åtgärd; notera när `plan.md` var ur synk (t.ex. `TransactionHandler` “saknas” i plan trots att klassen finns i `WebBankBudgeterService`). | Se checklist nedan |
| **Textfacit 2014–2015** | Full konsolrapport för 2014 och 2015, alla transaktioner, sparad till fil (namn: `facit-2014-2015.txt` eller motsvarande); samma kommando som i `AGENTS.md` / README. | Väntar din verifiering av filinnehåll |

## Planerade åtgärder (checklista)

1. [x] Merga `origin/cursor/console-budgeter-app-1a34` in i `master` (ConsoleBudgeter, facit-JSON, delad logik).
2. [x] Lägga `AGENTS.md` i repo-roten.
3. [x] Skapa/uppdatera `HISTORY.md` (branchläge, merge, planstäd `TransactionHandler`).
4. [ ] Kör `dotnet build` / `dotnet test` (lämpliga projekt) lokalt eller i CI — markera när grönt. *(Cloud Agent-miljön här saknade `dotnet` i PATH vid senaste körning — kör samma kommandon på din maskin.)*
5. [x] Uppdatera `plan.md` (TransactionHandler m.m. + länk till `AGENTS.md`).
6. [x] Uppdatera `README.md` (`AGENTS.md`, textfacit, regenerate-kommando).
7. [x] Committa merge + dokumentation på `master` och pusha `master`.

---

## Användarönskemål (ConsoleBudgeter) — markera **KLART** när du verifierat

| Krav | Status |
|------|--------|
| Rapportordning: först **In** (`gv_incomes`), sen **Ut** / Budget Total (`gv_budget`), sen **Kvar** (`gv_Kvar`). | Väntar din verifiering |
| Under **Kvar** ska raden **"-"** (transaktions-/saldoplaceholder i facit) inte visas. | Väntar din verifiering |
| **Budget Total:** `värnamoresor+övriga` ska ligga bland övriga utgifter, inte under inkomstraden **"+"** (inkomst = exakt kategori `"+"`, trimmat). | Väntar din verifiering |
| Kör och lita på **ConsoleBudgeter** / `dotnet test` (inte ad hoc Python). | Väntar din verifiering |

---

## Textfacit + Excel-pipeline (plan D15 / D16)

| Uppgift | Status |
|---------|--------|
| **`WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt`** — full rapport 2014+2015 (alla transaktioner), UTF-8; samma pipeline som `ConsoleBudgeter --out`. Tidigare namn `console-report-facit-reference.txt` är borttaget till förmån för detta namn. | Väntar din verifiering |
| **`Facit/README.md`** + **`plan.md`**: JSON från extraktorn; textfacit = endast konsol `--out`, ingen duplicerad layout i extraktorn. | Väntar din verifiering |
| **WinForms:** användaren ska kunna **välja källa för in-poster** — se `plan.md` 0.6 / D16. | PENDING (implementation) |

---

## Plan M5 — status (kod)

| Punkt | Status |
|-------|--------|
| D7, D10, D12 | Klart i kod + tester (se `WebBankBudgeterServiceTest`) |
| M5.1 IN i Budget Total, M5.2 Kvar via `SnurraIgenom`, M5.7 `sv-SE` i grid, rensning före ombindning | Klart i kod |
| M5.3 `BudgetIns` / union IN–UT, D16 WinForms val av in-källa | Se `plan.md` / öppna punkter ovan |

---

## Arkiverad riktning (får inte följas som “sanning”)

Tidigare förslag att **Kvar** skulle vara en kopia av **Budget Total**-griden är **ersatt** av IN+UT via `SnurraIgenom` / `KvarTextTableBuilder` (se merge `console-budgeter-app-1a34` och `plan.md`).
