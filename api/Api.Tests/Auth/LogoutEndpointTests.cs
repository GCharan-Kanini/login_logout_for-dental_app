using System.Net;
using Api.Sessions;
using static Api.Tests.Auth.SessionRequests;

namespace Api.Tests.Auth;

public sealed class LogoutEndpointTests : IDisposable
{
    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public LogoutEndpointTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Logout_removes_the_session_so_the_same_token_no_longer_authenticates()
    {
        await SeedActiveSessionAsync(_factory.Store);

        var logout = await PostLogoutAsync(_client, SessionId);
        var staff = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Null(await _factory.Store.FindAsync(SessionId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.Unauthorized, staff.StatusCode);
        Assert.Equal("application/problem+json", staff.MediaType);
    }

    [Fact]
    public async Task A_request_replayed_after_logout_is_rejected_and_returns_no_staff_data()
    {
        await SeedActiveSessionAsync(_factory.Store);
        await PostLogoutAsync(_client, SessionId);

        var replay = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
        Assert.DoesNotContain(UserId, replay.Body);
    }

    [Fact]
    public async Task Logout_for_a_session_that_is_already_ended_still_succeeds()
    {
        await SeedActiveSessionAsync(_factory.Store);

        var first = await PostLogoutAsync(_client, SessionId);
        var second = await PostLogoutAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
    }

    [Fact]
    public async Task Logout_that_fails_returns_an_error_and_keeps_the_session_active()
    {
        await SeedActiveSessionAsync(_factory.Store);
        _factory.Store.FailRemovals = true;

        var logout = await PostLogoutAsync(_client, SessionId);
        var staff = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, logout.StatusCode);
        Assert.Equal(HttpStatusCode.OK, staff.StatusCode);
        Assert.Contains(UserId, staff.Body);
    }

    [Fact]
    public async Task Logout_when_the_store_cannot_be_read_returns_an_error_and_keeps_the_session()
    {
        await SeedActiveSessionAsync(_factory.Store);
        _factory.Store.FailLookups = true;

        var logout = await PostLogoutAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, logout.StatusCode);
        Assert.Equal("application/problem+json", logout.MediaType);

        _factory.Store.FailLookups = false;
        Assert.NotNull(await _factory.Store.FindAsync(SessionId, CancellationToken.None));
    }

    [Fact]
    public async Task Logout_retried_after_a_failure_ends_the_session()
    {
        await SeedActiveSessionAsync(_factory.Store);
        _factory.Store.FailRemovals = true;
        await PostLogoutAsync(_client, SessionId);

        _factory.Store.FailRemovals = false;
        var retry = await PostLogoutAsync(_client, SessionId);
        var staff = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.NoContent, retry.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, staff.StatusCode);
    }

    [Fact]
    public async Task Logout_without_a_token_is_refused()
    {
        var response = await _client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_session_that_has_expired_is_rejected()
    {
        await _factory.Store.SaveAsync(
            new Session(SessionId, UserId, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(-9)),
            CancellationToken.None);

        var response = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(await _factory.Store.FindAsync(SessionId, CancellationToken.None));
    }

    [Fact]
    public async Task Staff_me_without_a_token_is_rejected()
    {
        var response = await _client.GetAsync("/api/staff/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
