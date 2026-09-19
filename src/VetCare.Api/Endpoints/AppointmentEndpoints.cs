using VetCare.Api.Contracts;
using VetCare.Api.Contracts.Appointments;
using VetCare.Application.Appointments;
using VetCare.Application.Appointments.Models;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Security;

namespace VetCare.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customerGroup = endpoints
                .MapGroup("/api/v1/appointments")
                .WithTags("Appointments")
                .RequireAuthorization(PolicyNames.CustomerOnly);

        customerGroup
            .MapGet("", GetMyAppointmentsAsync)
            .WithName("GetMyAppointments")
            .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        customerGroup
            .MapGet("/{id:guid}", GetMyAppointmentAsync)
            .WithName("GetMyAppointment")
            .Produces<AppointmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        customerGroup
            .MapPost("", CreateAsync)
            .WithName("CreateAppointment")
            .Produces<AppointmentResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        customerGroup
            .MapPut("/{id:guid}", RescheduleAsync)
            .WithName("RescheduleAppointment")
            .Produces<AppointmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        customerGroup
            .MapPatch("/{id:guid}/cancel", CancelAsync)
            .WithName("CancelAppointment")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        var adminGroup =
            endpoints
                .MapGroup("/api/v1/admin/appointments")
                .WithTags("Admin - Appointments")
                .RequireAuthorization(PolicyNames.AdminOnly);

        adminGroup
            .MapGet("", GetAdminAppointmentsAsync)
            .WithName("GetAdminAppointments")
            .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        adminGroup
            .MapPatch("/{id:guid}/status", UpdateStatusAsync)
            .WithName("UpdateAppointmentStatus")
            .Produces<AppointmentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> GetMyAppointmentsAsync(
            [AsParameters]
            AppointmentQueryParameters query,
            IAppointmentService service,
            CancellationToken cancellationToken)
    {
        var result = await service.GetMyAppointmentsAsync(ToSearchInput(query), cancellationToken);

        return TypedResults.Ok(ToPagedResponse(result));
    }

    private static async Task<IResult> GetMyAppointmentAsync(
            Guid id,
            IAppointmentService service,
            CancellationToken cancellationToken)
    {
        var result = await service.GetMyAppointmentAsync(id, cancellationToken);

        return TypedResults.Ok(ToResponse(result));
    }

    private static async Task<IResult> CreateAsync(
        CreateAppointmentRequest request,
        IAppointmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(
                new CreateAppointmentInput(
                    request.PetId!.Value,
                    request.VeterinaryServiceId!.Value,
                    request.ScheduledStartUtc!.Value,
                    request.Reason),
                cancellationToken);

        return TypedResults.Created($"/api/v1/appointments/{result.Id}", ToResponse(result));
    }

    private static async Task<IResult> RescheduleAsync(
            Guid id,
            RescheduleAppointmentRequest request,
            IAppointmentService service,
            CancellationToken cancellationToken)
    {
        var result = await service.RescheduleAsync(
                id,
                new RescheduleAppointmentInput(request.ScheduledStartUtc!.Value, request.Reason), cancellationToken);

        return TypedResults.Ok(ToResponse(result));
    }

    private static async Task<IResult> CancelAsync(
        Guid id,
        CancelAppointmentRequest request,
        IAppointmentService service,
        CancellationToken cancellationToken)
    {
        await service.CancelAsync(id, request.CancellationReason, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<IResult> GetAdminAppointmentsAsync(
            [AsParameters]
            AppointmentQueryParameters query,
            IAppointmentService service,
            CancellationToken cancellationToken)
    {
        var result = await service.GetAdminAppointmentsAsync(ToSearchInput(query), cancellationToken);

        return TypedResults.Ok(ToPagedResponse(result));
    }

    private static async Task<IResult> UpdateStatusAsync(Guid id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken cancellationToken)
    {
        var result = await service.UpdateStatusAsync(
                id,
                request.Status!.Value,
                request.CancellationReason,
                cancellationToken);

        return TypedResults.Ok(ToResponse(result));
    }

    private static AppointmentSearchInput ToSearchInput(AppointmentQueryParameters query)
    {
        return new AppointmentSearchInput(
            query.PageNumber ?? 1,
            query.PageSize ?? 10,
            query.Status,
            query.FromUtc,
            query.ToUtc,
            query.SortBy ?? "scheduledStart",
            query.SortDirection ?? "asc");
    }

    private static AppointmentResponse ToResponse(AppointmentResult appointment)
    {
        return new AppointmentResponse(
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

    private static PagedResponse<AppointmentResponse> ToPagedResponse(PagedResult<AppointmentResult> result)
    {
        return new PagedResponse<AppointmentResponse>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }
}
