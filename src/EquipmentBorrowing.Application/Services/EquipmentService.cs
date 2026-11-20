using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application;

/// <summary>UC-01, UC-02, UC-08, UC-15, UC-16 — BR-05..07, BR-10, BR-14, BR-16.</summary>
public sealed class EquipmentService(IEquipmentRepository equipment, IUnitOfWork uow)
{
    public async Task<IReadOnlyList<EquipmentDto>> ListAsync(bool? available, CancellationToken ct) =>
        (await equipment.ListAsync(available, ct)).Select(x => x.ToDto()).ToList();

    public async Task<EquipmentDto> GetAsync(Guid id, CancellationToken ct) =>
        (await equipment.GetAsync(id, ct) ?? throw NotFound(id)).ToDto();

    public async Task<EquipmentDto> CreateAsync(CreateEquipmentRequest r, CancellationToken ct)
    {
        var code = Rules.RequiredText(r.EquipmentCode, "equipmentCode", 50);
        var name = Rules.RequiredText(r.Name, "name", 150);
        var description = Rules.OptionalText(r.Description, "description", 500);
        var key = Normalizer.Key(code);

        if (await equipment.FindByCodeKeyAsync(key, ct) is not null) throw Duplicate(code);

        var item = new EquipmentItem
        {
            Id = Guid.NewGuid(),
            EquipmentCode = code,
            EquipmentCodeNormalized = key,
            Name = name,
            Description = description,
            IsAvailable = true // BR-06: new items start available
        };
        equipment.Add(item);
        await uow.SaveChangesAsync(ct);
        return item.ToDto();
    }

    public async Task<EquipmentDto> UpdateAsync(Guid id, UpdateEquipmentRequest r, CancellationToken ct)
    {
        var code = Rules.RequiredText(r.EquipmentCode, "equipmentCode", 50);
        var name = Rules.RequiredText(r.Name, "name", 150);
        var description = Rules.OptionalText(r.Description, "description", 500);
        var key = Normalizer.Key(code);

        var item = await equipment.GetAsync(id, ct) ?? throw NotFound(id);
        var clash = await equipment.FindByCodeKeyAsync(key, ct);
        if (clash is not null && clash.Id != id) throw Duplicate(code);

        // IsAvailable is protected (BR-07): only the borrowing workflow changes it.
        item.EquipmentCode = code;
        item.EquipmentCodeNormalized = key;
        item.Name = name;
        item.Description = description;
        await uow.SaveChangesAsync(ct);
        return item.ToDto();
    }

    private static NotFoundException NotFound(Guid id) =>
        new("equipment_not_found", $"No equipment with id {id} exists.");

    private static ConflictException Duplicate(string code) =>
        new("duplicate_equipment_code", $"Equipment with code '{code}' already exists.");
}
