namespace VetCare.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string code, string message) : base(message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("El código del error es obligatorio.", nameof(code));
        }

        Code = code;
    }

    public string Code { get; }
}
