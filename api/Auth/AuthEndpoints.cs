using Api.Sessions;

namespace Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // Anonymous on purpose: a repeated logout for a session that has already ended still answers 204.
        group.MapPost("/logout", LogoutAsync).AllowAnonymous();

        return app;
    }

    private static async Task<IResult> LogoutAsync(
        HttpRequest request,
        ISessionService sessions,
        CancellationToken cancellationToken)
    {
        var sessionId = BearerToken.Read(request);
        if (sessionId is null)
        {
            return TypedResults.Problem(
                title: "Unauthorized",
                detail: "Sign in to continue.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            await sessions.LogoutAsync(sessionId, cancellationToken);
        }
        catch (SessionStoreException)
        {
            // The session was not removed, so it stays active and the staff member can retry.
            return TypedResults.Problem(
                title: "Logout failed",
                detail: "Your session is still active. Try logging out again.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return TypedResults.NoContent();
    }
}
