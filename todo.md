# TODO

Öppna punkter. Beslut, genomförd kod och gammal plantext: **`todo-history-arkiv.md`**. Process för agenter: **`AGENTS.md`**.

| Uppgift | Anteckning |
|---------|--------------|
| **Bygg och test** | `dotnet build Budgetterarn.NoWindowsUi.slnf` och `dotnet test Budgetterarn.NoWindowsUi.slnf` (Linux/CI); hela `Budgetterarn.sln` på Windows om WinForms ska med. |
| **M0** | Verifiera `TransactionHandler` mot avsedd transaktionskälla (~1 654 rader 2014+2015) och att UI-fasaden får rätt `TransactionList` — se `plan.md`. |
| **M4** | UI-integrationstester (`BudgetIntegrationTests`) — `net8.0-windows`; spec i arkivet §4.3. |
| **M3 (valfritt)** | Utöka service-tester så de täcker samma intent som tabellen i arkivet §4.2 (filnamn i arkiv: `FacitBudgetTests.cs`). |

**Valfritt:** grafisk inställning för `InPosterSource` / `FacitBudgetInDirectory` i stället för manuell redigering av `GeneralSettings.xml`.
