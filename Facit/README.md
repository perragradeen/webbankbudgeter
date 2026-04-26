# Textfacit (konsol)

Generera med:

```bash
dotnet run --project ConsoleBudgeter/ConsoleBudgeter.csproj -- --year 2014 --year 2015 --out Facit/facit-2014-2015-console.txt
```

Kontrollera att blocket **## Diagnostik** visar icke-noll transaktioner för 2014 och 2015 innan filen committas som facit. Vid 0 rader: använd `--transaction-file` mot rätt `.xls`-export.
