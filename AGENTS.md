# Instruktioner för agenter (Cursor / motsvarande)

Läs denna fil **före** du ändrar kod eller dokumentation. Den speglar uttryckliga önskemål från projektägaren.

## 1. Verifiera den faktiska arbetskopian

- Kör `git branch --show-current`, `git branch -a`, `git log --oneline -20`, `git status` och tolka **den branch du faktiskt står på** — inte en “trolig” eller diskuterad gren någon annanstans.
- Vid behov: `git merge-base master HEAD` (eller motsvarande bas-gren) **innan** du påstår vad som finns eller “saknas”.
- Lista facit-filer och viktiga artefakter med verktyg eller `git ls-files`; gissa inte bort innehåll som användaren säger finns i “senaste branchen” — tolka det som **den checkout du står på** tills de anger annat.
- Om användaren talar om kod som “saknas” lokalt: kontrollera med `git log` / `git merge` mot `origin/…` innan du påstår att något inte finns i repot.

## 2. Grenar

- **Standard:** nya funktioner från **senaste `master`** (eller explicit angiven bas-gren).
- **Undvik** kedjor av nya `cursor/…`-grenar om användaren bett om att jobba på `master` eller om molnmiljön annars skapar redundanta PR-kedjor. Om du *måste* skapa en gren (t.ex. policy), dokumentera varför i `HISTORY.md`.
- Implementera inte genom att anta att kod finns i en annan remote-gren om den inte syns i den checkout du jobbar i.

## 3. Textfacit (hela konsolrapporten 2014–2015)

- **Enda tillåtna sättet** att producera eller uppdatera textfacit: kör `ConsoleBudgeter` och skriv utdata till fil med `--out`.
- **Filnamn i repot:** `WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt`.
- **Kommando:**

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- \
  --year 2014 --year 2015 --transactions 0 \
  --out WebBankBudgeterTests.Facit/Facit/facit-2014-2015.txt
```

- Vid behov: `--transaction-file /full/stig/till/pelles\ budget.xls` om standardkälla i inställningar inte är rätt. Om rapporten inleds med diagnostik och 2014/2015 får **0** transaktioner: byt källa — committa inte en tom “facit”.
- **Ingen** parallell beräkning av samma rapport i Python eller andra skript — utom om användaren **uttryckligen** ber om det.

## 4. När får facit (JSON eller text) ändras?

- Bara vid **ny eller korrigerad Excel-källa**, eller **medvetet beslut** som ändrar extraktionsregler — sedan diff-granskning och uppdatering av tester/plan som hör ihop.
- **Ändra inte** facit bara för att få tester gröna; då tappar ni referensen.

## 5. Efter bygg / test (när du verifierat lokalt eller i CI)

Uppdatera i samma leverans där det är relevant:

- `plan.md` — om milstolpar, risker eller beslut ändrats.
- `todo.md` — vad som är gjort / väntar användarverifiering.
- `README.md` — kommandon, Linux/WinForms-skillnader, länkar.
- `HISTORY.md` — kort post om **vad** som gjorts, **varför**, och ev. git-fakta (branch, merge).

Om `dotnet` saknas i PATH i en miljö: dokumentera att bygget inte körts där och ange exakt kommando för användaren.

## 6. `TransactionHandler` och `plan.md`

- Källan finns i `WebBankBudgeterService/TransactionHandler.cs`. Om `plan.md` fortfarande säger att klassen “saknas”, är planen **fel** — rätta den, upprepa inte myten i svar till användaren.

## 7. Todo först vid oklar tolkning

Om instruktionen kan tolkas på flera sätt: skriv avsedd tolkning som en **stämningstabell** överst i `todo.md`, genomför sedan arbetet när konflikten är hanterad eller uppenbart säker.

## 8. Linux / headless

- `WebBankBudgeterUi` kräver `net8.0-windows`. Konsol- och serviceprojekt kan byggas på Linux när .NET SDK finns.
