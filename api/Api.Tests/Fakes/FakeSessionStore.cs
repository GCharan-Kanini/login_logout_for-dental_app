using System.Collections.Concurrent;
using Api.Sessions;

namespace Api.Tests.Fakes;

public sealed class FakeSessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new(StringComparer.Ordinal);

    // When set, every removal fails the way a store outage would.
    public bool FailRemovals { get; set; }

    public Task SaveAsync(Session session, CancellationToken cancellationToken)
    {
        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    // When set, every lookup fails the way a store outage would.
    public bool FailLookups { get; set; }

    public Task<Session?> FindAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (FailLookups)
        {
            throw new SessionStoreException("Simulated session store outage.");
        }

        var found = _sessions.TryGetValue(sessionId, out var session) ? session : null;
        return Task.FromResult(found);
    }

    public Task RemoveAsync(string sessionId, CancellationToken cancellationToken)
    {
        if (FailRemovals)
        {
            throw new SessionStoreException("Simulated session store outage.");
        }

        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }
}
