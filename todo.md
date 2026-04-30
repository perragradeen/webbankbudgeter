# TODO

Öppna punkter. Beslut, genomförd kod och gammal plantext: **`todo-history-arkiv.md`**. Process för agenter: **`AGENTS.md`**. **Rensa plan:** nyckelord **`plan-arkiv`** i **`README.md`**.

| Uppgift | Anteckning |
|---------|--------------|
| **Bygg och test** | `dotnet build Budgetterarn.NoWindowsUi.slnf` och `dotnet test Budgetterarn.NoWindowsUi.slnf` (Linux/CI); hela `Budgetterarn.sln` på Windows om WinForms ska med. Kräver `.NET 8 SDK` i `PATH` (t.ex. `dotnet-sdk-8.0` på Ubuntu). |
| **M0** | **Klart (automatiserat):** `FacitTransactionCountTests`, `TransactionHandlerM0Tests`, `KontoEntry` år. **Kvar (manuellt):** arkiv-`.xls` 2014–2015 när fil finns — checklista **`todo-history-arkiv.md` § M0 verifiering**. UI ↔ `TransactionList`/`Account`: läs `WebBankBudgeter.cs` / `TransactionHandler`. |
| **M4** | UI-integrationstester (`BudgetIntegrationTests`) — `net8.0-windows`; spec i arkivet §4.3. |
| **M3 (valfritt)** | Utöka service-tester så de täcker samma intent som tabellen i arkivet §4.2 (filnamn i arkiv: `FacitBudgetTests.cs`). |

**Valfritt:** grafisk inställning för `InPosterSource` / `FacitBudgetInDirectory` i stället för manuell redigering av `GeneralSettings.xml`.
