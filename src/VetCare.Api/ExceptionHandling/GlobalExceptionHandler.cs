using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VetCare.Application.Common.Exceptions;

namespace VetCare.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is AppValidationException validationException)
        {
            return await HandleValidationExceptionAsync(httpContext, validationException);
        }

        var error = MapException(exception);

        if (error.StatusCode >=
            StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning("Request failed with {StatusCode}. ErrorCode: {ErrorCode}. TraceId: {TraceId}", error.StatusCode, error.ErrorCode, httpContext.TraceIdentifier);
        }

        httpContext.Response.StatusCode = error.StatusCode;

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Type = error.Type,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.ErrorCode;

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }

    private async ValueTask<bool> HandleValidationExceptionAsync(HttpContext httpContext, AppValidationException exception)
    {
        logger.LogWarning("Validation failed. ErrorCode: {ErrorCode}. TraceId: {TraceId}", exception.Code, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new HttpValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Error de validación",
            Detail = exception.Message,
            Type = "https://vetcare/errors/validation",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = exception.Code;

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }

    private static ErrorDetails MapException(Exception exception)
    {
        return exception switch
        {
            UnauthorizedException ex => new ErrorDetails(
                StatusCodes.Status401Unauthorized,
                "No autorizado",
                ex.Message,
                ex.Code,
                "https://vetcare/errors/unauthorized"),

            NotFoundException ex => new ErrorDetails(
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                ex.Message,
                ex.Code,
                "https://vetcare/errors/not-found"),

            ConflictException ex => new ErrorDetails(
                StatusCodes.Status409Conflict,
                "Conflicto",
                ex.Message,
                ex.Code,
                "https://vetcare/errors/conflict"),

            ForbiddenException ex => new ErrorDetails(
                StatusCodes.Status403Forbidden,
                "Acceso denegado",
                ex.Message,
                ex.Code,
                "https://vetcare/errors/forbidden"),

            BusinessRuleException ex => new ErrorDetails(
                StatusCodes.Status400BadRequest,
                "Regla de negocio no válida",
                ex.Message,
                ex.Code,
                "https://vetcare/errors/business-rule"),

            _ => new ErrorDetails(
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado al procesar la solicitud.",
                "INTERNAL_SERVER_ERROR",
                "https://vetcare/errors/internal-server-error")
        };
    }

    private sealed record ErrorDetails(
        int StatusCode,
        string Title,
        string Detail,
        string ErrorCode,
        string Type
    );
}
