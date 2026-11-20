# Implementation Baseline Record (Lab 5)

> Fill in the **Student entry** column yourself. Entries marked TODO could not be known while this solution was prepared. Do not invent history: if the original Lab 4 commit cannot be verified, say so under *Baseline limitations* (the reference requires this honesty).

| Field | Student entry |
|---|---|
| Lab 4 submitted commit or verifiable snapshot | **TODO** — commit hash from `git log` of `https://github.com/BruhhhhN1664/act4dad.git` (or the submitted zip/LMS file) and how you identified it as the submitted version. |
| Baseline limitations | **TODO** — missing original history or incomplete Lab 4 functionality, if any. Note: Lab 5 code in this package was written without access to your Lab 4 source; it re-implements the same domain and rules (see `docs/assumptions.md`). |
| Lab 5 revision and tag | **TODO** — commit hash after you commit; tag it `lab5-submitted` (`git tag lab5-submitted && git push origin lab5-submitted`). Create a `lab4-submitted` tag only if it points at the genuine submitted commit. |
| Before / after boundary | Before: desktop used the application/persistence layers directly (per the Lab 5 handout) — **TODO** name the Lab 4 classes. After: `Desktop` → `ApiClient` (HTTP) → `Api` → `Application` → `Infrastructure` (EF Core) → PostgreSQL. See `docs/architecture.md`. |
| Migration / refactoring | New initial migration `20261120000000_InitialCreate` (students, equipment, borrowings, unique indexes, partial unique index for one active borrowing per item, date check constraints, `Restrict` FKs). **TODO**: list any Lab 4 migrations you keep and how Lab 4 data maps to these tables (README "Existing Lab 4 database"). Disclose any refactoring you did while merging. |
| Reproduction | README sections 2–7; API base URL `http://localhost:5080`; reset with `dotnet run --project src/EquipmentBorrowing.Api -- reset`; evidence in `evidence/` and `docs/test-evidence.md`. |
