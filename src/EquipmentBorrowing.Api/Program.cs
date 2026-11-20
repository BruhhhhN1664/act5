using System.Text.Json.Serialization;
using EquipmentBorrowing.Api;
using EquipmentBorrowing.Contracts;
using EquipmentBorrowing.Infrastructure;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

// ---- configuration (no credentials in the repo: use user-secrets or ConnectionStrings__Default) ----
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
var maxActive = builder.Configuration.GetValue("Borrowing:MaxActiveBorrowings", 3);
DateOnly? fixedDate = null;
var fixedRaw = builder.Configuration["Clock:FixedDate"];
if (!string.IsNullOrWhiteSpace(fixedRaw))
    fixedDate = DateOnly.ParseExact(fixedRaw.Trim(), "yyyy-MM-dd");

builder.Services.AddPersistence(connectionString);
builder.Services.AddApplicationServices(maxActive, fixedDate);

// Strict JSON: unknown properties are rejected (400), e.g. "isAvailable" in an equipment update (API-09).
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});
// Bind failures always throw BadHttpRequestException so ErrorHandlingMiddleware can answer with the contract body.
builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

var app = builder.Build();

if (args.Length > 0 && args[0] is "migrate" or "seed" or "reset")
    return await DbCommands.RunAsync(app.Services, args[0]);

app.UseMiddleware<ErrorHandlingMiddleware>();

// Responses with no body (unknown route 404, wrong method 405, 415 ...) still get the contract error body.
app.UseStatusCodePages(async ctx =>
{
    var res = ctx.HttpContext.Response;
    var (code, message) = res.StatusCode switch
    {
        400 => ("bad_request", "The request is not valid."),
        404 => ("not_found", "The requested route or resource does not exist."),
        405 => ("method_not_allowed", "That HTTP method is not allowed for this route."),
        415 => ("unsupported_media_type", "Send the request body as application/json."),
        _ => ($"http_{res.StatusCode}", "The request could not be completed.")
    };
    await res.WriteAsJsonAsync(new ErrorResponse(code, message));
});

// Liveness only: does NOT touch the database, so it still answers when the database is down (T08).
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Machine-readable OpenAPI description of API-01..16.
app.MapGet("/openapi/v1.json", () => Results.File(Path.Combine(AppContext.BaseDirectory, "openapi", "openapi.json"), "application/json"));
app.MapGet("/openapi/v1.yaml", () => Results.File(Path.Combine(AppContext.BaseDirectory, "openapi", "openapi.yaml"), "application/yaml"));

app.MapApiV1();

if (app.Configuration.GetValue<bool>("Dev:Enabled"))
{
    app.Logger.LogWarning("Dev endpoints (/dev/*) are ENABLED. Use only with a disposable database.");
    app.MapDev();
}

app.Run();
return 0;
