namespace EquipmentBorrowing.Application;

public abstract class AppException : Exception
{
    public string Code { get; }
    protected AppException(string code, string message) : base(message) => Code = code;
}

/// <summary>Maps to HTTP 400.</summary>
public sealed class InvalidRequestException(string code, string message) : AppException(code, message);

/// <summary>Maps to HTTP 404.</summary>
public sealed class NotFoundException(string code, string message) : AppException(code, message);

/// <summary>Maps to HTTP 409 (business rule or uniqueness conflict).</summary>
public sealed class ConflictException(string code, string message) : AppException(code, message);

/// <summary>Commit acknowledgement was lost: the data may or may not be saved. Maps to HTTP 500 with a distinct code.</summary>
public sealed class OutcomeUnknownException(string message) : AppException("commit_outcome_unknown", message);

/// <summary>Raised only by the dev-only fault seam (disposable database).</summary>
public sealed class InjectedFaultException() : Exception("Injected test fault (disposable database only).");
