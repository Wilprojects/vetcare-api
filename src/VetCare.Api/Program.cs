using VetCare.Api.Extensions;
using VetCare.Infrastructure;
using VetCare.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

var seedDatabase = builder.Configuration.GetValue<bool>("SeedDatabase");

var app = builder.Build();

if (seedDatabase)
{
    await app.Services.SeedDatabaseAsync();
    return;
}

app.UseApiMiddleware();

app.MapApiEndpoints();

app.Run();

public partial class Program
{
}
