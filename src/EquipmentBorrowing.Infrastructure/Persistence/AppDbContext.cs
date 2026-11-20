using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<EquipmentItem> Equipment => Set<EquipmentItem>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    // Constraint / index names are referenced by EfUnitOfWork to translate DB violations into HTTP 409s.
    public const string StudentNumberIndex = "IX_Students_StudentNumberNormalized";
    public const string EquipmentCodeIndex = "IX_Equipment_EquipmentCodeNormalized";
    public const string ActiveBorrowingIndex = "IX_Borrowings_EquipmentId_Active";

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Student>(e =>
        {
            e.ToTable("Students");
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.StudentNumberNormalized).HasMaxLength(50).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Contact).HasMaxLength(150);
            e.HasIndex(x => x.StudentNumberNormalized).IsUnique().HasDatabaseName(StudentNumberIndex);
        });

        mb.Entity<EquipmentItem>(e =>
        {
            e.ToTable("Equipment");
            e.HasKey(x => x.Id);
            e.Property(x => x.EquipmentCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.EquipmentCodeNormalized).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.EquipmentCodeNormalized).IsUnique().HasDatabaseName(EquipmentCodeIndex);
        });

        mb.Entity<Borrowing>(e =>
        {
            e.ToTable("Borrowings", t =>
            {
                t.HasCheckConstraint("CK_Borrowings_ExpectedAfterBorrowed", "\"ExpectedReturnDate\" > \"BorrowedDate\"");
                t.HasCheckConstraint("CK_Borrowings_ReturnedNotBeforeBorrowed",
                    "\"ReturnedDate\" IS NULL OR \"ReturnedDate\" >= \"BorrowedDate\"");
            });
            e.HasKey(x => x.Id);
            e.Ignore(x => x.IsActive);
            e.HasOne(x => x.Student).WithMany(s => s.Borrowings).HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Equipment).WithMany(q => q.Borrowings).HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.StudentId);
            // DB-level guarantee: an item can have at most ONE active (not returned) borrowing.
            e.HasIndex(x => x.EquipmentId).IsUnique().HasFilter("\"ReturnedDate\" IS NULL")
                .HasDatabaseName(ActiveBorrowingIndex);
        });
    }
}
