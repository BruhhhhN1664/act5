using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application;

/// <summary>
/// UC-03, 04, 05, 06, 07, 08, 09 — BR-08..17, BR-19.
/// All business rules are enforced here (server side) and re-validated inside the transaction
/// against fresh, row-locked data, so stale desktop data can never produce a wrong write (BR-15).
/// </summary>
public sealed class BorrowingService(
    IStudentRepository students,
    IEquipmentRepository equipment,
    IBorrowingRepository borrowings,
    IUnitOfWork uow,
    IClock clock,
    IFaultInjector faults,
    BorrowingRules rules)
{
    // ---------- reads ----------

    public async Task<IReadOnlyList<BorrowingDto>> ListAsync(BorrowingListQuery q, CancellationToken ct)
    {
        var search = Rules.OptionalText(q.Search, "search", 100);
        var status = ParseEnum(q.Status, BorrowingStatusFilter.All, "status", "All, Active or Returned");
        var sort = ParseEnum(q.Sort, BorrowingSort.BorrowedDate, "sort", "borrowedDate or expectedReturnDate");
        var direction = q.Direction is null ? "asc" : q.Direction.Trim().ToLowerInvariant();
        if (direction is not ("asc" or "desc"))
            throw new InvalidRequestException(Rules.ValidationCode, "direction must be asc or desc.");

        var list = await borrowings.ListAsync(new BorrowingFilter(search, status, sort, direction == "desc"), ct);
        return list.Select(x => x.ToDto()).ToList();
    }

    public async Task<BorrowingDto> GetAsync(Guid id, CancellationToken ct) =>
        (await borrowings.GetAsync(id, ct) ?? throw BorrowingNotFound(id)).ToDto();

    public async Task<IReadOnlyList<BorrowingDto>> ListForStudentAsync(Guid studentId, CancellationToken ct)
    {
        if (await students.GetAsync(studentId, ct) is null)
            throw new NotFoundException("student_not_found", $"No student with id {studentId} exists.");
        return (await borrowings.ListForStudentAsync(studentId, ct)).Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<BorrowingDto>> ListForEquipmentAsync(Guid equipmentId, CancellationToken ct)
    {
        if (await equipment.GetAsync(equipmentId, ct) is null)
            throw new NotFoundException("equipment_not_found", $"No equipment with id {equipmentId} exists.");
        return (await borrowings.ListForEquipmentAsync(equipmentId, ct)).Select(x => x.ToDto()).ToList();
    }

    // ---------- writes ----------

    /// <summary>API-10 — single issue (UC-03).</summary>
    public async Task<BorrowingDto> BorrowAsync(CreateBorrowingRequest r, CancellationToken ct)
    {
        if (r.StudentId is null || r.EquipmentId is null)
            throw new InvalidRequestException(Rules.ValidationCode, "studentId and equipmentId are required.");
        var date = RequireDate(r.ExpectedReturnDate);

        var created = await IssueAsync(r.StudentId.Value, new[] { r.EquipmentId.Value }, date, ct);
        return created[0];
    }

    /// <summary>API-11 — atomic multi-item checkout (UC-06).</summary>
    public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest r, CancellationToken ct)
    {
        if (r.StudentId is null)
            throw new InvalidRequestException(Rules.ValidationCode, "studentId is required.");
        if (r.EquipmentIds is null || r.EquipmentIds.Count < 2)
            throw new InvalidRequestException(Rules.ValidationCode,
                "A multi-item checkout needs at least two equipment ids. Use POST /api/v1/borrowings for one item.");
        if (r.EquipmentIds.Distinct().Count() != r.EquipmentIds.Count)
            throw new InvalidRequestException(Rules.ValidationCode, "equipmentIds must not contain duplicates.");
        if (r.EquipmentIds.Any(g => g == Guid.Empty))
            throw new InvalidRequestException(Rules.ValidationCode, "equipmentIds contains an empty id.");
        var date = RequireDate(r.ExpectedReturnDate);

        var created = await IssueAsync(r.StudentId.Value, r.EquipmentIds, date, ct);
        return new CheckoutResponse(created);
    }

    /// <summary>API-12 — return (UC-05).</summary>
    public Task<BorrowingDto> ReturnAsync(Guid id, CancellationToken ct) =>
        uow.InTransactionAsync(async () =>
        {
            await borrowings.LockAsync(id, ct);
            var b = await borrowings.GetAsync(id, ct) ?? throw BorrowingNotFound(id);

            if (b.ReturnedDate is not null)
                throw new ConflictException("already_returned", "This borrowing has already been returned.");

            var today = clock.Today;
            if (today < b.BorrowedDate)
                throw new ConflictException("invalid_return_date",
                    "The return date cannot be earlier than the borrowed date.");

            await equipment.LockAsync(new[] { b.EquipmentId }, ct);
            b.ReturnedDate = today;        // BR-13: server decides the return date
            b.Equipment.IsAvailable = true; // BR-10: item becomes available again
            await uow.SaveChangesAsync(ct);
            return b.ToDto();              // history row is kept (BR-14)
        }, ct);

    // ---------- core ----------

    private Task<List<BorrowingDto>> IssueAsync(
        Guid studentId, IReadOnlyList<Guid> equipmentIds, DateOnly expectedReturn, CancellationToken ct)
    {
        var today = clock.Today;
        if (expectedReturn <= today)
            throw new InvalidRequestException("invalid_return_date",
                $"expectedReturnDate must be later than today ({today:yyyy-MM-dd}).");

        return uow.InTransactionAsync(async () =>
        {
            // Lock in a fixed order (student, then equipment ordered by id) to avoid deadlocks,
            // then read fresh rows: this is the stale-data revalidation of BR-15.
            await students.LockAsync(studentId, ct);
            var student = await students.GetAsync(studentId, ct)
                          ?? throw new NotFoundException("student_not_found", $"No student with id {studentId} exists.");

            await equipment.LockAsync(equipmentIds, ct);              // lock first (ordered by id) ...
            var items = await equipment.GetManyAsync(equipmentIds, ct); // ... then read the fresh, committed rows
            var missing = equipmentIds.Where(id => items.All(i => i.Id != id)).ToList();
            if (missing.Count > 0)
                throw new NotFoundException("equipment_not_found",
                    $"No equipment with id {string.Join(", ", missing)} exists.");

            if (!student.IsEligible)
                throw new ConflictException("student_ineligible",
                    $"Student {student.StudentNumber} is not eligible to borrow equipment.");

            var unavailable = items.Where(i => !i.IsAvailable).Select(i => i.EquipmentCode).ToList();
            if (unavailable.Count > 0)
                throw new ConflictException("equipment_unavailable",
                    $"Not available: {string.Join(", ", unavailable)}.");

            var active = await borrowings.CountActiveForStudentAsync(studentId, ct);
            if (active + items.Count > rules.MaxActiveBorrowings)
                throw new ConflictException("borrowing_limit_exceeded",
                    $"Student {student.StudentNumber} has {active} active borrowing(s); " +
                    $"adding {items.Count} would exceed the limit of {rules.MaxActiveBorrowings}.");

            var created = new List<BorrowingDto>();
            var written = 0;
            foreach (var id in equipmentIds) // keep request order in the response
            {
                var item = items.First(i => i.Id == id);
                var borrowing = new Borrowing
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    Student = student,
                    EquipmentId = item.Id,
                    Equipment = item,
                    BorrowedDate = today,          // server decides the checkout date
                    ExpectedReturnDate = expectedReturn,
                    ReturnedDate = null
                };
                item.IsAvailable = false;
                borrowings.Add(borrowing);
                await uow.SaveChangesAsync(ct);    // real write attempt inside the transaction
                written++;
                faults.AfterBorrowingWritten(written); // T05 step 4 seam (no-op unless armed)
                created.Add(borrowing.ToDto());
            }
            return created; // commit happens after this lambda returns; any throw above rolls everything back
        }, ct);
    }

    // ---------- helpers ----------

    private DateOnly RequireDate(DateOnly? date) =>
        date ?? throw new InvalidRequestException("invalid_return_date",
            "expectedReturnDate is required (format YYYY-MM-DD).");

    private static NotFoundException BorrowingNotFound(Guid id) =>
        new("borrowing_not_found", $"No borrowing with id {id} exists.");

    private static T ParseEnum<T>(string? raw, T defaultValue, string field, string allowed) where T : struct, Enum
    {
        if (raw is null) return defaultValue;
        var t = raw.Trim();
        if (t.Length == 0 || int.TryParse(t, out _) || !Enum.TryParse<T>(t, ignoreCase: true, out var value))
            throw new InvalidRequestException(Rules.ValidationCode, $"Unsupported {field} '{raw}'. Use {allowed}.");
        return value;
    }
}
