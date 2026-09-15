using Microsoft.Extensions.DependencyInjection;

namespace VetCare.Infrastructure.Persistence.Seed;

public static class DatabaseSeederExtensions
{
    public static async Task SeedDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            serviceProvider.CreateAsyncScope();

        var seeder =
            scope.ServiceProvider
                .GetRequiredService<DatabaseSeeder>();

        await seeder.SeedAsync(cancellationToken);
    }
}
