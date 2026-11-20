using EquipmentBorrowing.Application;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Api;

/// <summary>Safety rail: destructive helpers only run against a database whose name says it is disposable.</summary>
public static class DatabaseGuard
{
    public static string EnsureDisposable(string? connectionString)
    {
        var name = new NpgsqlConnectionStringBuilder(connectionString).Database ?? "";
        var lower = name.ToLowerInvariant();
        if (!(lower.Contains("_dev") || lower.Contains("_test") || lower.Contains("disposable")))
            throw new InvalidOperationException(
                $"Refusing to reset database '{name}'. Only databases whose name contains '_dev', '_test' or 'disposable' may be reset.");
        return name;
    }
}

public static class DbCommands
{
    /// <summary>`dotnet run --project src/EquipmentBorrowing.Api -- migrate | seed | reset`</summary>
    public static async Task<int> RunAsync(IServiceProvider services, string command)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            switch (command)
            {
                case "migrate":
                    await db.Database.MigrateAsync();
                    Console.WriteLine("Migrations applied.");
                    break;
                case "seed":
                case "reset":
                    var name = DatabaseGuard.EnsureDisposable(db.Database.GetConnectionString());
                    await db.Database.MigrateAsync();
                    await SeedData.ResetAsync(db);
                    Console.WriteLine($"Database '{name}' reset to the controlled dataset (S1-S4, E1-E7).");
                    break;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Command '{command}' failed: {ex.Message}");
            return 1;
        }
    }

    /// <summary>Dev-only HTTP helpers (enabled with Dev:Enabled=true). Never enable against a shared database.</summary>
    public static void MapDev(this IEndpointRouteBuilder app)
    {
        var dev = app.MapGroup("/dev");

        dev.MapGet("/info", (IClock clock, IConfiguration cfg, ToggleFaultInjector fault) =>
        {
            var db = new NpgsqlConnectionStringBuilder(cfg.GetConnectionString("Default")).Database; // name only, no secrets
            return Results.Ok(new { today = clock.Today.ToString("yyyy-MM-dd"), database = db, faultArmed = fault.IsArmed });
        });

        dev.MapPost("/reset", async (AppDbContext db, CancellationToken ct) =>
        {
            DatabaseGuard.EnsureDisposable(db.Database.GetConnectionString());
            await SeedData.ResetAsync(db, ct);
            return Results.Ok(new { reset = true });
        });

        // One-shot: the NEXT multi-item checkout fails after its first row is written, then disarms itself.
        dev.MapPost("/fault", (bool armed, ToggleFaultInjector fault) =>
        {
            fault.Arm(armed);
            return Results.Ok(new { faultArmed = fault.IsArmed });
        });
    }
}
