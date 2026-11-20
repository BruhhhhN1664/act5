# Assumptions and items to verify against your Lab 4 Use Case Suite

The reference says the **Lab 4 Use Case Suite** (a Google Drive file) is the authoritative specification of UC-01–16 and BR-01–19. That document was not available while this solution was written, and the Lab 4 repository could not be read. The business rules below were therefore **inferred from the Lab 5 handout and reference** (Sections 2–4). Compare this table with your Lab 4 BR list and change the code where your rule differs. Each rule lives in one place.

| Rule (inferred) | Where it is implemented | Source of the inference |
|---|---|---|
| BR-01 `studentNumber` unique, compared trimmed + case-insensitive | `Normalizer.Key`, `StudentService`, index `IX_Students_StudentNumberNormalized` | T02 duplicate `" st-100 "` → 409 |
| BR-02 student number and full name required, not blank | `Rules.RequiredText` | API-04 "400 missing/blank" |
| BR-03 contact optional; whitespace = absent | `Rules.OptionalText` | Section 2 intro |
| BR-04 new students start eligible; eligibility can be changed, same value is a no-op | `StudentService.CreateAsync/SetEligibilityAsync` | API-06 |
| BR-05 `equipmentCode` unique, trimmed + case-insensitive | `EquipmentService`, index `IX_Equipment_EquipmentCodeNormalized` | T03 `" eq-001 "` → 409 |
| BR-06 code and name required; new items start available | `EquipmentService.CreateAsync` | API-08, T03 |
| BR-07 `isAvailable` is protected (not editable) | strict JSON (`UnmappedMemberHandling.Disallow`), `EquipmentService.UpdateAsync` | T03 `isAvailable` in update → 400 |
| BR-08 ineligible student cannot borrow; restricting keeps loans and history | `BorrowingService.IssueAsync` | T02 |
| **BR-09 maximum 3 active borrowings per student** | `Borrowing:MaxActiveBorrowings` (default 3) → `BorrowingRules` | T05: S3 has 2 active, +1 OK (=3), +2 rejected |
| BR-10 only available equipment can be borrowed; availability follows borrow/return | `IssueAsync`, `ReturnAsync`, partial unique index | T04 |
| BR-11 `expectedReturnDate` must be later than the server's local date; server sets checkout/return dates | `BorrowingService`, `IClock` | Section 2 closing paragraph |
| BR-12 multi-item checkout needs 2+ distinct items | `CheckoutAsync` | API-11 |
| BR-13 a borrowing can be returned only once | `ReturnAsync` → 409 `already_returned` | API-12 |
| BR-14 history is never deleted; relationships preserved (FK `Restrict`) | `AppDbContext` | T02/T03/T06 |
| BR-15 stale data re-validated server-side at write time | row locks (`SELECT … FOR UPDATE`) + fresh reads inside the transaction | Handout II.4 |
| BR-16 known failures leave no partial writes; unique/FK/check constraints in the DB | `EfUnitOfWork`, constraints in the migration | T05 |
| BR-17 search/filter/sort semantics (case-insensitive partial, default `All`/`borrowedDate`/`asc`, id tie-break) | `EfBorrowingRepository.ListAsync` | API-14 note |
| BR-18/19 persistence across restart; history lists include active + returned | PostgreSQL + `ListForStudent/ListForEquipment` | T04, T08, API-15/16 |

## Other decisions you may want to confirm
- **IDs** are GUIDs (opaque stable IDs, as the contract requires).
- **`available=false`** on API-01 returns only *unavailable* items (the contract only defines `true`).
- **Borrowing list order for histories** (API-15/16): `borrowedDate` ascending, id as tie-break.
- **Tie-break direction**: for `direction=desc` the id tie-break is also descending.
- **Local calendar convention**: the API's local calendar date is the server machine's local date. `Clock:FixedDate` overrides it for repeatable tests (2026-11-10).
- **No authentication, Docker or cloud** — none is required by the handout.
- **Unknown JSON fields → 400** for every request body (strict contract).
