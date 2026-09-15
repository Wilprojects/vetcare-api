using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VetCare.Application.Common.Security;
using VetCare.Domain.Entities;
using VetCare.Infrastructure.Authentication;

namespace VetCare.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly VetCareDbContext _dbContext;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        VetCareDbContext dbContext,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        TimeProvider timeProvider,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting VetCare database initialization");

        await _dbContext.Database.MigrateAsync(
            cancellationToken);

        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedVeterinaryServicesAsync(
            cancellationToken);

        _logger.LogInformation(
            "VetCare database initialization completed");
    }

    private async Task SeedRolesAsync()
    {
        string[] roles =
        [
            RoleNames.Admin,
            RoleNames.Customer
        ];

        foreach (var roleName in roles)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new IdentityRole<Guid>(roleName)
            {
                Id = Guid.NewGuid()
            };

            var result =
                await _roleManager.CreateAsync(role);

            EnsureSucceeded(
                result,
                $"crear el rol '{roleName}'");

            _logger.LogInformation(
                "Role {RoleName} created",
                roleName);
        }
    }

    private async Task SeedAdminAsync()
    {
        var email = GetRequiredSetting(
            "SeedAdmin:Email");

        var password = GetRequiredSetting(
            "SeedAdmin:Password");

        var firstName = GetRequiredSetting(
            "SeedAdmin:FirstName");

        var lastName = GetRequiredSetting(
            "SeedAdmin:LastName");

        var admin =
            await _userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            var createdAtUtc =
                _timeProvider.GetUtcNow().UtcDateTime;

            admin = new ApplicationUser(
                firstName,
                lastName,
                email,
                createdAtUtc)
            {
                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(
                    admin,
                    password);

            EnsureSucceeded(
                createResult,
                "crear el usuario administrador");

            _logger.LogInformation(
                "Administrator user {AdminUserId} created",
                admin.Id);
        }

        var isAdmin =
            await _userManager.IsInRoleAsync(
                admin,
                RoleNames.Admin);

        if (!isAdmin)
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    admin,
                    RoleNames.Admin);

            EnsureSucceeded(
                roleResult,
                "asignar el rol Admin al usuario administrador");

            _logger.LogInformation(
                "Admin role assigned to user {AdminUserId}",
                admin.Id);
        }
    }

    private async Task SeedVeterinaryServicesAsync(
        CancellationToken cancellationToken)
    {
        var existingNames =
            await _dbContext.VeterinaryServices
                .AsNoTracking()
                .Select(service => service.Name)
                .ToListAsync(cancellationToken);

        var existingNameSet =
            existingNames.ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        var definitions = new[]
        {
            new VeterinaryServiceSeed(
                "Consulta general",
                "Evaluación veterinaria general de la mascota.",
                30,
                75.00m),

            new VeterinaryServiceSeed(
                "Vacunación",
                "Aplicación y control de vacunas veterinarias.",
                30,
                60.00m),

            new VeterinaryServiceSeed(
                "Desparasitación",
                "Evaluación y tratamiento antiparasitario.",
                30,
                50.00m),

            new VeterinaryServiceSeed(
                "Control preventivo",
                "Evaluación preventiva integral de la mascota.",
                45,
                90.00m)
        };

        var createdAtUtc =
            _timeProvider.GetUtcNow().UtcDateTime;

        var servicesToCreate = definitions
            .Where(definition =>
                !existingNameSet.Contains(definition.Name))
            .Select(definition =>
                VeterinaryService.Create(
                    definition.Name,
                    definition.Description,
                    definition.DurationMinutes,
                    definition.Price,
                    createdAtUtc))
            .ToArray();

        if (servicesToCreate.Length == 0)
        {
            return;
        }

        await _dbContext.VeterinaryServices.AddRangeAsync(
            servicesToCreate,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "{ServiceCount} veterinary services created",
            servicesToCreate.Length);
    }

    private string GetRequiredSetting(string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"No se encontró la configuración obligatoria '{key}'.");
        }

        return value.Trim();
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"No fue posible {operation}. Errores: {errors}");
    }

    private sealed record VeterinaryServiceSeed(
        string Name,
        string Description,
        int DurationMinutes,
        decimal Price);
}
