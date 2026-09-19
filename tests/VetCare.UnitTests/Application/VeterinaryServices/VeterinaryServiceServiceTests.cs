using Microsoft.Extensions.Logging.Abstractions;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.VeterinaryServices;
using VetCare.Application.VeterinaryServices.Models;
using VetCare.Application.VeterinaryServices.Repositories;
using VetCare.Domain.Entities;

namespace VetCare.UnitTests.Application.VeterinaryServices;

public sealed class VeterinaryServiceServiceTests
{
    private static readonly DateTime UtcNow = new(2026, 9, 17, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_ShouldCreateActiveService()
    {
        var repository = new FakeVeterinaryServiceRepository();

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(repository, unitOfWork);

        var result = await service.CreateAsync(new CreateVeterinaryServiceInput(
                    "Ecografía",
                    "Ecografía veterinaria.",
                    45,
                    120m));

        Assert.NotNull(repository.AddedService);

        Assert.True(result.IsActive);

        Assert.Equal("Ecografía", result.Name);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenNameAlreadyExists()
    {
        var repository = new FakeVeterinaryServiceRepository { NameExists = true };

        var service = CreateService(repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(
                            new CreateVeterinaryServiceInput(
                                "Consulta general",
                                "Descripción",
                                30,
                                75m)));

        Assert.Equal("SERVICE_NAME_ALREADY_EXISTS", exception.Code);
    }

    [Fact]
    public async Task GetPublicByIdAsync_ShouldReturnNotFound_WhenServiceIsInactive()
    {
        var veterinaryService = VeterinaryService.Create(
                "Ecografía",
                "Descripción",
                45,
                120m,
                UtcNow);

        veterinaryService.Deactivate(UtcNow.AddMinutes(1));

        var repository = new FakeVeterinaryServiceRepository
        {
            ServiceToReturn = veterinaryService
        };

        var service = CreateService(repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetPublicByIdAsync(veterinaryService.Id));

        Assert.Equal("VETERINARY_SERVICE_NOT_FOUND", exception.Code);
    }

    [Fact]
    public async Task UpdateAsync_ShouldFail_WhenServiceIsInactive()
    {
        var veterinaryService = VeterinaryService.Create(
                "Ecografía",
                "Descripción",
                45,
                120m,
                UtcNow);

        veterinaryService.Deactivate(UtcNow.AddMinutes(1));

        var repository = new FakeVeterinaryServiceRepository
        {
            ServiceToReturn = veterinaryService
        };

        var service = CreateService(repository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<ConflictException>(
                    () => service.UpdateAsync(veterinaryService.Id,
                            new UpdateVeterinaryServiceInput(
                                "Ecografía",
                                "Nueva descripción",
                                60,
                                140m)));

        Assert.Equal("SERVICE_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateService()
    {
        var veterinaryService = VeterinaryService.Create(
                "Ecografía",
                "Descripción",
                45,
                120m,
                UtcNow);

        var repository = new FakeVeterinaryServiceRepository
        {
            ServiceToReturn = veterinaryService
        };

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(repository, unitOfWork);

        await service.DeactivateAsync(veterinaryService.Id);

        Assert.False(veterinaryService.IsActive);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task GetAdminPagedAsync_ShouldFail_WhenPageSizeIsGreaterThan100()
    {
        var service = CreateService(new FakeVeterinaryServiceRepository(), new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<AppValidationException>(
                    () => service.GetAdminPagedAsync(new VeterinaryServiceSearchInput(
                                1,
                                101,
                                null,
                                true,
                                "name",
                                "asc")));

        Assert.Equal("VETERINARY_SERVICE_QUERY_INVALID", exception.Code);
    }

    private static VeterinaryServiceService CreateService(IVeterinaryServiceRepository repository, IUnitOfWork unitOfWork)
    {
        return new VeterinaryServiceService(
            repository,
            unitOfWork,
            new FixedTimeProvider(UtcNow),
            NullLogger<VeterinaryServiceService>.Instance);
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
        public int SaveChangesCalls
        {
            get;
            private set;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;

            return Task.FromResult(1);
        }
    }

    private sealed class FakeVeterinaryServiceRepository : IVeterinaryServiceRepository
    {
        public VeterinaryService? ServiceToReturn
        {
            get;
            init;
        }

        public VeterinaryService? AddedService
        {
            get;
            private set;
        }

        public bool NameExists
        {
            get;
            init;
        }

        public Task<VeterinaryService?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
        {
            if (ServiceToReturn is null || ServiceToReturn.Id != serviceId)
            {
                return Task.FromResult<VeterinaryService?>(null);
            }

            return Task.FromResult<VeterinaryService?>(ServiceToReturn);
        }

        public Task<PagedResult<VeterinaryService>> GetPagedAsync(VeterinaryServiceSearchInput input, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<VeterinaryService>([], input.PageNumber, input.PageSize, 0));
        }

        public Task<bool> ExistsByNameAsync(string name, Guid? excludingServiceId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NameExists);
        }

        public Task AddAsync(VeterinaryService service, CancellationToken cancellationToken = default)
        {
            AddedService = service;
            return Task.CompletedTask;
        }
    }
}
