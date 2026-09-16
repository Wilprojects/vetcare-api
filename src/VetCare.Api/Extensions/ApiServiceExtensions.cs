using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VetCare.Api.ExceptionHandling;
using VetCare.Api.OpenApi;
using VetCare.Api.Security;
using VetCare.Application.Common.Security;
using VetCare.Infrastructure.Persistence;

namespace VetCare.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddProblemDetails(services);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddValidation();
        AddJsonConfiguration(services);
        AddCorsConfiguration(services, configuration);
        AddOpenApiConfiguration(services);
        AddHealthChecks(services);

        AddAuthorizationConfiguration(services);

        AddCurrentUser(services);


        return services;
    }

    private static void AddProblemDetails(IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
            };
        });
    }

    private static void AddJsonConfiguration(IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    private static void AddCorsConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyNames.Frontend,
                policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
                    }
                });
        });
    }

    private static void AddOpenApiConfiguration(IServiceCollection services)
    {
        services.AddOpenApi(
            "v1",
            options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                    {
                        document.Info.Title = "VetCare API";
                        document.Info.Version = "v1";
                        document.Info.Description = "API REST desarrollada con .NET 10 " + "para la gestión de mascotas, " + "servicios y citas veterinarias.";
                        return Task.CompletedTask;
                    });

                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

                options.AddOperationTransformer<AuthOperationTransformer>();

            });
    }

    private static void AddHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<VetCareDbContext>(name: "database", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);
    }

    private static void AddAuthorizationConfiguration(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.AuthenticatedUser, policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

            options.AddPolicy(PolicyNames.AdminOnly, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(RoleNames.Admin);
                });
        });
    }

    private static void AddCurrentUser(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUser>();
    }
}
