namespace EquipmentBorrowing.Desktop.Services;

public abstract class ApiException : Exception
{
    protected ApiException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>No HTTP response, and the request never reached the server (connection refused, DNS failure...). Nothing was changed.</summary>
public sealed class ApiUnreachableException : ApiException
{
    public ApiUnreachableException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// No HTTP response, but the request MAY have been delivered (timeout / connection dropped mid-flight).
/// For a mutation the outcome is UNKNOWN: never retry automatically; refresh and look.
/// </summary>
public sealed class ApiNoResponseException : ApiException
{
    public bool WasMutation { get; }
    public ApiNoResponseException(bool wasMutation, string message, Exception? inner = null) : base(message, inner) =>
        WasMutation = wasMutation;
}

/// <summary>The API answered with an HTTP error status (400/404/409/500...). A 500 here is a RECEIVED server failure, not "unreachable".</summary>
public sealed class ApiErrorException : ApiException
{
    public int Status { get; }
    public string Code { get; }
    public string ServerMessage { get; }
    public bool IsServerError => Status >= 500;
    public bool IsUnknownOutcome => Status >= 500 && Code == "commit_outcome_unknown";

    public ApiErrorException(int status, string code, string serverMessage) : base($"HTTP {status} {code}: {serverMessage}")
    {
        Status = status;
        Code = code;
        ServerMessage = serverMessage;
    }
}
