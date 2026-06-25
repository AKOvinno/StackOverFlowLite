namespace StackOverflowLite.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.") { }
}

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("You do not have permission to perform this action.") { }
}

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
