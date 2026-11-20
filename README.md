# ITSD 81 — Laboratory Activity 5: API Integration and Acceptance Testing
Campus Equipment Borrowing System — ASP.NET Core REST API (`/api/v1`) over PostgreSQL + an Avalonia desktop client that talks to it over asynchronous HTTP.

> **Read first:** this solution was prepared **without access to your Lab 4 source** (GitHub blocked the repo) and **without a .NET SDK to compile it**. It re-implements the same domain from the Lab 5 handout/reference. Expect to fix a few compile errors on first build and to compare business rules with your Lab 4 Use Case Suite (`docs/assumptions.md`). Record real findings in `docs/test-evidence.md` §5. Nothing in the evidence files is pre-filled as "passed".

## 1. Layout
```
EquipmentBorrowing.sln
src/EquipmentBorrowing.Contracts      shared DTOs (only thing the desktop references)
src/EquipmentBorrowing.Domain         entities
src/EquipmentBorrowing.Application    services, repository abstractions, errors
src/EquipmentBorrowing.Infrastructure EF Core + PostgreSQL, migrations, repositories, seed data
src/EquipmentBorrowing.Api            REST API host, OpenAPI at /openapi/v1.json
src/EquipmentBorrowing.Desktop        Avalonia MVVM client (HTTP only)
tools/EquipmentBorrowing.AcceptanceRunner   optional T01–T06 runner that writes evidence
docs/    architecture, assumptions, openapi.yaml, traceability matrix, baseline record, test evidence
db/readback.sql   independent PostgreSQL readbacks     http/api.http   request collection
scripts/check-desktop-isolation.(ps1|sh)  T07 evidence
```

## 2. Prerequisites
- .NET SDK 8 or newer (`dotnet --version`). Targeting is one property: `AppTfm` in `Directory.Build.props` (change to `net9.0`/`net10.0` to match Lab 4).
- PostgreSQL 14+ running locally. No Docker, cloud, paid service or authentication is needed.

## 3. Create a disposable database (once)
```sql
CREATE DATABASE equipment_borrowing_dev;
```
The name must contain `_dev`, `_test` or `disposable`: reset/seed refuse to run on any other database.

## 4. Configure credentials (nothing secret is committed)
`appsettings.json` has the connection string **without a password**. Provide the real one with user-secrets (works when running with the default `api` launch profile, which sets `Development`):
```bash
cd src/EquipmentBorrowing.Api
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=equipment_borrowing_dev;Username=postgres;Password=YOUR_PASSWORD"
```
or with an environment variable (works everywhere, also for `dotnet ef` and the runner):
- PowerShell: `$env:ConnectionStrings__Default = "Host=localhost;Database=equipment_borrowing_dev;Username=postgres;Password=YOUR_PASSWORD"`
- bash: `export ConnectionStrings__Default="Host=localhost;Database=equipment_borrowing_dev;Username=postgres;Password=YOUR_PASSWORD"`

## 5. Apply migrations and seed the controlled dataset
```bash
dotnet run --project src/EquipmentBorrowing.Api -- migrate   # apply migrations only
dotnet run --project src/EquipmentBorrowing.Api -- reset     # migrate + wipe + load S1–S4 / E1–E7 (disposable DB only)
```
Add a future migration: `dotnet ef migrations add Name --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Infrastructure` (needs `dotnet tool install -g dotnet-ef`, and `ConnectionStrings__Default` set). If `dotnet ef` complains that the hand-written snapshot differs from the model, regenerate with `dotnet ef migrations remove` / `add InitialCreate` against an empty database and keep the generated files.

**Existing Lab 4 database:** the initial migration creates `Students`, `Equipment`, `Borrowings`. If your Lab 4 PostgreSQL schema already has tables of these names, either point Lab 5 at a fresh database (recommended for the acceptance runs) or reconcile your Lab 4 migrations with this one and document it in `docs/baseline-record.md`.

## 6. Start the API (without Avalonia)
```bash
# normal
dotnet run --project src/EquipmentBorrowing.Api
# repeatable acceptance runs: fixed clock + dev helpers (disposable DB only!)
#   PowerShell: $env:Clock__FixedDate="2026-11-10"; $env:Dev__Enabled="true"
#   bash:       export Clock__FixedDate=2026-11-10 Dev__Enabled=true
```
Listens on `http://localhost:5080`. Try: `GET /health` (no DB), `GET /api/v1/equipment`, `GET /openapi/v1.json`. Use `http/api.http` or curl.

Optional settings (appsettings or env vars with `__`): `Borrowing:MaxActiveBorrowings` (3), `Clock:FixedDate`, `Dev:Enabled`, `Urls`.

## 7. Start the desktop app (separately)
```bash
dotnet run --project src/EquipmentBorrowing.Desktop
```
API address: `src/EquipmentBorrowing.Desktop/appsettings.json` (`Api:BaseUrl`) or env var `EQUIPMENT_Api__BaseUrl`. The app has tabs for Borrowings (search/filter/sort/return), Check out (single or atomic multi-item), Students and Equipment, with loading bar, validation, connection and server-error banners. It never blocks the UI thread.

## 8. Run the acceptance tests and collect evidence
1. Dev API running as in §6 (`Dev__Enabled=true`, `Clock__FixedDate=2026-11-10`, disposable database).
2. In another terminal, with `ConnectionStrings__Default` set (enables the independent PostgreSQL snapshots):
```bash
dotnet run --project tools/EquipmentBorrowing.AcceptanceRunner -- --out evidence/acceptance-run.md
```
It resets the data before each test, calls the real API, checks status codes/bodies/DB rows, and writes every request, response and snapshot to `evidence/acceptance-run.md`. Exit code 1 means at least one check failed — diagnose, fix, re-run, and log it in `docs/test-evidence.md` §5.
3. Persistence/restart: after T04, stop and restart the API, repeat the GET and `db/readback.sql`.
4. T07 and T08 are manual: follow `docs/test-evidence.md` and run `scripts/check-desktop-isolation.ps1`.
5. Fill the status table, the traceability matrix evidence column and `docs/baseline-record.md`.

## 9. Submission checklist (Handout §IV)
- [ ] Copy this folder's contents into your Lab 4 repository (keep Lab 4 history); commit.
- [ ] `docs/baseline-record.md` completed (Lab 4 commit/snapshot, limitations).
- [ ] `docs/traceability-matrix.md` evidence column completed.
- [ ] `docs/test-evidence.md` completed honestly (including failures and retests).
- [ ] `git tag lab5-submitted` on the final commit and push tags.
- [ ] Each pair member submits individually, naming both partners (full name + student ID) in the submission comment.

## 10. Endpoint contract
Summary table of API-01–16 with routes, statuses and error codes: `docs/openapi.yaml` (also served at `/openapi/v1.yaml`). Error body is always `{ "code": "...", "message": "..." }`; 400 invalid, 404 missing/unknown route, 405 wrong method, 409 business conflict, 500 safe server/database failure (`server_error`, `database_unavailable`, `database_error`, `commit_outcome_unknown`).
