using Api.Sessions;

namespace Api.Tests.Sessions;

public sealed class SessionStoreTests
{
    private static readonly Session Active = new(
        "session-1",
        "user-42",
        DateTimeOffset.UtcNow.AddHours(8),
        DateTimeOffset.UtcNow);

    [Fact]
    public async Task Saved_session_is_found_until_it_is_removed()
    {
        var store = new SessionStore();
        await store.SaveAsync(Active, CancellationToken.None);

        Assert.Equal(Active, await store.FindAsync("session-1", CancellationToken.None));

        await store.RemoveAsync("session-1", CancellationToken.None);

        Assert.Null(await store.FindAsync("session-1", CancellationToken.None));
    }

    [Fact]
    public async Task Removing_a_session_twice_is_not_an_error()
    {
        var store = new SessionStore();
        await store.SaveAsync(Active, CancellationToken.None);

        await store.RemoveAsync("session-1", CancellationToken.None);
        await store.RemoveAsync("session-1", CancellationToken.None);

        Assert.Null(await store.FindAsync("session-1", CancellationToken.None));
    }
}
