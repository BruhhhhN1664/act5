using EquipmentBorrowing.Application;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// DbContext + repositories + unit of work are SCOPED (one per HTTP request).
    /// The context is never shared between requests, so concurrent requests cannot corrupt each other's tracking.
    /// The database is NOT contacted at startup, so the API can start (and report 500 database_unavailable) even with a bad connection.
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IStudentRepository, EfStudentRepository>();
        services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
        services.AddScoped<IBorrowingRepository, EfBorrowingRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, int maxActiveBorrowings, DateOnly? fixedClockDate)
    {
        services.AddSingleton<IClock>(new LocalClock(fixedClockDate));
        services.AddSingleton(new BorrowingRules(maxActiveBorrowings));
        services.AddSingleton<ToggleFaultInjector>();
        services.AddSingleton<IFaultInjector>(sp => sp.GetRequiredService<ToggleFaultInjector>());
        services.AddScoped<StudentService>();
        services.AddScoped<EquipmentService>();
        services.AddScoped<BorrowingService>();
        return services;
    }
}
