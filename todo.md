# TODO

Öppna punkter. Beslut, genomförd kod och gammal plantext: **`todo-history-arkiv.md`**. Process för agenter: **`AGENTS.md`**.

| Uppgift | Anteckning |
|---------|--------------|
| **Bygg och test** | `dotnet build Budgetterarn.NoWindowsUi.slnf` och `dotnet test Budgetterarn.NoWindowsUi.slnf` (Linux/CI); hela `Budgetterarn.sln` på Windows om WinForms ska med. Kräver `.NET 8 SDK` i `PATH` (t.ex. `dotnet-sdk-8.0` på Ubuntu). |
| **M0** | **Klart (automatiserat):** `FacitTransactionCountTests` — antal rader i facit-JSON (809+845); `TransactionHandlerM0Tests` — `pelles budget.xls`-spann + **`Pelles-budget-slim-2014-2015-gform.xlsx`** (809/845 mot facit). **Kvar:** manuell `TransactionHandler` + arkiv-.`xls` 2014–2015 när fil finns på `TransactionTestFilePath`. UI ↔ `TransactionList` / `Account`: se `plan.md`. |
| **M4** | UI-integrationstester (`BudgetIntegrationTests`) — `net8.0-windows`; spec i arkivet §4.3. |
| **M3 (valfritt)** | Utöka service-tester så de täcker samma intent som tabellen i arkivet §4.2 (filnamn i arkiv: `FacitBudgetTests.cs`). |

**Valfritt:** grafisk inställning för `InPosterSource` / `FacitBudgetInDirectory` i stället för manuell redigering av `GeneralSettings.xml`.
