using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// The controlled acceptance dataset (reference Section 3: S1–S4, E1–E7).
/// Intended for a disposable database; ResetAsync deletes all rows first.
/// </summary>
public static class SeedData
{
    public static async Task ResetAsync(AppDbContext db, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Borrowings.ExecuteDeleteAsync(ct);
        await db.Equipment.ExecuteDeleteAsync(ct);
        await db.Students.ExecuteDeleteAsync(ct);
        db.ChangeTracker.Clear();

        Student St(string no, string name, bool eligible) => new()
        {
            Id = Guid.NewGuid(), StudentNumber = no, StudentNumberNormalized = Normalizer.Key(no),
            FullName = name, IsEligible = eligible
        };
        EquipmentItem Eq(string code, string name, bool available) => new()
        {
            Id = Guid.NewGuid(), EquipmentCode = code, EquipmentCodeNormalized = Normalizer.Key(code),
            Name = name, IsAvailable = available
        };
        Borrowing Bo(Student s, EquipmentItem e, DateOnly borrowed, DateOnly expected, DateOnly? returned) => new()
        {
            Id = Guid.NewGuid(), StudentId = s.Id, EquipmentId = e.Id,
            BorrowedDate = borrowed, ExpectedReturnDate = expected, ReturnedDate = returned
        };

        var s1 = St("ST-100", "Ada Cruz", true);
        var s2 = St("ST-200", "Ben Diaz", false);
        var s3 = St("ST-300", "Cara Lim", true);
        var s4 = St("ST-400", "Dani Ong", true);

        var e1 = Eq("EQ-001", "Camera", false);
        var e2 = Eq("EQ-002", "Laptop", true);
        var e3 = Eq("EQ-003", "Projector", true);
        var e4 = Eq("EQ-004", "Tablet", false);
        var e5 = Eq("EQ-005", "Microphone", false);
        var e6 = Eq("EQ-006", "Tripod", true);
        var e7 = Eq("EQ-007", "Speaker", true);

        db.Students.AddRange(s1, s2, s3, s4);
        db.Equipment.AddRange(e1, e2, e3, e4, e5, e6, e7);
        await db.SaveChangesAsync(ct);

        db.Borrowings.AddRange(
            Bo(s1, e1, new(2026, 11, 1), new(2026, 11, 9), null),                 // S1 active E1 (overdue on 2026-11-10)
            Bo(s1, e3, new(2026, 11, 2), new(2026, 11, 12), new(2026, 11, 8)),     // S1 returned E3
            Bo(s3, e4, new(2026, 11, 3), new(2026, 11, 12), null),                // S3 active E4
            Bo(s3, e5, new(2026, 11, 3), new(2026, 11, 12), null));               // S3 active E5
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        db.ChangeTracker.Clear();
    }
}
