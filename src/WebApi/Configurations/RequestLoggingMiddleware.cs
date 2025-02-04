using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Utils;

namespace WebApi.Configurations;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var request = context.Request;
        var userId = context.User.GetUserId();
        var referer = request.Headers.Referer;

        request.EnableBuffering();
        string body;
        if (request is { HasFormContentType: true, Form.Files.Count: > 0 })
        {
            body = "file";
        }
        else
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var logEntry = new ApiRequestLog
        {
            UserId = userId,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            Body = body,
            Referer = referer,
            TimeStamp = DateTime.UtcNow
        };

        using var scope = context.RequestServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ApiRequestLogs.Add(logEntry);
        await dbContext.SaveChangesAsync();

        await _next(context);
    }
}