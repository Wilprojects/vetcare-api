using VetCare.Api.Contracts;
using VetCare.Api.Contracts.VeterinaryServices;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Security;
using VetCare.Application.VeterinaryServices;
using VetCare.Application.VeterinaryServices.Models;

namespace VetCare.Api.Endpoints;

public static class VeterinaryServiceEndpoints
{
    public static IEndpointRouteBuilder MapVeterinaryServiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
                .MapGroup("/api/v1/veterinary-services")
                .WithTags("Veterinary Services");

        group
            .MapGet("", GetPublicPagedAsync)
            .AllowAnonymous()
            .WithName("GetVeterinaryServices")
            .Produces<PagedResponse<VeterinaryServiceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group
            .MapGet("/{id:guid}", GetPublicByIdAsync)
            .AllowAnonymous()
            .WithName("GetVeterinaryServiceById")
            .Produces<VeterinaryServiceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPost("", CreateAsync)
            .RequireAuthorization(PolicyNames.AdminOnly)
            .WithName("CreateVeterinaryService")
            .Produces<VeterinaryServiceResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(PolicyNames.AdminOnly)
            .WithName("UpdateVeterinaryService")
            .Produces<VeterinaryServiceResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapDelete("/{id:guid}", DeactivateAsync)
            .RequireAuthorization(PolicyNames.AdminOnly)
            .WithName("DeactivateVeterinaryService")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var adminGroup = endpoints
                .MapGroup("/api/v1/admin/veterinary-services")
                .WithTags("Admin - Veterinary Services")
                .RequireAuthorization(PolicyNames.AdminOnly);

        adminGroup
            .MapGet("", GetAdminPagedAsync)
            .WithName("GetAdminVeterinaryServices")
            .Produces<PagedResponse<VeterinaryServiceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static async Task<IResult> GetPublicPagedAsync([AsParameters] VeterinaryServiceQueryParameters query, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        var result =
            await service.GetPublicPagedAsync(
                new VeterinaryServiceSearchInput(
                    query.PageNumber ?? 1,
                    query.PageSize ?? 10,
                    query.Search, IncludeInactive: false,
                    query.SortBy ?? "name",
                    query.SortDirection ?? "asc"),
                cancellationToken);

        return TypedResults.Ok(ToPagedResponse(result));
    }

    private static async Task<IResult> GetPublicByIdAsync(Guid id, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        var result = await service.GetPublicByIdAsync(id, cancellationToken);

        return TypedResults.Ok(ToResponse(result));
    }

    private static async Task<IResult> GetAdminPagedAsync([AsParameters] AdminVeterinaryServiceQueryParameters query, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        var result =
            await service.GetAdminPagedAsync(
                new VeterinaryServiceSearchInput(
                    query.PageNumber ?? 1,
                    query.PageSize ?? 10,
                    query.Search,
                    query.IncludeInactive ?? true,
                    query.SortBy ?? "name",
                    query.SortDirection ?? "asc"),
                cancellationToken);

        return TypedResults.Ok(ToPagedResponse(result));
    }

    private static async Task<IResult> CreateAsync(CreateVeterinaryServiceRequest request, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(
                new CreateVeterinaryServiceInput(
                    request.Name,
                    request.Description,
                    request.DurationMinutes,
                    request.Price),
                cancellationToken);

        return TypedResults.Created($"/api/v1/veterinary-services/{result.Id}", ToResponse(result));
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateVeterinaryServiceRequest request, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(
                id,
                new UpdateVeterinaryServiceInput(
                    request.Name,
                    request.Description,
                    request.DurationMinutes,
                    request.Price),
                cancellationToken);

        return TypedResults.Ok(ToResponse(result));
    }

    private static async Task<IResult> DeactivateAsync(Guid id, IVeterinaryServiceService service, CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);

        return TypedResults.NoContent();
    }

    private static VeterinaryServiceResponse ToResponse(VeterinaryServiceResult service)
    {
        return new VeterinaryServiceResponse(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive,
            service.CreatedAtUtc,
            service.UpdatedAtUtc);
    }

    private static PagedResponse<VeterinaryServiceResponse> ToPagedResponse(PagedResult<VeterinaryServiceResult> result)
    {
        return new PagedResponse<VeterinaryServiceResponse>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }
}
