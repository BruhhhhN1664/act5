using EquipmentBorrowing.Application;
using EquipmentBorrowing.Contracts;

namespace EquipmentBorrowing.Api;

public static class Endpoints
{
    public static void MapApiV1(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/api/v1");

        // ---- Equipment: API-01, 07, 08, 09, 16 ----
        v1.MapGet("/equipment", async (HttpRequest req, EquipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(Query.OptionalBool(req, "available"), ct)));                  // API-01

        v1.MapGet("/equipment/{id:guid}", async (Guid id, EquipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));                                                       // API-07

        v1.MapPost("/equipment", async (CreateEquipmentRequest body, EquipmentService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(body, ct);                                                 // API-08
            return Results.Created($"/api/v1/equipment/{created.Id}", created);
        });

        v1.MapPut("/equipment/{id:guid}", async (Guid id, UpdateEquipmentRequest body, EquipmentService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateAsync(id, body, ct)));                                              // API-09

        v1.MapGet("/equipment/{id:guid}/borrowings", async (Guid id, BorrowingService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListForEquipmentAsync(id, ct)));                                          // API-16

        // ---- Students: API-02..06, 15 ----
        v1.MapGet("/students", async (HttpRequest req, StudentService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(Query.Single(req, "search"), ct)));                             // API-02

        v1.MapGet("/students/{id:guid}", async (Guid id, StudentService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));                                                       // API-03

        v1.MapPost("/students", async (CreateStudentRequest body, StudentService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(body, ct);                                                 // API-04
            return Results.Created($"/api/v1/students/{created.Id}", created);
        });

        v1.MapPut("/students/{id:guid}", async (Guid id, UpdateStudentRequest body, StudentService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateAsync(id, body, ct)));                                              // API-05

        v1.MapPatch("/students/{id:guid}/eligibility", async (Guid id, SetEligibilityRequest body, StudentService svc, CancellationToken ct) =>
            Results.Ok(await svc.SetEligibilityAsync(id, body, ct)));                                      // API-06

        v1.MapGet("/students/{id:guid}/borrowings", async (Guid id, BorrowingService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListForStudentAsync(id, ct)));                                            // API-15

        // ---- Borrowings: API-10..14 ----
        v1.MapPost("/borrowings", async (CreateBorrowingRequest body, BorrowingService svc, CancellationToken ct) =>
        {
            var created = await svc.BorrowAsync(body, ct);                                                 // API-10
            return Results.Created($"/api/v1/borrowings/{created.Id}", created);
        });

        v1.MapPost("/checkout-requests", async (CheckoutRequest body, BorrowingService svc, CancellationToken ct) =>
        {
            var result = await svc.CheckoutAsync(body, ct);                                                // API-11
            var first = result.Borrowings.Count > 0 ? $"/api/v1/borrowings/{result.Borrowings[0].Id}" : "/api/v1/borrowings";
            return Results.Created(first, result);
        });

        v1.MapPost("/borrowings/{id:guid}/return",
            async (Guid id, ReturnRequest? body, BorrowingService svc, CancellationToken ct) =>
                Results.Ok(await svc.ReturnAsync(id, ct)));                                                // API-12 (body is {} or empty)

        v1.MapGet("/borrowings/{id:guid}", async (Guid id, BorrowingService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));                                                       // API-13

        v1.MapGet("/borrowings", async (HttpRequest req, BorrowingService svc, CancellationToken ct) =>
        {
            var q = new BorrowingListQuery(
                Query.Single(req, "search"), Query.Single(req, "status"),
                Query.Single(req, "sort"), Query.Single(req, "direction"));
            return Results.Ok(await svc.ListAsync(q, ct));                                                 // API-14
        });
    }
}

internal static class Query
{
    /// <summary>Returns the single value of a query parameter, or throws 400 if it is repeated.</summary>
    public static string? Single(HttpRequest req, string name)
    {
        if (!req.Query.TryGetValue(name, out var values)) return null;
        if (values.Count > 1)
            throw new InvalidRequestException("validation_failed", $"Query parameter '{name}' may only be given once.");
        return values[0];
    }

    public static bool? OptionalBool(HttpRequest req, string name)
    {
        var raw = Single(req, name);
        if (raw is null) return null;
        if (bool.TryParse(raw.Trim(), out var b)) return b;
        throw new InvalidRequestException("validation_failed", $"Query parameter '{name}' must be true or false.");
    }
}
