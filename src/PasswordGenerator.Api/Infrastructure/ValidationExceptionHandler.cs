using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PasswordGenerator.Api.Application;

namespace PasswordGenerator.Api.Infrastructure;

public class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [validation.Field] = [validation.Message]
            }),
            // Corpo malformado: em Development o ASP.NET lança em vez de só responder 400
            BadHttpRequestException badRequest => new ProblemDetails { Detail = badRequest.Message },
            _ => null
        };

        if (problem is null)
            return false;

        problem.Status = StatusCodes.Status400BadRequest;
        problem.Title = "Erro de validação.";
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
