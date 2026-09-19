using Microsoft.Extensions.Logging;
using VetCare.Application.Appointments.Models;
using VetCare.Application.Appointments.Repositories;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.Common.Security;
using VetCare.Application.Pets.Repositories;
using VetCare.Application.VeterinaryServices.Repositories;
using VetCare.Domain.Common;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.Application.Appointments;

public sealed class AppointmentService(
    IAppointmentRepository appointmentRepository,
    IPetRepository petRepository,
    IVeterinaryServiceRepository veterinaryServiceRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    ILogger<AppointmentService> logger)
    : IAppointmentService
{
    public async Task<PagedResult<AppointmentResult>> GetMyAppointmentsAsync(AppointmentSearchInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var normalizedInput = NormalizeSearchInput(input);

        ValidateSearchInput(normalizedInput);

        var page = await appointmentRepository.GetPagedForOwnerAsync(ownerId, normalizedInput, cancellationToken);

        return MapPage(page);
    }

    public async Task<AppointmentResult> GetMyAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var appointment = await appointmentRepository.GetByIdForOwnerAsync(appointmentId, ownerId, cancellationToken);

        if (appointment is null)
        {
            throw AppointmentNotFound();
        }

        return Map(appointment);
    }

    public async Task<AppointmentResult> CreateAsync(CreateAppointmentInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var pet = await petRepository.GetByIdAsync(input.PetId, ownerId, cancellationToken);

        if (pet is null)
        {
            throw new NotFoundException("PET_NOT_FOUND", "La mascota no existe.");
        }

        if (!pet.IsActive)
        {
            throw new ConflictException("PET_INACTIVE", "No se puede crear una cita para una mascota inactiva.");
        }

        var veterinaryService = await veterinaryServiceRepository
                .GetByIdAsync(input.VeterinaryServiceId, cancellationToken);

        if (veterinaryService is null)
        {
            throw new NotFoundException("VETERINARY_SERVICE_NOT_FOUND", "El servicio veterinario no existe.");
        }

        if (!veterinaryService.IsActive)
        {
            throw new ConflictException("VETERINARY_SERVICE_INACTIVE", "El servicio veterinario no está disponible.");
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        Appointment appointment;

        try
        {
            appointment = Appointment.Create(
                    pet.Id,
                    veterinaryService.Id,
                    input.ScheduledStartUtc,
                    veterinaryService.DurationMinutes,
                    veterinaryService.Price,
                    input.Reason,
                    nowUtc);
        }
        catch (DomainException exception)
        {
            throw MapDomainException(exception);
        }

        var overlaps = await appointmentRepository.HasOverlapAsync(
                appointment.ScheduledStartUtc,
                appointment.ScheduledEndUtc,
                cancellationToken: cancellationToken);

        if (overlaps)
        {
            logger.LogWarning("Appointment conflict detected for start {ScheduledStartUtc} and end {ScheduledEndUtc}",
                appointment.ScheduledStartUtc,
                appointment.ScheduledEndUtc);

            throw new ConflictException("APPOINTMENT_TIME_CONFLICT", "Ya existe una cita activa que se superpone con el horario solicitado.");
        }

        await appointmentRepository.AddAsync(appointment, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Appointment {AppointmentId} created by user {UserId} for pet {PetId}",
            appointment.Id,
            ownerId,
            appointment.PetId);

        return Map(appointment);
    }

    public async Task<AppointmentResult> RescheduleAsync(Guid appointmentId, RescheduleAppointmentInput input, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var appointment = await appointmentRepository.GetByIdForOwnerAsync(
                    appointmentId,
                    ownerId,
                    cancellationToken);

        if (appointment is null)
        {
            throw AppointmentNotFound();
        }

        var veterinaryService = await veterinaryServiceRepository
                .GetByIdAsync(
                    appointment.VeterinaryServiceId,
                    cancellationToken);

        if (veterinaryService is null)
        {
            throw new NotFoundException("VETERINARY_SERVICE_NOT_FOUND", "El servicio veterinario asociado no existe.");
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            appointment.Reschedule(
                input.ScheduledStartUtc,
                veterinaryService.DurationMinutes,
                input.Reason,
                nowUtc);
        }
        catch (DomainException exception)
        {
            throw MapDomainException(exception);
        }

        var overlaps = await appointmentRepository.HasOverlapAsync(
                appointment.ScheduledStartUtc,
                appointment.ScheduledEndUtc,
                appointment.Id,
                cancellationToken);

        if (overlaps)
        {
            throw new ConflictException("APPOINTMENT_TIME_CONFLICT", "Ya existe una cita activa que se superpone con el nuevo horario.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Appointment {AppointmentId} rescheduled by user {UserId}", appointment.Id, ownerId);

        return Map(appointment);
    }

    public async Task CancelAsync(Guid appointmentId, string? cancellationReason, CancellationToken cancellationToken = default)
    {
        var ownerId = GetRequiredUserId();

        var appointment = await appointmentRepository.GetByIdForOwnerAsync(
                    appointmentId,
                    ownerId,
                    cancellationToken);

        if (appointment is null)
        {
            throw AppointmentNotFound();
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            appointment.Cancel(cancellationReason, nowUtc);
        }
        catch (DomainException exception)
        {
            throw MapDomainException(exception);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Appointment {AppointmentId} cancelled by user {UserId}", appointment.Id, ownerId);
    }

    public async Task<PagedResult<AppointmentResult>> GetAdminAppointmentsAsync(AppointmentSearchInput input, CancellationToken cancellationToken = default)
    {
        var normalizedInput = NormalizeSearchInput(input);

        ValidateSearchInput(normalizedInput);

        var page = await appointmentRepository.GetPagedAsync(normalizedInput, cancellationToken);

        return MapPage(page);
    }

    public async Task<AppointmentResult> UpdateStatusAsync(Guid appointmentId, AppointmentStatus status, string? cancellationReason, CancellationToken cancellationToken = default)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);

        if (appointment is null)
        {
            throw AppointmentNotFound();
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            switch (status)
            {
                case AppointmentStatus.Confirmed:
                    appointment.Confirm(nowUtc);
                    break;

                case AppointmentStatus.Completed:
                    appointment.Complete(nowUtc);
                    break;

                case AppointmentStatus.Cancelled:
                    appointment.Cancel(cancellationReason, nowUtc);
                    break;

                default:
                    throw new AppValidationException("APPOINTMENT_STATUS_INVALID", "El estado solicitado no es válido para esta operación.",
                        new Dictionary<string, string[]>
                        {
                            ["status"] = ["El estado debe ser Confirmed, Completed o Cancelled."]
                        });
            }
        }
        catch (DomainException exception)
        {
            throw MapDomainException(exception);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Appointment {AppointmentId} changed to status {AppointmentStatus}", appointment.Id, appointment.Status);

        return Map(appointment);
    }

    private Guid GetRequiredUserId()
    {
        if (currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedException("INVALID_AUTHENTICATED_USER", "No fue posible determinar el usuario autenticado.");
        }

        return userId;
    }

    private static AppointmentSearchInput NormalizeSearchInput(AppointmentSearchInput input)
    {
        return input with
        {
            SortBy = input.SortBy.Trim().ToLowerInvariant(),

            SortDirection = input.SortDirection.Trim().ToLowerInvariant()
        };
    }

    private static void ValidateSearchInput(AppointmentSearchInput input)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (input.PageNumber < 1)
        {
            errors["pageNumber"] =
            [
                "El número de página debe ser mayor o igual a 1."
            ];
        }

        if (input.PageSize is < 1 or > 100)
        {
            errors["pageSize"] =
            [
                "El tamaño de página debe estar entre 1 y 100."
            ];
        }

        if (input.Status.HasValue && !Enum.IsDefined(input.Status.Value))
        {
            errors["status"] =
            [
                "El estado de la cita no es válido."
            ];
        }

        if (input.FromUtc.HasValue && input.FromUtc.Value.Kind != DateTimeKind.Utc)
        {
            errors["fromUtc"] =
            [
                "fromUtc debe estar expresado en UTC."
            ];
        }

        if (input.ToUtc.HasValue && input.ToUtc.Value.Kind != DateTimeKind.Utc)
        {
            errors["toUtc"] =
            [
                "toUtc debe estar expresado en UTC."
            ];
        }

        if (input.FromUtc.HasValue && input.ToUtc.HasValue && input.FromUtc.Value > input.ToUtc.Value)
        {
            errors["dateRange"] =
            [
                "fromUtc no puede ser posterior a toUtc."
            ];
        }

        if (input.SortBy is not ("scheduledstart" or "createdat"))
        {
            errors["sortBy"] =
            [
                "sortBy debe ser scheduledStart o createdAt."
            ];
        }

        if (input.SortDirection is not ("asc" or "desc"))
        {
            errors["sortDirection"] =
            [
                "sortDirection debe ser asc o desc."
            ];
        }

        if (errors.Count > 0)
        {
            throw new AppValidationException("APPOINTMENT_QUERY_INVALID", "Los parámetros de consulta de citas no son válidos.", errors);
        }
    }

    private static AppointmentResult Map(Appointment appointment)
    {
        return new AppointmentResult(
            appointment.Id,
            appointment.PetId,
            appointment.VeterinaryServiceId,
            appointment.ScheduledStartUtc,
            appointment.ScheduledEndUtc,
            appointment.Price,
            appointment.Status,
            appointment.Reason,
            appointment.CancellationReason,
            appointment.CreatedAtUtc,
            appointment.UpdatedAtUtc);
    }

    private static PagedResult<AppointmentResult> MapPage(PagedResult<Appointment> page)
    {
        return new PagedResult<AppointmentResult>(
            page.Items.Select(Map).ToArray(),
            page.PageNumber,
            page.PageSize,
            page.TotalCount);
    }

    private static NotFoundException AppointmentNotFound()
    {
        return new NotFoundException("APPOINTMENT_NOT_FOUND", "La cita no existe.");
    }

    private static AppException MapDomainException(DomainException exception)
    {
        if (exception.Code == DomainErrorCodes.AppointmentInvalidStatusTransition)
        {
            return new ConflictException(exception.Code, exception.Message);
        }

        return new BusinessRuleException(exception.Code, exception.Message);
    }
}
