using VetCare.Api.Contracts.System;
using VetCare.Api.Endpoints;

namespace VetCare.Api.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/",
            () => TypedResults.Ok(new ApiInfoResponse(Name: "VetCare API", Version: "v1", Status: "Running", Documentation: "/swagger", Health: "/health")))
            .WithName("GetApiInformation")
            .WithTags("System")
            .Produces<ApiInfoResponse>(StatusCodes.Status200OK);

        app.MapVetCareHealthChecks();

        app.MapAuthEndpoints();

        app.MapPetEndpoints();

        app.MapVeterinaryServiceEndpoints();

        return app;
    }
}
