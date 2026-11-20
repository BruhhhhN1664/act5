using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application;

internal static class Rules
{
    public const string ValidationCode = "validation_failed";

    public static string RequiredText(string? value, string field, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidRequestException(ValidationCode, $"{field} is required and cannot be blank.");
        var t = value.Trim();
        if (t.Length > max)
            throw new InvalidRequestException(ValidationCode, $"{field} must be at most {max} characters.");
        return t;
    }

    /// <summary>Whitespace-only counts as absent (null).</summary>
    public static string? OptionalText(string? value, string field, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var t = value.Trim();
        if (t.Length > max)
            throw new InvalidRequestException(ValidationCode, $"{field} must be at most {max} characters.");
        return t;
    }
}

internal static class Mapper
{
    public static StudentDto ToDto(this Student s) =>
        new(s.Id, s.StudentNumber, s.FullName, s.Contact, s.IsEligible);

    public static EquipmentDto ToDto(this EquipmentItem e) =>
        new(e.Id, e.EquipmentCode, e.Name, e.Description, e.IsAvailable);

    public static BorrowingDto ToDto(this Borrowing b) =>
        new(b.Id, b.StudentId, b.EquipmentId, b.BorrowedDate, b.ExpectedReturnDate, b.ReturnedDate,
            b.ReturnedDate is null ? BorrowingStatus.Active : BorrowingStatus.Returned,
            b.Student.StudentNumber, b.Student.FullName, b.Equipment.EquipmentCode, b.Equipment.Name);
}
