using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application;

public enum BorrowingStatusFilter { All, Active, Returned }
public enum BorrowingSort { BorrowedDate, ExpectedReturnDate }

public sealed record BorrowingFilter(string? Search, BorrowingStatusFilter Status, BorrowingSort Sort, bool Descending);
public sealed record BorrowingListQuery(string? Search, string? Status, string? Sort, string? Direction);

/// <summary>Business limits (BR-09). Configured, not hard-coded in services.</summary>
public sealed record BorrowingRules(int MaxActiveBorrowings);

public interface IClock
{
    /// <summary>The application's local calendar date (checkout / return date, BR-11).</summary>
    DateOnly Today { get; }
}

/// <summary>Fault seam for the T05 rollback test. No-op unless explicitly armed in a disposable database.</summary>
public interface IFaultInjector
{
    void AfterBorrowingWritten(int writtenCount);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
    Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct);
}

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> ListAsync(string? search, CancellationToken ct);
    Task<Student?> GetAsync(Guid id, CancellationToken ct);
    Task<Student?> FindByNumberKeyAsync(string key, CancellationToken ct);
    Task LockAsync(Guid id, CancellationToken ct);
    void Add(Student student);
}

public interface IEquipmentRepository
{
    Task<IReadOnlyList<EquipmentItem>> ListAsync(bool? available, CancellationToken ct);
    Task<EquipmentItem?> GetAsync(Guid id, CancellationToken ct);
    Task<EquipmentItem?> FindByCodeKeyAsync(string key, CancellationToken ct);
    Task<IReadOnlyList<EquipmentItem>> GetManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    Task LockAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
    void Add(EquipmentItem item);
}

public interface IBorrowingRepository
{
    Task<IReadOnlyList<Borrowing>> ListAsync(BorrowingFilter filter, CancellationToken ct);
    Task<IReadOnlyList<Borrowing>> ListForStudentAsync(Guid studentId, CancellationToken ct);
    Task<IReadOnlyList<Borrowing>> ListForEquipmentAsync(Guid equipmentId, CancellationToken ct);
    Task<Borrowing?> GetAsync(Guid id, CancellationToken ct);
    Task LockAsync(Guid id, CancellationToken ct);
    Task<int> CountActiveForStudentAsync(Guid studentId, CancellationToken ct);
    void Add(Borrowing borrowing);
}
