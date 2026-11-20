namespace EquipmentBorrowing.Domain;

public sealed class Student
{
    public Guid Id { get; set; }
    public string StudentNumber { get; set; } = "";
    /// <summary>Trimmed + upper-cased key; unique in the database (BR-01).</summary>
    public string StudentNumberNormalized { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Contact { get; set; }
    public bool IsEligible { get; set; } = true;
    public List<Borrowing> Borrowings { get; set; } = new();
}

public sealed class EquipmentItem
{
    public Guid Id { get; set; }
    public string EquipmentCode { get; set; } = "";
    /// <summary>Trimmed + upper-cased key; unique in the database (BR-05).</summary>
    public string EquipmentCodeNormalized { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<Borrowing> Borrowings { get; set; } = new();
}

public sealed class Borrowing
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public Guid EquipmentId { get; set; }
    public EquipmentItem Equipment { get; set; } = null!;
    public DateOnly BorrowedDate { get; set; }
    public DateOnly ExpectedReturnDate { get; set; }
    public DateOnly? ReturnedDate { get; set; }
    public bool IsActive => ReturnedDate is null;
}

public static class Normalizer
{
    /// <summary>Case-insensitive, trimmed comparison key used for studentNumber / equipmentCode.</summary>
    public static string Key(string value) => value.Trim().ToUpperInvariant();
}
