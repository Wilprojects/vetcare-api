using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace VetCare.Api.OpenApi;

public sealed class AuthOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        var requiresAuthorization = metadata
            .OfType<IAuthorizeData>()
            .Any();

        var allowsAnonymous = metadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (!requiresAuthorization || allowsAnonymous || context.Document is null)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference("Bearer", context.Document)
                ] = []
            });

        return Task.CompletedTask;
    }
}
