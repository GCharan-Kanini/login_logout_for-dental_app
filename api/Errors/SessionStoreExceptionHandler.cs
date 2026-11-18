using Api.Sessions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Api.Errors;

// Answers a session store outage with 503 wherever it surfaces. Authentication runs before any endpoint, so a
// failed lookup would otherwise reach the caller as a 500 and skip the logout endpoint's own error response.
public sealed class SessionStoreExceptionHandler(ILogger<SessionStoreExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not SessionStoreException)
        {
            return false;
        }

        logger.LogError(exception, "The session store could not complete a request.");
        await TypedResults.Problem(
                title: "Service unavailable",
                detail: "Sessions cannot be checked right now. Try again shortly.",
                statusCode: StatusCodes.Status503ServiceUnavailable)
            .ExecuteAsync(httpContext);
        return true;
    }
}
