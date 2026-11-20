# API-to-Use-Case Traceability Matrix

Implementation references are real file/method names in this repository. **Evidence is not filled in on purpose:** it must point to what *you* observe when you run the tests. Replace `NOT RUN` with the actual evidence reference (for example `evidence/acceptance-run.md#t04`, a screenshot file, or a readback capture) only after executing the test.

Paths are relative to `src/EquipmentBorrowing.<Project>/` (Api, Application, Infrastructure, Desktop).

| Lab 4 use case | Applicable rules | Lab 5 endpoint | Test | Implementation reference | Evidence |
|---|---|---|---|---|---|
| UC-01 | BR-05–07,10,18–19 | API-01 | T01,T07 | `Api/Endpoints.cs` GET /equipment → `EquipmentService.ListAsync` → `EfEquipmentRepository.ListAsync`; desktop `EquipmentViewModel.LoadAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-02 | BR-10,18–19 | API-01 | T01,T07 | same, `available=true` via `Query.OptionalBool`; desktop `CheckoutViewModel.LoadAsync` (available only) | NOT RUN — see `docs/test-evidence.md` |
| UC-03 | BR-08–11,14–16,18–19 | API-10,13 | T04,T07 | `BorrowingService.BorrowAsync` / `IssueAsync`; `Endpoints` POST /borrowings, GET /borrowings/{id}; desktop `CheckoutViewModel.SubmitAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-04 | BR-14,18–19 | API-14 | T01,T06,T07 | `BorrowingService.ListAsync` → `EfBorrowingRepository.ListAsync` (status=Active); desktop `BorrowingsViewModel.ShowActiveAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-05 | BR-10,13–16,18–19 | API-12,13 | T04,T07 | `BorrowingService.ReturnAsync`; desktop `BorrowingsViewModel.ReturnAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-06 | BR-08–12,14–16,18–19 | API-11 | T05,T07 | `BorrowingService.CheckoutAsync` + `IssueAsync` inside `EfUnitOfWork.InTransactionAsync`; row locks in repositories; partial unique index `IX_Borrowings_EquipmentId_Active`; fault seam `ToggleFaultInjector`; desktop `CheckoutViewModel.SubmitAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-07 | BR-14,18–19 | API-02,03,15 | T01,T06,T07 | `StudentService.ListAsync/GetAsync`, `BorrowingService.ListForStudentAsync`; desktop `StudentsViewModel.ShowHistoryAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-08 | BR-14,18–19 | API-01,07,16 | T01,T06,T07 | `EquipmentService.GetAsync`, `BorrowingService.ListForEquipmentAsync`; desktop `EquipmentViewModel.ShowHistoryAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-09 | BR-17–19 | API-14 | T06,T07 | `BorrowingService.ListAsync` (search/sort/direction parsing) → `EfBorrowingRepository.ListAsync`; desktop `BorrowingsViewModel.LoadAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-12 | BR-01–02,16,18–19 | API-04 | T02,T07 | `StudentService.CreateAsync`; unique index `IX_Students_StudentNumberNormalized`; desktop `StudentsViewModel.AddAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-13 | BR-01–03,14,16,18–19 | API-02,03,05 | T02,T07 | `StudentService.UpdateAsync`; desktop `StudentsViewModel.SaveAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-14 | BR-04,08,13–14,18–19 | API-02,03,06 | T02,T07 | `StudentService.SetEligibilityAsync`; desktop `StudentsViewModel.ToggleEligibilityAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-15 | BR-05–06,10,16,18–19 | API-08 | T03,T07 | `EquipmentService.CreateAsync`; unique index `IX_Equipment_EquipmentCodeNormalized`; desktop `EquipmentViewModel.AddAsync` | NOT RUN — see `docs/test-evidence.md` |
| UC-16 | BR-05–07,10,14,16,18–19 | API-01,07,09 | T03,T07 | `EquipmentService.UpdateAsync` (isAvailable protected; strict JSON in `Program.cs`); desktop `EquipmentViewModel.SaveAsync` | NOT RUN — see `docs/test-evidence.md` |
