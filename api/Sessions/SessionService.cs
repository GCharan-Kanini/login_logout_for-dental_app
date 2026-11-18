using Microsoft.Extensions.Logging;

namespace Api.Sessions;

public sealed class SessionService(
    ISessionStore store,
    TimeProvider time,
    ILogger<SessionService> logger) : ISessionService
{
    public async Task<Session?> ResolveAsync(string sessionId, CancellationToken cancellationToken)
    {
        var session = await store.FindAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt > time.GetUtcNow())
        {
            return session;
        }

        // An expired session is ended here so that it cannot be presented again.
        await store.RemoveAsync(sessionId, cancellationToken);
        return null;
    }

    public async Task LogoutAsync(string sessionId, CancellationToken cancellationToken)
    {
        try
        {
            await store.RemoveAsync(sessionId, cancellationToken);
        }
        catch (SessionStoreException exception)
        {
            // Logged here and passed up unchanged. The session was not removed, so it stays active for a retry.
            logger.LogError(exception, "Logout could not end the session because the session store failed.");
            throw;
        }
    }
}
