using Api.Sessions;
using Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests.Sessions;

public sealed class SessionServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeSessionStore _store = new();
    private readonly SessionService _service;

    public SessionServiceTests()
    {
        _service = new SessionService(_store, new FixedTimeProvider(Now), NullLogger<SessionService>.Instance);
    }

    [Fact]
    public async Task Resolve_returns_an_active_session()
    {
        await _store.SaveAsync(Session("session-1", Now.AddHours(1)), CancellationToken.None);

        var resolved = await _service.ResolveAsync("session-1", CancellationToken.None);

        Assert.Equal("user-42", resolved?.UserId);
    }

    [Fact]
    public async Task Resolve_returns_null_for_an_unknown_session()
    {
        var resolved = await _service.ResolveAsync("missing", CancellationToken.None);

        Assert.Null(resolved);
    }

    [Fact]
    public async Task Resolve_returns_null_and_removes_an_expired_session()
    {
        await _store.SaveAsync(Session("session-1", Now.AddMinutes(-1)), CancellationToken.None);

        var resolved = await _service.ResolveAsync("session-1", CancellationToken.None);

        Assert.Null(resolved);
        Assert.Null(await _store.FindAsync("session-1", CancellationToken.None));
    }

    [Fact]
    public async Task Logout_removes_the_session_from_the_store()
    {
        await _store.SaveAsync(Session("session-1", Now.AddHours(1)), CancellationToken.None);

        await _service.LogoutAsync("session-1", CancellationToken.None);

        Assert.Null(await _service.ResolveAsync("session-1", CancellationToken.None));
    }

    [Fact]
    public async Task Logout_of_a_session_that_is_already_gone_completes()
    {
        await _service.LogoutAsync("missing", CancellationToken.None);
        await _service.LogoutAsync("missing", CancellationToken.None);
    }

    [Fact]
    public async Task Logout_that_the_store_fails_to_complete_throws_and_keeps_the_session()
    {
        await _store.SaveAsync(Session("session-1", Now.AddHours(1)), CancellationToken.None);
        _store.FailRemovals = true;

        await Assert.ThrowsAsync<SessionStoreException>(
            () => _service.LogoutAsync("session-1", CancellationToken.None));

        Assert.NotNull(await _service.ResolveAsync("session-1", CancellationToken.None));
    }

    private static Session Session(string sessionId, DateTimeOffset expiresAt) =>
        new(sessionId, "user-42", expiresAt, Now);
}
