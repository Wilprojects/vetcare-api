var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
