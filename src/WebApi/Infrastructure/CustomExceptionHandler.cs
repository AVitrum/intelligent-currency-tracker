using System.Diagnostics;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace WebApi.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CustomExceptionHandler> _logger;

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        
        _logger.LogError(
            exception,
            "Could not process a request on machine {MachineName}. TraceId: {TraceId}",
            Environment.MachineName,
            traceId
        );

        (int statusCode, string title) = MapException(exception);

        await Results.Problem(
            title: title,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                { "traceId", traceId }
            }
        ).ExecuteAsync(httpContext);

        return true;
    }
    private static (int statusCode, string title) MapException(Exception exception)
    {
        return exception switch
        {
            DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } => 
                (409, "Duplicate key value violates unique constraint"),
            DataNotFoundException ex => (404, ex.Message),
            ImportCsvException ex => (400, ex.Message),
            ExportCsvException ex => (400, ex.Message),
            UserNotFoundException ex => (401, ex.Message),
            _ => (500, "An error occurred while processing your request")
        };
    }
}