# Projekt­historik — arkiv (WebBankBudgeter)

> **Syfte:** Långa bakgrundsposter, gamla branch-namn och sessionsdetaljer som **inte** ska behövas för dagligt arbete.  
> **Aktuellt läge:** se `HISTORY.md` (Del A) och `plan.md` / `todo.md`.

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

*Flyttat från `HISTORY.md` 2026-04-26 (dokumentstäd på gren `ai`).*
