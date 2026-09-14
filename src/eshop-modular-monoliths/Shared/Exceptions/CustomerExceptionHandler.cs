using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Shared.Exceptions;

public class CustomerExceptionHandler(ILogger<CustomerExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            "Error Message: {exceptionMessage}, Time of occurence {time}",
            exception.Message, DateTime.UtcNow);
        (string Detail, string Title, int StatusCode) details = exception switch
        {
            ProductNotFoundException => (
                exception.Message, exception.GetType().Name, StatusCodes.Status404NotFound),
            
            ValidationException => (
                exception.Message, exception.GetType().Name, StatusCodes.Status400BadRequest),
            
            _=> (
                exception.Message, exception.GetType().Name, StatusCodes.Status500InternalServerError)
        };

        var problemDetails = new ProblemDetails
        {
            Title = details.Title,
            Detail = details.Detail,
            Status = details.StatusCode,
            Instance = httpContext.Request.Path
        };
        
        problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
        }

        httpContext.Response.StatusCode = details.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}