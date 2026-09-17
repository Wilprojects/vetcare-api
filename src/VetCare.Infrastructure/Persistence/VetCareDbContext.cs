using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetCare.Application.Common.Persistence;
using VetCare.Domain.Entities;
using VetCare.Infrastructure.Authentication;
using VetCare.Infrastructure.Persistence.Converters;

namespace VetCare.Infrastructure.Persistence;

public sealed class VetCareDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
{
    public VetCareDbContext(DbContextOptions<VetCareDbContext> options) : base(options)
    {
    }

    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<VeterinaryService> VeterinaryServices => Set<VeterinaryService>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(VetCareDbContext).Assembly);
    }
}
