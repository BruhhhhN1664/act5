using System.Net.Sockets;
using System.Text.Json;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Contracts;
using Npgsql;

namespace EquipmentBorrowing.Api;

/// <summary>
/// Converts every failure into the contract error body { code, message }.
/// Bodies never contain connection strings, SQL, or stack traces; full details go to the server log only.
/// </summary>
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // client went away; nothing to send
        }
        catch (Exception ex)
        {
            if (ctx.Response.HasStarted) throw;

            var (status, body) = Map(ex);
            if (status >= 500)
                log.LogError(ex, "{Method} {Path} failed -> {Status} {Code}", ctx.Request.Method, ctx.Request.Path, status, body.Code);
            else
                log.LogInformation("{Method} {Path} rejected -> {Status} {Code}: {Message}",
                    ctx.Request.Method, ctx.Request.Path, status, body.Code, body.Message);

            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(body);
        }
    }

    private static (int Status, ErrorResponse Body) Map(Exception ex) => ex switch
    {
        InvalidRequestException e => (400, new ErrorResponse(e.Code, e.Message)),
        NotFoundException e => (404, new ErrorResponse(e.Code, e.Message)),
        ConflictException e => (409, new ErrorResponse(e.Code, e.Message)),
        OutcomeUnknownException e => (500, new ErrorResponse(e.Code, e.Message)),
        BadHttpRequestException or JsonException => (400, new ErrorResponse("invalid_request",
            "The request is not valid. Check the JSON syntax and that every field is present, correctly typed, and allowed.")),
        _ when Has<PostgresException>(ex) => (500, new ErrorResponse("database_error",
            "The database reported an error. No details are shown for safety; see the server log.")),
        _ when Has<NpgsqlException>(ex) || Has<SocketException>(ex) || Has<TimeoutException>(ex) => (500, new ErrorResponse("database_unavailable",
            "The API is running but cannot reach its database. Nothing was saved by this request.")),
        _ => (500, new ErrorResponse("server_error",
            "An unexpected server error occurred. If you were saving data, check the current records before retrying."))
    };

    private static bool Has<T>(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
            if (e is T) return true;
        return false;
    }
}
