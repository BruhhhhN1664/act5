using CommunityToolkit.Mvvm.ComponentModel;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop.Services;

/// <summary>Lets view-models trigger "reload everything" without referencing each other.</summary>
public sealed class RefreshCoordinator
{
    public Func<bool, Task>? RefreshAll { get; set; }
    public Task RefreshAsync(bool quiet = true) => RefreshAll?.Invoke(quiet) ?? Task.CompletedTask;
}

/// <summary>
/// Runs one asynchronous API operation: shows loading, turns every failure kind into a distinct,
/// understandable message, and refreshes state when the data may be stale. The UI thread is never blocked.
/// </summary>
public sealed class OperationRunner(StatusViewModel status, ApiClient api, RefreshCoordinator refresh)
{
    public async Task<bool> RunAsync(Func<Task> action, string busyText, bool clearMessages = true)
    {
        if (clearMessages) status.Clear();
        status.BeginBusy(busyText);
        var ok = false;
        var refreshAfter = false;
        try
        {
            await action();
            ok = true;
        }
        catch (ApiUnreachableException)
        {
            // No HTTP response and the request was never delivered.
            status.Error = $"CONNECTION PROBLEM: cannot reach the API at {api.BaseAddress}. " +
                           "Check that the API is running and the base URL is correct. Nothing was changed.";
        }
        catch (ApiNoResponseException ex)
        {
            if (ex.WasMutation)
            {
                status.Error = "UNKNOWN OUTCOME: the connection was lost before the API answered, so the change may or may not " +
                               "have been saved. It was NOT retried. The lists below were reloaded — check them before trying again.";
                refreshAfter = true;
            }
            else
            {
                status.Error = "CONNECTION PROBLEM: the API did not answer (timeout or dropped connection). Try Reload.";
            }
        }
        catch (ApiErrorException ex)
        {
            if (ex.IsUnknownOutcome)
            {
                status.Error = $"UNKNOWN OUTCOME (server): {ex.ServerMessage} The lists were reloaded — check them before retrying.";
                refreshAfter = true;
            }
            else if (ex.IsServerError)
            {
                // The API IS reachable and answered with a failure: different from "cannot connect".
                status.Error = $"SERVER ERROR (HTTP {ex.Status}, {ex.Code}): {ex.ServerMessage} " +
                               "The API is reachable but could not complete the request; nothing is shown as saved.";
            }
            else
            {
                var prefix = ex.Status switch
                {
                    400 => "Please correct the input: ",
                    404 => "That record no longer exists: ",
                    409 => "Not allowed: ",
                    _ => $"Request rejected (HTTP {ex.Status}): "
                };
                status.Error = prefix + ex.ServerMessage;
                refreshAfter = ex.Status is 404 or 409; // stale data -> show the current state
            }
        }
        catch (OperationCanceledException)
        {
            // cancelled by the app; no message
        }
        catch (Exception ex)
        {
            status.Error = $"Unexpected application error: {ex.Message}";
        }
        finally
        {
            status.EndBusy();
        }

        if (refreshAfter) await refresh.RefreshAsync(quiet: true);
        return ok;
    }
}
