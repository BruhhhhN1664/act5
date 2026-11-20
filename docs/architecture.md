# Architecture (Lab 5)

```
┌────────────────────────┐   HTTP + JSON (/api/v1)   ┌────────────────────────────────────────────┐   Npgsql   ┌────────────┐
│ Avalonia desktop       │ ────────────────────────► │ ASP.NET Core API (EquipmentBorrowing.Api)  │ ─────────► │ PostgreSQL │
│ Views → ViewModels     │ ◄──────────────────────── │  Endpoints → Application services          │ ◄───────── │            │
│ → ApiClient (HttpClient)│   status + JSON / errors  │  → repositories / unit of work (EF Core)   │            └────────────┘
└────────────────────────┘                           └────────────────────────────────────────────┘
        │ references only                                      │ references
        ▼                                                      ▼
  EquipmentBorrowing.Contracts (DTOs)  ◄──── shared ────►  Contracts, Application, Domain, Infrastructure
```

| Project | Responsibility |
|---|---|
| `Contracts` | The wire DTOs (requests, responses, `ErrorResponse`). The **only** assembly shared with the desktop app. |
| `Domain` | Entities `Student`, `EquipmentItem`, `Borrowing`, key normalisation. No dependencies. |
| `Application` | Business coordination: `StudentService`, `EquipmentService`, `BorrowingService`; repository/unit-of-work abstractions; errors (400/404/409). |
| `Infrastructure` | EF Core + PostgreSQL: `AppDbContext`, migrations, repositories, `EfUnitOfWork`, seed dataset. |
| `Api` | Minimal-API endpoints, strict JSON, error middleware, OpenAPI document, dev-only helpers. |
| `Desktop` | Avalonia MVVM client. `ApiClient` is its single data path; no SQL, DbContext or connection string. |
| `tools/AcceptanceRunner` | Optional console tool that runs T01–T06 and writes evidence. |

## Before → after boundary
*Before (Lab 4, per the Lab 5 handout):* the desktop application used the application layer and EF Core/PostgreSQL infrastructure directly.
*After:* the desktop calls HTTP only. Business coordination, repository abstractions and EF Core/PostgreSQL persistence sit behind the API. (Fill in the exact Lab 4 classes you moved in `docs/baseline-record.md`.)

## Lifetimes (dependency injection)
- `AppDbContext`, repositories, `EfUnitOfWork`, services: **scoped** (one per HTTP request, never shared).
- `IClock`, `BorrowingRules`, `ToggleFaultInjector`: singletons (stateless or thread-safe).
- Desktop: one `HttpClient` + `ApiClient`, view-models and the shared `StatusViewModel` as singletons.

## Atomic checkout (UC-06) and stale data (BR-15)
`BorrowingService.IssueAsync` runs everything in one PostgreSQL transaction (`EfUnitOfWork.InTransactionAsync`):
1. validate input (400) → 2. lock the student row, then the equipment rows ordered by id (`SELECT … FOR UPDATE`, fixed order = no deadlocks) →
3. read **fresh** rows → 4. check existence (404), eligibility, availability and limit (409) →
5. write each borrowing and flip availability; the fault seam can throw after the first row →
6. commit. Any exception rolls the whole transaction back: no partial rows.

Database safety nets that hold even if two requests race: unique partial index `IX_Borrowings_EquipmentId_Active` (an item has at most one active borrowing), unique normalised student number / equipment code, check constraints on dates, `Restrict` foreign keys (history can never be orphaned). Violations are translated to 409 in `EfUnitOfWork.Translate`.

## Failure model
| Situation | What the API/desktop does |
|---|---|
| API process not running | **No HTTP response.** `ApiClient` raises `ApiUnreachableException`; UI: "CONNECTION PROBLEM … Nothing was changed." |
| Connection dropped / timeout before a mutation's response | `ApiNoResponseException(WasMutation)`; UI: "UNKNOWN OUTCOME … NOT retried", then the lists are reloaded for the user to check. |
| API up, database unreachable / bad connection | HTTP **500** `database_unavailable` (safe body). UI: "SERVER ERROR (HTTP 500 …) the API is reachable…". |
| Lost COMMIT acknowledgement inside the API | HTTP 500 `commit_outcome_unknown` → same unknown-outcome UI path; never auto-retried. |
| Business/validation failure | 400 / 404 / 409 with a readable message; 404/409 also trigger a refresh of stale lists. |
| Any other exception | 500 `server_error`; details only in the server log. No stack trace, SQL or credentials in bodies. |

## Contract notes
`docs/openapi.yaml` and `src/EquipmentBorrowing.Api/openapi/openapi.json` describe API-01–16 (methods, routes, schemas, status codes, `x-api-id`). The API serves them at `/openapi/v1.json` and `/openapi/v1.yaml`. The OpenAPI file is hand-maintained alongside the code: if you change an endpoint, update it too (T01 step 4 compares them).
