using Microsoft.Extensions.Logging.Abstractions;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.Common.Security;
using VetCare.Application.Pets;
using VetCare.Application.Pets.Models;
using VetCare.Application.Pets.Repositories;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.UnitTests.Application.Pets;

public sealed class PetServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 16, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_ShouldUseAuthenticatedUserAsOwner()
    {
        var userId = Guid.NewGuid();
        var repository = new FakePetRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userId, repository, unitOfWork);

        var result = await service.CreateAsync(new CreatePetInput("Luna", PetSpecies.Dog, "Labrador", PetSex.Female, new DateOnly(2022, 5, 10), 24.5m));

        Assert.NotNull(repository.AddedPet);

        Assert.Equal(userId, repository.AddedPet.OwnerId);

        Assert.Equal(result.Id, repository.AddedPet.Id);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenPetBelongsToAnotherUser()
    {
        var currentUserId = Guid.NewGuid();

        var anotherUserId = Guid.NewGuid();

        var pet = Pet.Create(anotherUserId, "Milo", PetSpecies.Cat, null, PetSex.Male, null, 5m, UtcNow);

        var repository = new FakePetRepository { PetToReturn = pet };

        var service = CreateService(currentUserId, repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(pet.Id));

        Assert.Equal("PET_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldFail_WhenPetHasFutureAppointments()
    {
        var userId = Guid.NewGuid();

        var pet = Pet.Create(userId, "Luna", PetSpecies.Dog, null, PetSex.Female, null, 20m, UtcNow);

        var repository = new FakePetRepository
        {
            PetToReturn = pet,
            HasFutureAppointments = true
        };

        var service = CreateService(userId, repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.DeactivateAsync(pet.Id));

        Assert.Equal("PET_HAS_FUTURE_APPOINTMENTS", exception.Code);

        Assert.True(pet.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivatePet_WhenNoFutureAppointmentsExist()
    {
        var userId = Guid.NewGuid();

        var pet = Pet.Create(userId, "Luna", PetSpecies.Dog, null, PetSex.Female, null, 20m, UtcNow);

        var repository = new FakePetRepository
        {
            PetToReturn = pet
        };

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(userId, repository, unitOfWork);

        await service.DeactivateAsync(pet.Id);

        Assert.False(pet.IsActive);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldRejectPageSizeGreaterThan100()
    {
        var service = CreateService(Guid.NewGuid(), new FakePetRepository(), new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<AppValidationException>(() => service.GetPagedAsync(new PetSearchInput(1, 101, null, null, false, "name", "asc")));

        Assert.Equal("PET_QUERY_INVALID", exception.Code);

        Assert.Contains("pageSize", exception.Errors.Keys);
    }

    private static PetService CreateService(Guid userId, IPetRepository repository, IUnitOfWork unitOfWork)
    {
        return new PetService(repository, unitOfWork, new FakeCurrentUser(userId), new FixedTimeProvider(UtcNow), NullLogger<PetService>.Instance);
    }

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => userId;
        public string? Email => "customer@vetcare.local";
        public IReadOnlyCollection<string> Roles => [RoleNames.Customer];
        public bool IsInRole(string role)
        {
            return string.Equals(role, RoleNames.Customer, StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        private readonly DateTimeOffset _utcNow = new(utcNow);

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakePetRepository : IPetRepository
    {
        public Pet? PetToReturn { get; init; }
        public Pet? AddedPet { get; private set; }
        public bool HasFutureAppointments { get; init; }
        public Task<Pet?> GetByIdAsync(Guid petId, Guid ownerId, CancellationToken cancellationToken = default)
        {
            if (PetToReturn is null || PetToReturn.Id != petId || PetToReturn.OwnerId != ownerId)
            {
                return Task.FromResult<Pet?>(null);
            }

            return Task.FromResult<Pet?>(PetToReturn);
        }

        public Task<PagedResult<Pet>> GetPagedAsync(Guid ownerId, PetSearchInput input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Pet>([], input.PageNumber, input.PageSize, 0));
        }

        public Task AddAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            AddedPet = pet;
            return Task.CompletedTask;
        }

        public Task<bool> HasFutureActiveAppointmentsAsync(Guid petId, DateTime referenceUtc, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HasFutureAppointments);
        }
    }
}
