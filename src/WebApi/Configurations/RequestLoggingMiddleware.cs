using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Utils;
using Microsoft.Extensions.Primitives;

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
        HttpRequest request = context.Request;
        string? userId = context.User.GetUserId();
        StringValues referer = request.Headers.Referer;

        request.EnableBuffering();
        string body;

        if (request is { HasFormContentType: true, Form.Files.Count: > 0 })
        {
            body = "file";
        }
        else
        {
            using StreamReader reader = new(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        ApiRequestLog logEntry = new()
        {
            UserId = userId,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            Body = body,
            Referer = referer,
            TimeStamp = DateTime.UtcNow
        };

        using IServiceScope scope = context.RequestServices.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ApiRequestLogs.Add(logEntry);
        await dbContext.SaveChangesAsync();

        await _next(context);
    }
}