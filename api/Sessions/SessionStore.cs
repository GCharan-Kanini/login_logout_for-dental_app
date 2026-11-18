using System.Collections.Concurrent;

namespace Api.Sessions;

public sealed class SessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new(StringComparer.Ordinal);

    public Task SaveAsync(Session session, CancellationToken cancellationToken)
    {
        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<Session?> FindAsync(string sessionId, CancellationToken cancellationToken)
    {
        var found = _sessions.TryGetValue(sessionId, out var session) ? session : null;
        return Task.FromResult(found);
    }

    public Task RemoveAsync(string sessionId, CancellationToken cancellationToken)
    {
        // TryRemove reports false when a concurrent logout already removed the session. The outcome is the
        // same either way, so there is nothing to report.
        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }
}
