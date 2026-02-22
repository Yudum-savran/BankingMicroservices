namespace Customer.Domain.Exceptions;

/// <summary>
/// Base exception for application-level errors
/// Should NOT be exposed directly to clients
/// </summary>
public class ApplicationException : Exception
{
    public string Code { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }

    public ApplicationException(string message, string code = "APPLICATION_ERROR", 
        Dictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception for validation failures
/// </summary>
public class ValidationException : ApplicationException
{
    public ValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR", errors)
    {
    }

    public ValidationException(string propertyName, string message)
        : base("Validation failed", "VALIDATION_ERROR", 
            new Dictionary<string, string[]> { { propertyName, new[] { message } } })
    {
    }
}

/// <summary>
/// Exception for domain business rule violations
/// </summary>
public class DomainException : ApplicationException
{
    public DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message, code)
    {
    }
}

/// <summary>
/// Exception for resource not found scenarios
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string resourceName, Guid id)
        : base($"{resourceName} with ID '{id}' was not found", "RESOURCE_NOT_FOUND")
    {
    }

    public NotFoundException(string resourceName, string identifier)
        : base($"{resourceName} '{identifier}' was not found", "RESOURCE_NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception for duplicate resource scenarios
/// </summary>
public class DuplicateException : ApplicationException
{
    public DuplicateException(string resourceName, string? identifier = null)
        : base(
            string.IsNullOrEmpty(identifier)
                ? $"Duplicate {resourceName} already exists"
                : $"A {resourceName} with identifier '{identifier}' already exists",
            "DUPLICATE_RESOURCE")
    {
    }
}

/// <summary>
/// Exception for invalid operations
/// </summary>
public class InvalidOperationException : ApplicationException
{
    public InvalidOperationException(string message, string code = "INVALID_OPERATION")
        : base(message, code)
    {
    }
}

/// <summary>
/// Exception for unauthorized access
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized")
        : base(message, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Exception for forbidden access
/// </summary>
public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message, "FORBIDDEN")
    {
    }
}
