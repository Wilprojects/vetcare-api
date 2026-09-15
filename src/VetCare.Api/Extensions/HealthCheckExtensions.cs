using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace VetCare.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IEndpointRouteBuilder MapVetCareHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (
                    context,
                    report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(
                            entry => new
                            {
                                name = entry.Key,
                                status = entry.Value.Status.ToString(),
                                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
                            }),

                        totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2)
                    };

                    await context.Response.WriteAsJsonAsync(response);
                }
            })
            .WithName("Health")
            .WithTags("System");

        return endpoints;
    }
}
