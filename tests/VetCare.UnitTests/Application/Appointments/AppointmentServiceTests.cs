using Microsoft.Extensions.Logging.Abstractions;
using VetCare.Application.Appointments;
using VetCare.Application.Appointments.Models;
using VetCare.Application.Appointments.Repositories;
using VetCare.Application.Common.Exceptions;
using VetCare.Application.Common.Pagination;
using VetCare.Application.Common.Persistence;
using VetCare.Application.Common.Security;
using VetCare.Application.Pets.Models;
using VetCare.Application.Pets.Repositories;
using VetCare.Application.VeterinaryServices.Models;
using VetCare.Application.VeterinaryServices.Repositories;
using VetCare.Domain.Entities;
using VetCare.Domain.Enums;

namespace VetCare.UnitTests.Application.Appointments;

public sealed class AppointmentServiceTests
{
    private static readonly DateTime UtcNow =
        new(
            2026,
            9,
            18,
            15,
            0,
            0,
            DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_ShouldCreatePendingAppointmentUsingServicePrice()
    {
        var ownerId = Guid.NewGuid();

        var pet = Pet.Create(
                ownerId,
                "Luna",
                PetSpecies.Dog,
                "Labrador",
                PetSex.Female,
                null,
                20m,
                UtcNow);

        var veterinaryService = VeterinaryService.Create(
                "Consulta general",
                "Consulta veterinaria.",
                30,
                75m,
                UtcNow);

        var appointmentRepository = new FakeAppointmentRepository();

        var unitOfWork = new FakeUnitOfWork();

        var service = CreateService(
                ownerId,
                pet,
                veterinaryService,
                appointmentRepository,
                unitOfWork);

        var result = await service.CreateAsync(new CreateAppointmentInput(
                    pet.Id,
                    veterinaryService.Id,
                    UtcNow.AddDays(1),
                    "Control general"));

        Assert.Equal(AppointmentStatus.Pending, result.Status);

        Assert.Equal(veterinaryService.Price, result.Price);

        Assert.Equal(UtcNow.AddDays(1).AddMinutes(veterinaryService.DurationMinutes), result.ScheduledEndUtc);

        Assert.NotNull(appointmentRepository.AddedAppointment);

        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenAppointmentOverlaps()
    {
        var ownerId = Guid.NewGuid();

        var pet = CreatePet(ownerId);

        var veterinaryService = CreateVeterinaryService();

        var appointmentRepository = new FakeAppointmentRepository
        {
            HasOverlap = true
        };

        var service = CreateService(ownerId, pet, veterinaryService, appointmentRepository, new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateAppointmentInput(pet.Id, veterinaryService.Id, UtcNow.AddDays(1), "Control")));

        Assert.Equal("APPOINTMENT_TIME_CONFLICT", exception.Code);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenPetIsInactive()
    {
        var ownerId = Guid.NewGuid();

        var pet = CreatePet(ownerId);

        pet.Deactivate(UtcNow.AddMinutes(1));

        var service = CreateService(
                ownerId,
                pet,
                CreateVeterinaryService(),
                new FakeAppointmentRepository(),
                new FakeUnitOfWork());

        var exception = await Assert.ThrowsAsync<
                ConflictException>(() =>
                        service.CreateAsync(
                            new CreateAppointmentInput(pet.Id, CreateVeterinaryService().Id, UtcNow.AddDays(1), "Control")));

        Assert.Equal("PET_INACTIVE", exception.Code);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancelPendingAppointment()
    {
        var ownerId = Guid.NewGuid();

        var pet = CreatePet(ownerId);

        var veterinaryService = CreateVeterinaryService();

        var appointment = Appointment.Create(
                pet.Id,
                veterinaryService.Id,
                UtcNow.AddDays(1),
                veterinaryService.DurationMinutes,
                veterinaryService.Price,
                "Control",
                UtcNow);

        var appointmentRepository = new FakeAppointmentRepository
        {
            AppointmentToReturn = appointment
        };

        var service = CreateService(
                ownerId,
                pet,
                veterinaryService,
                appointmentRepository,
                new FakeUnitOfWork());

        await service.CancelAsync(appointment.Id, "No puedo asistir.");

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldConfirmPendingAppointment()
    {
        var ownerId = Guid.NewGuid();

        var pet = CreatePet(ownerId);

        var veterinaryService = CreateVeterinaryService();

        var appointment = Appointment.Create(
                pet.Id,
                veterinaryService.Id,
                UtcNow.AddDays(1),
                veterinaryService.DurationMinutes,
                veterinaryService.Price,
                "Control",
                UtcNow);

        var repository = new FakeAppointmentRepository
        {
            AppointmentToReturn = appointment
        };

        var service = CreateService(
                ownerId,
                pet,
                veterinaryService,
                repository,
                new FakeUnitOfWork());

        var result = await service.UpdateStatusAsync(
                appointment.Id,
                AppointmentStatus.Confirmed,
                null);

        Assert.Equal(AppointmentStatus.Confirmed, result.Status);
    }

    private static Pet CreatePet(Guid ownerId)
    {
        return Pet.Create(
            ownerId,
            "Luna",
            PetSpecies.Dog,
            null,
            PetSex.Female,
            null,
            20m,
            UtcNow);
    }

    private static VeterinaryService CreateVeterinaryService()
    {
        return VeterinaryService.Create(
            "Consulta general",
            "Consulta veterinaria.",
            30,
            75m,
            UtcNow);
    }

    private static AppointmentService CreateService(
        Guid ownerId,
        Pet pet,
        VeterinaryService veterinaryService,
        FakeAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        return new AppointmentService(appointmentRepository,
            new FakePetRepository(pet),
            new FakeVeterinaryServiceRepository(veterinaryService), unitOfWork,
            new FakeCurrentUser(ownerId),
            new FixedTimeProvider(UtcNow), NullLogger<AppointmentService>.Instance);
    }

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public bool IsAuthenticated => true;
        public Guid? UserId => userId;
        public string? Email => "customer@vetcare.local";
        public IReadOnlyCollection<string> Roles => [RoleNames.Customer];

        public bool IsInRole(string role)
        {
            return role == RoleNames.Customer;
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

    private sealed class FakeAppointmentRepository : IAppointmentRepository
    {
        public Appointment? AppointmentToReturn
        {
            get;
            init;
        }

        public Appointment? AddedAppointment
        {
            get;
            private set;
        }

        public bool HasOverlap
        {
            get;
            init;
        }

        public Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AppointmentToReturn?.Id == appointmentId ? AppointmentToReturn : null);
        }

        public Task<Appointment?> GetByIdForOwnerAsync(
                Guid appointmentId,
                Guid ownerId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                AppointmentToReturn?.Id
                    == appointmentId
                    ? AppointmentToReturn
                    : null);
        }

        public Task<PagedResult<Appointment>> GetPagedForOwnerAsync(
                Guid ownerId,
                AppointmentSearchInput input,
                CancellationToken cancellationToken = default)
        {
            return EmptyPage(input);
        }

        public Task<PagedResult<Appointment>> GetPagedAsync(
                AppointmentSearchInput input,
                CancellationToken cancellationToken = default)
        {
            return EmptyPage(input);
        }

        public Task<bool> HasOverlapAsync(
            DateTime scheduledStartUtc,
            DateTime scheduledEndUtc,
            Guid? excludingAppointmentId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                HasOverlap);
        }

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            AddedAppointment = appointment;

            return Task.CompletedTask;
        }

        private static Task<PagedResult<Appointment>> EmptyPage(AppointmentSearchInput input)
        {
            return Task.FromResult(new PagedResult<Appointment>(
                    [],
                    input.PageNumber,
                    input.PageSize,
                    0));
        }
    }

