# Consolidated T01–T08 Execution / Evidence Record

**Honesty rule (from the handout):** do not mark a test Pass unless you ran it and have evidence. If a test is not run, write `Not run` and the reason. Screenshots alone are not enough for API responses, persistence, transactions or architecture.

Environment to record for every run: date/time, machine, .NET SDK (`dotnet --version`), PostgreSQL version, API clock (`GET /dev/info` → `today`; target **2026-11-10**), commit hash.

How to generate most of the evidence automatically: see README §8. The runner writes `evidence/acceptance-run.md` with real requests, responses, status codes and PostgreSQL snapshots for **T01–T06**. T07 and T08 need manual steps (below).

## Status summary (update after you run things)

| Test | Automated by runner | Manual part | Result | Evidence reference |
|---|---|---|---|---|
| T01 | yes | — | Not run | |
| T02 | yes | restart check (README §8.3) | Not run | |
| T03 | yes | restart check | Not run | |
| T04 | yes | restart API, re-read | Not run | |
| T05 | yes (uses `/dev/fault`) | — | Not run | |
| T06 | yes | — | Not run | |
| T07 | no | desktop + isolation script + code walkthrough | Not run | |
| T08 | no | stop API, bad DB config, recovery | Not run | |

## Test Execution Records
Copy one block per run/retest.

### T01 — date / environment: ________
- Expected: 200 arrays/objects match seed; `available=true` excludes E1/E4/E5; empty history `[]`; unknown id and bad route 404; unsupported method 405; OpenAPI retrievable and describes all 16 contracts. No DB change.
- Observed: ________
- Pass / fail: ________
- Issue / diagnosis: ________
- Correction: ________
- Retest outcome: ________
- Evidence reference: `evidence/acceptance-run.md#t01…`

### T02 — Students (API-04–06)
- Expected: 201+Location; PUT 200; blank 400; duplicate `" st-100 "` 409; restricted issue 409; return then permit then valid issue 201; same eligibility 200; history preserved; edits survive restart.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

### T03 — Equipment (API-07–09)
- Expected: 201 available=true; PUT 200; blank 400; duplicate `" eq-001 "` 409; `isAvailable` in update 400; E1 stays unavailable with its history.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

### T04 — Single issue/return (API-10, 12, 13)
- Expected: 201 then GET 200; invalid date 400; missing 404; ineligible/unavailable 409; return 200; repeat 409; E2 available after return; rejected attempts write nothing; state survives API restart.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________
- **Restart step (manual):** stop the API (Ctrl+C), start it again, run `GET /api/v1/borrowings/{id}` and `db/readback.sql`; paste both.

### T05 — Multi-item & rollback (API-10, 11)
- Expected: 2a 201 (1 borrowing, S3 has 3 active); 2b 201 (2 borrowings, S1 has 3 active); 2c 409; invalid requests 400/404/409; injected failure → 500 and **fresh DB read shows no rows/availability changes**.
- Documented failure point: after the first `Borrowing` row is written (`SaveChangesAsync`) inside the transaction, before commit (`IFaultInjector.AfterBorrowingWritten`, armed via `POST /dev/fault?armed=true`, disposable DB only).
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

### T06 — Histories/search/filters/sorting (API-14–16)
- Expected: see runner checks; `[]` for no match; 404 unknown id; 400 unsupported status; no DB change.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

### T07 — Desktop is an HTTP client (API-01–16)
Steps:
1. Start the API only (no desktop) and run a valid read (`GET /api/v1/equipment`) — paste request/response.
2. Start the desktop. Register/edit a student and an item, issue (single and multi), return, open both histories. Capture screenshots **and** the API server log lines for the same requests.
3. With the desktop open, change data with an independent HTTP client (e.g. `PUT` a student name), click **Reload all data**, show the UI now displays it. Run `db/readback.sql` to show the same value in PostgreSQL.
4. Run `scripts/check-desktop-isolation.ps1` (or `.sh`) and paste the output; show `EquipmentBorrowing.Desktop.csproj` references only `Contracts`. Code walk-through: `ApiClient.SendAsync`, `OperationRunner.RunAsync`, `MainWindowViewModel.RefreshAllAsync`.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

### T08 — Unreachable vs 500, recovery, restart (API-01–16, BR-18/19)
Steps:
1. **Stop the API.** `curl -v http://localhost:5080/api/v1/equipment` → *connection refused, no HTTP status* (paste). In the desktop click **Reload all data** → banner starts "CONNECTION PROBLEM…" (screenshot).
2. Start the API with a deliberately wrong password for a **disposable** DB:
   `ConnectionStrings__Default="Host=localhost;Database=equipment_borrowing_dev;Username=postgres;Password=wrong"`
   `GET /health` → 200 (API alive); `GET /api/v1/equipment` → **500** `database_unavailable` with a safe body (paste, check no password/stack trace). Desktop banner starts "SERVER ERROR (HTTP 500…)" (screenshot). Paste the server log excerpt with secrets redacted.
3. Restore the correct configuration, restart: reads return 200; previously saved data (from T04/T05) is still present (`db/readback.sql`).
- Unknown-outcome note: if a connection drops while a mutation is in flight, the desktop shows "UNKNOWN OUTCOME", does not retry, and reloads; verify with a fresh read/`db/readback.sql` before repeating.
- Observed / Pass-fail / Issue / Correction / Retest / Evidence: ________

## Section 5 — Failures, diagnosis, correction, retest log
| # | Test | What failed (actual) | Diagnosis | Correction (commit) | Retest outcome |
|---|---|---|---|---|---|
| 1 | | | | | |

> Expect some first-run failures: this code was written without being compiled or executed (no .NET SDK in the authoring environment). Record real compile/runtime problems here with their fix — that is exactly what this section is for.
