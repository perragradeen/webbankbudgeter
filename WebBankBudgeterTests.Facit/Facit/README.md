# Facit-data (utdragen ur pelles-budget-slim-2014-2015.xlsx)

## Ursprung
- Källa: `pelles budget.xls`
- Filen är ett fryst snapshot av användarens riktiga budget 2014–2015.
- Extrakt gjort av `tools/FacitExtractor/` (engångskörning, inte en del av bygget).

## Filer
| Fil | Innehåll | Källrad i Excel |
|-----|----------|-----------------|
| transactions-YYYY.json | En rad per transaktion | `Kontoutdrag_officiella` rad 2+ |
| budget-in-YYYY.json    | Budget per kategori per månad | `Budget (YYYY)` rad 25–57 |
| expected-ut-YYYY.json  | Summa transaktioner per (kat, mån) | Beräknat ur transaktioner |
| expected-kvar-YYYY.json| Budget + utfall per (kat, mån) | Beräknat (IN + UT) |

## Invarianter som testas
1. `sum(transactions.amount where Flag != "Ignore") per kategori per månad == expected-ut` (tolerans ±0.01)
2. `budget-in + expected-ut == expected-kvar` (per kategori per månad)
3. Transaktioner med `Flag == "Ignore"` räknas **inte** med i UT.
4. Antal transaktioner per år: 2014 = 809, 2015 = 845.
5. IN 2014 har 336 rader.
6. IN 2015 har 336 rader.
7. Transfers (`" -"`) ingår i egen fil `expected-transfers-YYYY.json`.
