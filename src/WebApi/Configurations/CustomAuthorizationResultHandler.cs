using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Shared.Payload.Responses;

namespace WebApi.Configurations;

public class CustomAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new AuthorizationMiddlewareResultHandler();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult is { Forbidden: true, AuthorizationFailure: not null })
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            DefaultResponse response = new DefaultResponse(
                false,
                "Access forbidden. You do not have the required role for this operation.",
                StatusCodes.Status403Forbidden,
                new List<string> { "Insufficient role permissions." });

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}