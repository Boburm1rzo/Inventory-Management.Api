using InventoryApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace InventoryApp.Api.Middleware;

public class BlockedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is not null)
            {
                var user = await userManager.FindByIdAsync(userId);

                if (user?.IsBlocked == true)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        StatusCode = 403,
                        Message = "Your account has been blocked."
                    });

                    return;
                }
            }
        }

        await next(context);
    }
}
