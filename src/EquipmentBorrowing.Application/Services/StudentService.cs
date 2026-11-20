using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application;

/// <summary>UC-07, UC-12, UC-13, UC-14 — BR-01..04, BR-08, BR-14, BR-16.</summary>
public sealed class StudentService(IStudentRepository students, IUnitOfWork uow)
{
    private const int MaxSearch = 100;

    public async Task<IReadOnlyList<StudentDto>> ListAsync(string? search, CancellationToken ct)
    {
        var s = Rules.OptionalText(search, "search", MaxSearch);
        var list = await students.ListAsync(s, ct);
        return list.Select(x => x.ToDto()).ToList();
    }

    public async Task<StudentDto> GetAsync(Guid id, CancellationToken ct) =>
        (await students.GetAsync(id, ct) ?? throw NotFound(id)).ToDto();

    public async Task<StudentDto> CreateAsync(CreateStudentRequest r, CancellationToken ct)
    {
        var number = Rules.RequiredText(r.StudentNumber, "studentNumber", 50);
        var name = Rules.RequiredText(r.FullName, "fullName", 150);
        var contact = Rules.OptionalText(r.Contact, "contact", 150);
        var key = Normalizer.Key(number);

        if (await students.FindByNumberKeyAsync(key, ct) is not null)
            throw Duplicate(number);

        var student = new Student
        {
            Id = Guid.NewGuid(),
            StudentNumber = number,
            StudentNumberNormalized = key,
            FullName = name,
            Contact = contact,
            IsEligible = true // BR-04: new students start eligible
        };
        students.Add(student);
        await uow.SaveChangesAsync(ct); // DB unique index is the final arbiter (race-safe 409)
        return student.ToDto();
    }

    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest r, CancellationToken ct)
    {
        var number = Rules.RequiredText(r.StudentNumber, "studentNumber", 50);
        var name = Rules.RequiredText(r.FullName, "fullName", 150);
        var contact = Rules.OptionalText(r.Contact, "contact", 150);
        var key = Normalizer.Key(number);

        var student = await students.GetAsync(id, ct) ?? throw NotFound(id);
        var clash = await students.FindByNumberKeyAsync(key, ct);
        if (clash is not null && clash.Id != id) throw Duplicate(number);

        student.StudentNumber = number;
        student.StudentNumberNormalized = key;
        student.FullName = name;
        student.Contact = contact; // null clears the optional field
        await uow.SaveChangesAsync(ct);
        return student.ToDto();
    }

    public Task<StudentDto> SetEligibilityAsync(Guid id, SetEligibilityRequest r, CancellationToken ct)
    {
        if (r.IsEligible is null)
            throw new InvalidRequestException(Rules.ValidationCode, "isEligible is required and must be true or false.");
        var wanted = r.IsEligible.Value;

        return uow.InTransactionAsync(async () =>
        {
            await students.LockAsync(id, ct); // serialises against concurrent checkouts (BR-15)
            var student = await students.GetAsync(id, ct) ?? throw NotFound(id);
            if (student.IsEligible != wanted)
            {
                student.IsEligible = wanted;
                await uow.SaveChangesAsync(ct);
            }
            // Restricting never touches history or active loans (BR-08, BR-14).
            return student.ToDto();
        }, ct);
    }

    private static NotFoundException NotFound(Guid id) =>
        new("student_not_found", $"No student with id {id} exists.");

    private static ConflictException Duplicate(string number) =>
        new("duplicate_student_number", $"A student with number '{number}' already exists.");
}
