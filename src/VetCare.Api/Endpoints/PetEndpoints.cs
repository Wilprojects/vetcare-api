using VetCare.Api.Contracts;
using VetCare.Api.Contracts.Pets;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Security;
using VetCare.Application.Pets;
using VetCare.Application.Pets.Models;

namespace VetCare.Api.Endpoints;

public static class PetEndpoints
{
    public static IEndpointRouteBuilder MapPetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
                .MapGroup("/api/v1/pets")
                .WithTags("Pets")
                .RequireAuthorization(PolicyNames.CustomerOnly);

        group
            .MapGet("", GetPagedAsync)
            .WithName("GetPets")
            .Produces<PagedResponse<PetResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetPetById")
            .Produces<PetResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapPost("", CreateAsync)
            .WithName("CreatePet")
            .Produces<PetResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group
            .MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdatePet")
            .Produces<PetResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapDelete("/{id:guid}", DeactivateAsync)
            .WithName("DeactivatePet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> GetPagedAsync([AsParameters] PetQueryParameters query, IPetService petService, CancellationToken cancellationToken)
    {
        var result = await petService.GetPagedAsync(
                new PetSearchInput(
                    query.PageNumber ?? 1,
                    query.PageSize ?? 10,
                    query.Search,
                    query.Species,
                    query.IncludeInactive ?? false,
                    query.SortBy ?? "name",
                    query.SortDirection ?? "asc"),
                cancellationToken);

        return TypedResults.Ok(ToPagedResponse(result));
    }

    private static async Task<IResult> GetByIdAsync(Guid id, IPetService petService, CancellationToken cancellationToken)
    {
        var pet = await petService.GetByIdAsync(id, cancellationToken);

        return TypedResults.Ok(ToResponse(pet));
    }

    private static async Task<IResult> CreateAsync(CreatePetRequest request, IPetService petService, CancellationToken cancellationToken)
    {
        var pet = await petService.CreateAsync(
                new CreatePetInput(
                    request.Name,
                    request.Species,
                    request.Breed,
                    request.Sex,
                    request.BirthDate,
                    request.WeightKg),
                cancellationToken);

        return TypedResults.Created($"/api/v1/pets/{pet.Id}", ToResponse(pet));
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdatePetRequest request, IPetService petService, CancellationToken cancellationToken)
    {
        var pet = await petService.UpdateAsync(id,
                new UpdatePetInput(
                    request.Name,
                    request.Species,
                    request.Breed,
                    request.Sex,
                    request.BirthDate,
                    request.WeightKg),
                cancellationToken);

        return TypedResults.Ok(ToResponse(pet));
    }

    private static async Task<IResult> DeactivateAsync(Guid id, IPetService petService, CancellationToken cancellationToken)
    {
        await petService.DeactivateAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static PetResponse ToResponse(PetResult pet)
    {
        return new PetResponse(
            pet.Id,
            pet.Name,
            pet.Species,
            pet.Breed,
            pet.Sex,
            pet.BirthDate,
            pet.WeightKg,
            pet.IsActive,
            pet.CreatedAtUtc,
            pet.UpdatedAtUtc
        );
    }

    private static PagedResponse<PetResponse> ToPagedResponse(PagedResult<PetResult> result)
    {
        return new PagedResponse<PetResponse>(
            result.Items.Select(ToResponse).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage
        );
    }
}
