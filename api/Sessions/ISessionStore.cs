namespace Api.Sessions;

// Failures the store cannot recover from are reported as SessionStoreException.
public interface ISessionStore
{
    Task SaveAsync(Session session, CancellationToken cancellationToken);

    Task<Session?> FindAsync(string sessionId, CancellationToken cancellationToken);

    // Removing a session that is already gone is not an error: logout is idempotent.
    Task RemoveAsync(string sessionId, CancellationToken cancellationToken);
}
