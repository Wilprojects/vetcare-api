namespace VetCare.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
    }

    protected Entity(DateTime createdAtUtc)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        Id = Guid.NewGuid();
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    protected void MarkAsUpdated(DateTime updatedAtUtc)
    {
        EnsureUtc(updatedAtUtc, nameof(updatedAtUtc));

        if (updatedAtUtc < CreatedAtUtc)
        {
            throw new DomainException(DomainErrorCodes.UpdatedAtBeforeCreatedAt, "La fecha de actualización no puede ser anterior a la fecha de creación.");
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    protected static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainException(DomainErrorCodes.DateTimeMustBeUtc, $"El valor de '{parameterName}' debe estar expresado en UTC.");
        }
    }
}
