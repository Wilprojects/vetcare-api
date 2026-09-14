namespace VetCare.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainException(string code, string message)
        : base(message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("El código del error de dominio es obligatorio.", nameof(code));
        }

        Code = code;
    }

    public string Code { get; }
}
