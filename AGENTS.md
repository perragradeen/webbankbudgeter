# Instruktioner för agenter (Cursor m.fl.)

Det här dokumentet är **bindande** för automatiserad hjälp i repot. Läs det innan du ändrar kod, grenar eller facit.

## 1. Verifiera *den här* arbetskopian — inte “någon branch”

- Kör `git branch --show-current`, `git log -5 --oneline` och vid behov `git merge-base master HEAD` **innan** du påstår vad som finns i repot eller “saknas”.
- Gissa inte bort facit-filer, `ConsoleBudgeter` eller andra artefakter: lista dem med verktyg eller `git ls-files`.
- Om användaren säger att något ska ligga i “senaste branchen”: tolka det som **den gren du faktiskt står på** tills de anger annat.

## 2. Grenar och “all kod i senaste branchen”

- Nya funktionsgrenar ska följa projektets namnkonvention (t.ex. `cursor/...-d200` i denna miljö).
- Om du fortsätter på en befintlig `cursor/…`-gren utan att först merga `master` in i den, blir PR-diffen ofta en **kedja** av tidigare arbete. Det är ett medvetet val; om målet är en **minimal PR** mot `master`, checkout från uppdaterad `master` (eller rebase) enligt vad användaren vill.
- **Implementera inte** funktioner genom att anta att de redan finns i en annan remote-gren om de inte syns i den checkout du jobbar i.

## 3. Textfacit = kör konsolappen, spara stdout — ingen parallell pipeline

- **Textfacit** är den fulla rapporten som **`ConsoleBudgeter`** skriver till fil (UTF-8), inte en separat Python-beräkning och inte manuellt hopkok som duplicerar affärslogik.
- Standardkommando (från repo-roten, när `pelles budget.xls` och kategorier finns där `ConsoleBudgeter/Data/GeneralSettings.xml` pekar):

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- --year 2014 --year 2015 --out Facit/facit-2014-2015-console.txt
```

- Vid behov, peka ut en annan `.xls` än standard i `ConsoleBudgeter/Data/GeneralSettings.xml`:
  `--transaction-file /full/path/till/pelles budget.xls`
- Rapporten inleds med en **diagnostikrad** (antal transaktioner per år). Om 2014/2015 blir 0: fel källa eller ofullständig fil — byt fil, committa inte en tom “facit”.

- Filnamn kan vara t.ex. `Facit/facit-2014-2015-console.txt` (byt bara om teamet enas om annat namn).
- **Ändra inte** en committad facit-fil för att “få gröna tester”. Uppdatera facit endast när källan (Excel/regler) eller rapportpipen medvetet ändrats; granska diff.

## 4. Efter genomförd och verifierad ändring: plan, todo, README, historik

- Uppdatera `plan.md` / `todo.md` / `README.md` så de stämmer med **faktisk kod** (inga kvarvarande påståenden om att `TransactionHandler` “saknas” om klassen finns).
- Lägg en kort post i **`HISTORY.md`** (vad, varför, vilka filer) vid varje väsentlig åtgärd som användaren bör kunna spåra.

## 5. `TransactionHandler`

- Klassen ligger i `WebBankBudgeterService/TransactionHandler.cs`. Planens gamla formuleringar om att den “saknas fysiskt” ska **rättas i planen**, inte upprepas i nya svar.

## 6. Linux / headless

- `WebBankBudgeterUi` kräver `net8.0-windows`. Konsol- och serviceprojekt kan byggas på Linux när .NET SDK finns.
- I denna miljö kan `dotnet` saknas i PATH; då ska agenten **dokumentera** att bygget inte körts här och ange exakt kommando för användaren — men ändå leverera konsekvent kod.
