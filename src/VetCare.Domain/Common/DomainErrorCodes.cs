namespace VetCare.Domain.Common;

public static class DomainErrorCodes
{
    // Auditoría y fechas.
    public const string DateTimeMustBeUtc = "DATETIME_MUST_BE_UTC";
    public const string UpdatedAtBeforeCreatedAt = "UPDATED_AT_BEFORE_CREATED_AT";

    // Mascotas.
    public const string PetOwnerRequired = "PET_OWNER_REQUIRED";
    public const string PetNameRequired = "PET_NAME_REQUIRED";
    public const string PetNameTooLong = "PET_NAME_TOO_LONG";
    public const string PetSpeciesInvalid = "PET_SPECIES_INVALID";
    public const string PetSexInvalid = "PET_SEX_INVALID";
    public const string PetBreedTooLong = "PET_BREED_TOO_LONG";
    public const string PetBirthDateInFuture = "PET_BIRTH_DATE_IN_FUTURE";
    public const string PetWeightOutOfRange = "PET_WEIGHT_OUT_OF_RANGE";

    // Servicios veterinarios.
    public const string ServiceNameRequired = "SERVICE_NAME_REQUIRED";
    public const string ServiceNameTooLong = "SERVICE_NAME_TOO_LONG";
    public const string ServiceDescriptionRequired = "SERVICE_DESCRIPTION_REQUIRED";
    public const string ServiceDescriptionTooLong = "SERVICE_DESCRIPTION_TOO_LONG";
    public const string ServiceDurationOutOfRange = "SERVICE_DURATION_OUT_OF_RANGE";
    public const string ServicePriceNegative = "SERVICE_PRICE_NEGATIVE";

    // Citas.
    public const string AppointmentPetRequired = "APPOINTMENT_PET_REQUIRED";
    public const string AppointmentServiceRequired = "APPOINTMENT_SERVICE_REQUIRED";
    public const string AppointmentStartMustBeFuture =
        "APPOINTMENT_START_MUST_BE_FUTURE";
    public const string AppointmentDurationOutOfRange =
        "APPOINTMENT_DURATION_OUT_OF_RANGE";
    public const string AppointmentPriceNegative =
        "APPOINTMENT_PRICE_NEGATIVE";
    public const string AppointmentReasonRequired =
        "APPOINTMENT_REASON_REQUIRED";
    public const string AppointmentReasonTooLong =
        "APPOINTMENT_REASON_TOO_LONG";
    public const string AppointmentCancellationReasonTooLong =
        "APPOINTMENT_CANCELLATION_REASON_TOO_LONG";
    public const string AppointmentInvalidStatusTransition =
        "APPOINTMENT_INVALID_STATUS_TRANSITION";
}
