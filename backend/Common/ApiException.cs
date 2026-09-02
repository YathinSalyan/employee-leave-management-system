namespace EmployeeLeaveManagement.Common;

// A single exception type carrying an HTTP status code. Services throw this
// (or the helper subclasses below) instead of returning nulls/bools, and
// ExceptionMiddleware turns it into the matching HTTP response.
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(404, message) { }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(400, message) { }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(403, message) { }
}

public class ConflictException : ApiException
{
    public ConflictException(string message) : base(409, message) { }
}
