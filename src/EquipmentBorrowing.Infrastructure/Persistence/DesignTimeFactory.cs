using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef`. Reads the connection string from the environment so no credential is in the repo:
///   ConnectionStrings__Default="Host=localhost;Database=equipment_borrowing_dev;Username=postgres;Password=..."
/// </summary>
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? "Host=localhost;Database=equipment_borrowing_dev;Username=postgres"; // no password on purpose
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(cs).Options;
        return new AppDbContext(options);
    }
}
