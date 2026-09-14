using VetCare.Domain.Common;
using VetCare.Domain.Constants;
using VetCare.Domain.Enums;

namespace VetCare.Domain.Entities;

public sealed class Appointment : Entity
{
    private Appointment()
    {
    }

    private Appointment(Guid petId, Guid veterinaryServiceId, DateTime scheduledStartUtc, int durationMinutes, decimal price, string reason, DateTime createdAtUtc)
        : base(createdAtUtc)
    {
        var validatedPetId = ValidatePetId(petId);
        var validatedServiceId =
            ValidateVeterinaryServiceId(veterinaryServiceId);
        var normalizedReason = NormalizeReason(reason);
        var validatedPrice = ValidatePrice(price);
        var scheduledEndUtc = ValidateScheduleAndCalculateEnd(scheduledStartUtc, durationMinutes, createdAtUtc);

        PetId = validatedPetId;
        VeterinaryServiceId = validatedServiceId;
        ScheduledStartUtc = scheduledStartUtc;
        ScheduledEndUtc = scheduledEndUtc;
        Price = validatedPrice;
        Reason = normalizedReason;
        Status = AppointmentStatus.Pending;
        CancellationReason = null;
    }

    public Guid PetId { get; private set; }
    public Guid VeterinaryServiceId { get; private set; }
    public DateTime ScheduledStartUtc { get; private set; }
    public DateTime ScheduledEndUtc { get; private set; }
    public decimal Price { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? CancellationReason { get; private set; }

    public static Appointment Create(Guid petId, Guid veterinaryServiceId, DateTime scheduledStartUtc, int durationMinutes, decimal price, string reason, DateTime createdAtUtc)
    {
        return new Appointment(
            petId,
            veterinaryServiceId,
            scheduledStartUtc,
            durationMinutes,
            price,
            reason,
            createdAtUtc);
    }

    public void Reschedule(DateTime newScheduledStartUtc, int durationMinutes, string reason, DateTime updatedAtUtc)
    {
        EnsureCanBeRescheduled();

        var normalizedReason = NormalizeReason(reason);
        var newScheduledEndUtc = ValidateScheduleAndCalculateEnd(newScheduledStartUtc, durationMinutes, updatedAtUtc);

        MarkAsUpdated(updatedAtUtc);

        ScheduledStartUtc = newScheduledStartUtc;
        ScheduledEndUtc = newScheduledEndUtc;
        Reason = normalizedReason;
    }

    public void Confirm(DateTime updatedAtUtc)
    {
        TransitionTo(AppointmentStatus.Confirmed, updatedAtUtc);
    }

    public void Complete(DateTime updatedAtUtc)
    {
        TransitionTo(AppointmentStatus.Completed, updatedAtUtc);
    }

    public void Cancel(string? cancellationReason, DateTime updatedAtUtc)
    {
        EnsureCanBeCancelled();

        var normalizedCancellationReason = NormalizeCancellationReason(cancellationReason);

        MarkAsUpdated(updatedAtUtc);

        Status = AppointmentStatus.Cancelled;
        CancellationReason = normalizedCancellationReason;
    }

    private void TransitionTo(
        AppointmentStatus newStatus,
        DateTime updatedAtUtc)
    {
        var isAllowed = (Status, newStatus) switch
        {
            (AppointmentStatus.Pending, AppointmentStatus.Confirmed) => true,

            (AppointmentStatus.Confirmed, AppointmentStatus.Completed) => true,

            _ => false
        };

        if (!isAllowed)
        {
            throw new DomainException(DomainErrorCodes.AppointmentInvalidStatusTransition, $"No se puede cambiar una cita de '{Status}' a '{newStatus}'.");
        }

        MarkAsUpdated(updatedAtUtc);
        Status = newStatus;
    }

    private void EnsureCanBeRescheduled()
    {
        if (Status != AppointmentStatus.Pending &&
            Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(DomainErrorCodes.AppointmentInvalidStatusTransition, $"Una cita en estado '{Status}' no puede reprogramarse.");
        }
    }

    private void EnsureCanBeCancelled()
    {
        if (Status != AppointmentStatus.Pending &&
            Status != AppointmentStatus.Confirmed)
        {
            throw new DomainException(DomainErrorCodes.AppointmentInvalidStatusTransition, $"Una cita en estado '{Status}' no puede cancelarse.");
        }
    }

    private static Guid ValidatePetId(Guid petId)
    {
        if (petId == Guid.Empty)
        {
            throw new DomainException(DomainErrorCodes.AppointmentPetRequired, "La cita debe estar asociada a una mascota válida.");
        }

        return petId;
    }

    private static Guid ValidateVeterinaryServiceId(
        Guid veterinaryServiceId)
    {
        if (veterinaryServiceId == Guid.Empty)
        {
            throw new DomainException(DomainErrorCodes.AppointmentServiceRequired, "La cita debe estar asociada a un servicio veterinario válido.");
        }

        return veterinaryServiceId;
    }

    private static DateTime ValidateScheduleAndCalculateEnd(DateTime scheduledStartUtc, int durationMinutes, DateTime referenceUtc)
    {
        EnsureUtc(scheduledStartUtc, nameof(scheduledStartUtc));
        EnsureUtc(referenceUtc, nameof(referenceUtc));

        if (scheduledStartUtc <= referenceUtc)
        {
            throw new DomainException(DomainErrorCodes.AppointmentStartMustBeFuture, "La fecha y hora de la cita deben estar en el futuro.");
        }

        if (durationMinutes < VeterinaryServiceConstraints.MinimumDurationMinutes || durationMinutes > VeterinaryServiceConstraints.MaximumDurationMinutes)
        {
            throw new DomainException(DomainErrorCodes.AppointmentDurationOutOfRange, $"La duración debe estar entre " + $"{VeterinaryServiceConstraints.MinimumDurationMinutes} y " + $"{VeterinaryServiceConstraints.MaximumDurationMinutes} minutos.");
        }

        return scheduledStartUtc.AddMinutes(durationMinutes);
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < VeterinaryServiceConstraints.MinimumPrice)
        {
            throw new DomainException(DomainErrorCodes.AppointmentPriceNegative, "El precio de la cita no puede ser negativo.");
        }

        return price;
    }

    private static string NormalizeReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException(DomainErrorCodes.AppointmentReasonRequired, "El motivo de la cita es obligatorio.");
        }

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > AppointmentConstraints.ReasonMaxLength)
        {
            throw new DomainException(DomainErrorCodes.AppointmentReasonTooLong, $"El motivo de la cita no puede superar " + $"{AppointmentConstraints.ReasonMaxLength} caracteres.");
        }

        return normalizedReason;
    }

    private static string? NormalizeCancellationReason(string? cancellationReason)
    {
        if (string.IsNullOrWhiteSpace(cancellationReason))
        {
            return null;
        }

        var normalizedReason = cancellationReason.Trim();

        if (normalizedReason.Length >
            AppointmentConstraints.CancellationReasonMaxLength)
        {
            throw new DomainException(
                DomainErrorCodes.AppointmentCancellationReasonTooLong, $"El motivo de cancelación no puede superar " + $"{AppointmentConstraints.CancellationReasonMaxLength} caracteres.");
        }

        return normalizedReason;
    }
}