    private sealed class FakePetRepository(Pet pet) : IPetRepository
    {
        public Task<Pet?> GetByIdAsync(
            Guid petId,
            Guid ownerId,
            CancellationToken cancellationToken = default)
        {
            if (pet.Id != petId || pet.OwnerId != ownerId)
            {
                return Task.FromResult<Pet?>(null);
            }

            return Task.FromResult<Pet?>(pet);
        }

        public Task<PagedResult<Pet>> GetPagedAsync(
                Guid ownerId,
                PetSearchInput input,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<Pet>(
                    [],
                    input.PageNumber,
                    input.PageSize,
                    0));
        }

        public Task AddAsync(
            Pet pet,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<bool>
            HasFutureActiveAppointmentsAsync(
                Guid petId,
                DateTime referenceUtc,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class
        FakeVeterinaryServiceRepository(
            VeterinaryService service)
        : IVeterinaryServiceRepository
    {
        public Task<VeterinaryService?>
            GetByIdAsync(
                Guid serviceId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                service.Id == serviceId
                    ? service
                    : null);
        }

        public Task<
            PagedResult<VeterinaryService>>
            GetPagedAsync(
                VeterinaryServiceSearchInput input,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new PagedResult<VeterinaryService>(
                    [],
                    input.PageNumber,
                    input.PageSize,
                    0));
        }

        public Task<bool> ExistsByNameAsync(
            string name,
            Guid? excludingServiceId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task AddAsync(
            VeterinaryService service,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
