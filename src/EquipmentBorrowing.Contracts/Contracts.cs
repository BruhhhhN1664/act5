namespace EquipmentBorrowing.Contracts;

// Shared wire contracts (API-01..16). Used by the API and by the Avalonia HTTP client.
// Only these DTOs cross the HTTP boundary; no persistence type is shared with the desktop app.

public static class BorrowingStatus
{
    public const string Active = "Active";
    public const string Returned = "Returned";
}

public sealed record StudentDto(Guid Id, string StudentNumber, string FullName, string? Contact, bool IsEligible);

public sealed record EquipmentDto(Guid Id, string EquipmentCode, string Name, string? Description, bool IsAvailable);

public sealed record BorrowingDto(
    Guid Id,
    Guid StudentId,
    Guid EquipmentId,
    DateOnly BorrowedDate,
    DateOnly ExpectedReturnDate,
    DateOnly? ReturnedDate,
    string Status,
    string StudentNumber,
    string StudentName,
    string EquipmentCode,
    string EquipmentName);

// Requests: every property is nullable so the server (not the JSON binder) reports "missing" as a clean 400.
public sealed record CreateStudentRequest(string? StudentNumber, string? FullName, string? Contact);
public sealed record UpdateStudentRequest(string? StudentNumber, string? FullName, string? Contact);
public sealed record SetEligibilityRequest(bool? IsEligible);

public sealed record CreateEquipmentRequest(string? EquipmentCode, string? Name, string? Description);
public sealed record UpdateEquipmentRequest(string? EquipmentCode, string? Name, string? Description);

public sealed record CreateBorrowingRequest(Guid? StudentId, Guid? EquipmentId, DateOnly? ExpectedReturnDate);
public sealed record CheckoutRequest(Guid? StudentId, List<Guid>? EquipmentIds, DateOnly? ExpectedReturnDate);
public sealed record CheckoutResponse(IReadOnlyList<BorrowingDto> Borrowings);

public sealed record ReturnRequest();

public sealed record ErrorResponse(string Code, string Message);
