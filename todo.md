# TODO

## Stämning (aktiva tolkningar)

| Punkt | Tolkning | Status |
|-------|----------|--------|
| **Grenpolicy** | Nya funktioner från senaste `master` om inget annat sägs; undvik onödiga `cursor/…`-grenar om du uttryckligen vill jobba på `master` — se `AGENTS.md`. | Öppen (process) |
| **Verifiera checkout** | Läs alltid faktisk branch (`git branch`, `git status`); spekulera inte om filer på “okända” grenar. | Påminnelse |

---

## Öppet arbete (kod / miljö)

| Uppgift | Anteckning |
|---------|-------------|
| **Bygg och test** | Kör `dotnet build Budgetterarn.NoWindowsUi.slnf` och `dotnet test Budgetterarn.NoWindowsUi.slnf` (eller hela lösningen på Windows) och notera resultat i CI eller här när grönt. |
| **M0** | Verifiera `TransactionHandler` mot **riktig** transaktionskälla (~1 654 rader 2014+2015) och att UI-fasaden får rätt `TransactionList` — se `plan.md` §5 M0. |
| **M4** | UI-integrationstester (`BudgetIntegrationTests` enligt plan §4.3) — kräver **Windows** / `net8.0-windows`. |
| **M3 (valfritt utökning)** | Plan §4.2 nämner `FacitBudgetTests.cs`; dagens facit-täckning ligger bl.a. i `ConsoleBudgeterTest` och `InBudgetMathSnurraIgenomTests` — utöka om ni vill spegla exakt tabellen i planen. |

---

## Valfritt / backlog (produkt)

| Uppgift | Anteckning |
|---------|-------------|
| **Grafiskt val av in-källa** | D16 minimum är **uppfyllt** via `GeneralSettings.xml` (`InPosterSource` = `BudgetIns` \| `FacitJson`). Utökning: dialog under Inställningar om du vill slippa manuell XML-redigering. |

---

## Arkiverad riktning (får inte följas som “sanning”)

Tidigare förslag att **Kvar** skulle vara en kopia av **Budget Total**-griden är **ersatt** av IN+UT via `SnurraIgenom` / `KvarTextTableBuilder` (se merge `console-budgeter-app-1a34` och `plan.md`).

**Längre sessionsbakgrund** (multi-agent, gamla branch-namn, agent-ID): `HISTORY_ARCHIVE.md`.
