namespace VetCare.Api.Extensions;

public static class MiddlewareExtensions
{
    public static WebApplication UseApiMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseCors(CorsPolicyNames.Frontend);
        ConfigureOpenApi(app);

        return app;
    }

    private static void ConfigureOpenApi(WebApplication app)
    {
        var openApiEnabled = app.Configuration.GetValue<bool>("OpenApi:Enabled");

        if (!openApiEnabled)
        {
            return;
        }

        app.MapOpenApi("/openapi/{documentName}.json");

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "VetCare API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "VetCare API - Swagger";
            options.DisplayRequestDuration();
        });
    }
}
