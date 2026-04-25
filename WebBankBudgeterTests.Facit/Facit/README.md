# Facit (JSON)

Källa: `Pelles-budget-slim-2014-2015-gform.xlsx` i repots rot.

## Uppdatera filerna

Kör (kräver .NET 8 SDK):

```bash
dotnet run --project tools/FacitExtractor/FacitExtractor.csproj -- \
  Pelles-budget-slim-2014-2015-gform.xlsx \
  WebBankBudgeterTests.Facit/Facit
```

## Filer

| Fil | Innehåll |
|-----|----------|
| `transactions-YYYY.json` | En rad per rad i `Kontoutdrag_officiella` för året, med `flag` (`Regular` / `Ignore`) |
| `budget-in-YYYY.json` | Budget IN per kategori och månad |
| `expected-ut-YYYY.json` | Summa transaktioner per (kategori, månad), **utan** `Ignore` och **utan** kategori ` -` (förflyttningar) |
| `expected-transfers-YYYY.json` | Aggregerat för kategori ` -` |
| `expected-kvar-YYYY.json` | `budgetAmount + actualAmount` per nyckel (union IN/UT) |

## Särfall

- **2014 IN**: januari-kolumnen i Excel är 0 för alla kategorier; extraktorn **hoppar januari** för 2014 så att `budget-in-2014` speglar planens D5 (inga egna IN-rader för januari).
- **2015 IN**: alla tolv månader exporteras.

Antal transaktioner ska vara **809** (2014) och **845** (2015).
