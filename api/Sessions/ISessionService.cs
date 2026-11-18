namespace Api.Sessions;

public interface ISessionService
{
    // Returns the active session, or null when it is unknown or expired.
    Task<Session?> ResolveAsync(string sessionId, CancellationToken cancellationToken);

    // Ends the session. Ending a session that is already gone succeeds.
    Task LogoutAsync(string sessionId, CancellationToken cancellationToken);
}
