namespace VetCare.Application.Common.Exceptions;

public sealed class AppValidationException : AppException
{
    public AppValidationException(string code, string message, IReadOnlyDictionary<string, string[]> errors) : base(code, message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
