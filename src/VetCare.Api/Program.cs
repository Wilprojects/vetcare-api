using VetCare.Infrastructure;
using VetCare.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(
    builder.Configuration);

var seedDatabase =
    builder.Configuration.GetValue<bool>(
        "SeedDatabase");

var app = builder.Build();

if (seedDatabase)
{
    await app.Services.SeedDatabaseAsync();
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    name = "VetCare API",
    version = "v1",
    status = "Running"
}))
.WithName("GetApiInformation")
.WithTags("System");

app.Run();

public partial class Program
{
}
