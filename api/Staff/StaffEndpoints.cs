using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Staff;

public static class StaffEndpoints
{
    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/staff/me", GetCurrentStaff).RequireAuthorization();

        return app;
    }

    private static Results<Ok<StaffMeResponse>, UnauthorizedHttpResult> GetCurrentStaff(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId is null
            ? TypedResults.Unauthorized()
            : TypedResults.Ok(new StaffMeResponse(userId));
    }
}
