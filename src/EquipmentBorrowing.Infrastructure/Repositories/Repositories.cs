using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

internal static class Like
{
    /// <summary>Builds a case-insensitive "contains" pattern; escapes LIKE wildcards (backslash is PostgreSQL's default escape).</summary>
    public static string Contains(string term) =>
        "%" + term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}

public sealed class EfStudentRepository(AppDbContext db) : IStudentRepository
{
    public async Task<IReadOnlyList<Student>> ListAsync(string? search, CancellationToken ct)
    {
        IQueryable<Student> q = db.Students.AsNoTracking();
        if (search is not null)
        {
            var p = Like.Contains(search);
            q = q.Where(s => EF.Functions.ILike(s.FullName, p) || EF.Functions.ILike(s.StudentNumber, p));
        }
        return await q.OrderBy(s => s.StudentNumberNormalized).ThenBy(s => s.Id).ToListAsync(ct);
    }

    public Task<Student?> GetAsync(Guid id, CancellationToken ct) =>
        db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Student?> FindByNumberKeyAsync(string key, CancellationToken ct) =>
        db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentNumberNormalized == key, ct);

    public async Task LockAsync(Guid id, CancellationToken ct) =>
        await db.Database.ExecuteSqlAsync($"SELECT 1 FROM \"Students\" WHERE \"Id\" = {id} FOR UPDATE", ct);

    public void Add(Student student) => db.Students.Add(student);
}

public sealed class EfEquipmentRepository(AppDbContext db) : IEquipmentRepository
{
    public async Task<IReadOnlyList<EquipmentItem>> ListAsync(bool? available, CancellationToken ct)
    {
        IQueryable<EquipmentItem> q = db.Equipment.AsNoTracking();
        if (available is not null) q = q.Where(e => e.IsAvailable == available.Value);
        return await q.OrderBy(e => e.EquipmentCodeNormalized).ThenBy(e => e.Id).ToListAsync(ct);
    }

    public Task<EquipmentItem?> GetAsync(Guid id, CancellationToken ct) =>
        db.Equipment.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<EquipmentItem?> FindByCodeKeyAsync(string key, CancellationToken ct) =>
        db.Equipment.AsNoTracking().FirstOrDefaultAsync(e => e.EquipmentCodeNormalized == key, ct);

    public async Task<IReadOnlyList<EquipmentItem>> GetManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var arr = ids.ToArray();
        return await db.Equipment.Where(e => arr.Contains(e.Id)).OrderBy(e => e.Id).ToListAsync(ct);
    }

    public async Task LockAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var arr = ids.ToArray();
        // ORDER BY Id => every transaction locks rows in the same order (no deadlocks).
        await db.Database.ExecuteSqlAsync(
            $"SELECT 1 FROM \"Equipment\" WHERE \"Id\" = ANY({arr}) ORDER BY \"Id\" FOR UPDATE", ct);
        foreach (var e in db.ChangeTracker.Entries<EquipmentItem>().Where(x => arr.Contains(x.Entity.Id)).ToList())
            await e.ReloadAsync(ct); // tracked copies may be stale after waiting on the lock
    }

    public void Add(EquipmentItem item) => db.Equipment.Add(item);
}

public sealed class EfBorrowingRepository(AppDbContext db) : IBorrowingRepository
{
    private IQueryable<Borrowing> WithDetails(bool track = false)
    {
        IQueryable<Borrowing> q = db.Borrowings.Include(b => b.Student).Include(b => b.Equipment);
        return track ? q : q.AsNoTracking();
    }

    public async Task<IReadOnlyList<Borrowing>> ListAsync(BorrowingFilter f, CancellationToken ct)
    {
        var q = WithDetails();
        if (f.Search is not null)
        {
            var p = Like.Contains(f.Search);
            q = q.Where(b => EF.Functions.ILike(b.Student.FullName, p)
                             || EF.Functions.ILike(b.Student.StudentNumber, p)
                             || EF.Functions.ILike(b.Equipment.Name, p));
        }
        q = f.Status switch
        {
            BorrowingStatusFilter.Active => q.Where(b => b.ReturnedDate == null),
            BorrowingStatusFilter.Returned => q.Where(b => b.ReturnedDate != null),
            _ => q
        };

        // Stable ordering: the borrowing id breaks ties in the same direction.
        IOrderedQueryable<Borrowing> ordered = (f.Sort, f.Descending) switch
        {
            (BorrowingSort.BorrowedDate, false) => q.OrderBy(b => b.BorrowedDate).ThenBy(b => b.Id),
            (BorrowingSort.BorrowedDate, true) => q.OrderByDescending(b => b.BorrowedDate).ThenByDescending(b => b.Id),
            (BorrowingSort.ExpectedReturnDate, false) => q.OrderBy(b => b.ExpectedReturnDate).ThenBy(b => b.Id),
            _ => q.OrderByDescending(b => b.ExpectedReturnDate).ThenByDescending(b => b.Id)
        };
        return await ordered.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Borrowing>> ListForStudentAsync(Guid studentId, CancellationToken ct) =>
        await WithDetails().Where(b => b.StudentId == studentId)
            .OrderBy(b => b.BorrowedDate).ThenBy(b => b.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Borrowing>> ListForEquipmentAsync(Guid equipmentId, CancellationToken ct) =>
        await WithDetails().Where(b => b.EquipmentId == equipmentId)
            .OrderBy(b => b.BorrowedDate).ThenBy(b => b.Id).ToListAsync(ct);

    public Task<Borrowing?> GetAsync(Guid id, CancellationToken ct) =>
        WithDetails(track: true).FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task LockAsync(Guid id, CancellationToken ct) =>
        await db.Database.ExecuteSqlAsync($"SELECT 1 FROM \"Borrowings\" WHERE \"Id\" = {id} FOR UPDATE", ct);

    public Task<int> CountActiveForStudentAsync(Guid studentId, CancellationToken ct) =>
        db.Borrowings.CountAsync(b => b.StudentId == studentId && b.ReturnedDate == null, ct);

    public void Add(Borrowing borrowing) => db.Borrowings.Add(borrowing);
}
