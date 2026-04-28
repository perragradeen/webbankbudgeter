# TODO

Öppna punkter. Beslut, genomförd kod och gammal plantext: **`todo-history-arkiv.md`**. Process för agenter: **`AGENTS.md`**.

| Uppgift | Anteckning |
|---------|--------------|
| **Bygg och test** | `dotnet build Budgetterarn.NoWindowsUi.slnf` och `dotnet test Budgetterarn.NoWindowsUi.slnf` (Linux/CI); hela `Budgetterarn.sln` på Windows om WinForms ska med. Kräver `.NET 8 SDK` i `PATH` (t.ex. `dotnet-sdk-8.0` på Ubuntu). |
| **M0** | **Kvar:** `TransactionHandler` + samma `Pelles Budget.xls` som facit kom från — jämför antal/struktur mot facit-JSON i CI (saknas ofta i sandlådor). **Automatiserat:** antal rader i `transactions-2014/2015.json` (809+845) testas i `WebBankBudgeterServiceTest/FacitTransactionCountTests.cs`. UI ↔ `TransactionList` / `Account`: se `plan.md`. |
| **M4** | UI-integrationstester (`BudgetIntegrationTests`) — `net8.0-windows`; spec i arkivet §4.3. |
| **M3 (valfritt)** | Utöka service-tester så de täcker samma intent som tabellen i arkivet §4.2 (filnamn i arkiv: `FacitBudgetTests.cs`). |

**Valfritt:** grafisk inställning för `InPosterSource` / `FacitBudgetInDirectory` i stället för manuell redigering av `GeneralSettings.xml`.
