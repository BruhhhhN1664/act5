using EquipmentBorrowing.Application;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Transaction boundary + translation of PostgreSQL violations into contract errors.
/// A DB failure never leaves a partial write: the transaction is rolled back and the exception rethrown.
/// </summary>
public sealed class EfUnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            throw Translate(pg, ex);
        }
    }

    public async Task<T> InTransactionAsync<T>(Func<Task<T>> work, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        T result;
        try
        {
            result = await work();
            await SaveChangesAsync(ct);
        }
        catch
        {
            try { await tx.RollbackAsync(CancellationToken.None); } catch { /* connection may already be gone */ }
            throw;
        }

        try
        {
            await tx.CommitAsync(ct);
        }
        catch (PostgresException)
        {
            throw; // the server answered: a definite failure, nothing was committed
        }
        catch (Exception)
        {
            // The acknowledgement of COMMIT was lost (socket reset, timeout, cancellation...).
            // The data may or may not be saved. Never claim success or rollback: tell the caller to verify.
            throw new OutcomeUnknownException(
                "The server lost its database connection while saving. The result is unknown: " +
                "check the current data before trying again.");
        }
        return result;
    }

    private static Exception Translate(PostgresException pg, DbUpdateException original)
    {
        switch (pg.SqlState)
        {
            case PostgresErrorCodes.UniqueViolation:
                return pg.ConstraintName switch
                {
                    AppDbContext.StudentNumberIndex =>
                        new ConflictException("duplicate_student_number", "A student with that number already exists."),
                    AppDbContext.EquipmentCodeIndex =>
                        new ConflictException("duplicate_equipment_code", "Equipment with that code already exists."),
                    AppDbContext.ActiveBorrowingIndex =>
                        new ConflictException("equipment_unavailable",
                            "That equipment was just borrowed by someone else. Refresh and try again."),
                    _ => new ConflictException("conflict", "The change conflicts with existing data.")
                };
            case PostgresErrorCodes.ForeignKeyViolation:
                return new ConflictException("reference_conflict", "A referenced record no longer exists.");
            case PostgresErrorCodes.CheckViolation:
                return new ConflictException("constraint_violation", "The change violates a data integrity rule.");
            default:
                return original; // surfaced as a safe 500 `database_error`
        }
    }
}
