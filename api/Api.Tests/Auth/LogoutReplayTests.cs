using System.Net;
using static Api.Tests.Auth.SessionRequests;

namespace Api.Tests.Auth;

public sealed class LogoutReplayTests : IDisposable
{
    private const int ParallelRequests = 20;
    private const int RapidRequests = 5;

    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public LogoutReplayTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Logout_removes_the_session_record_once_an_authenticated_request_has_confirmed_it()
    {
        await SeedActiveSessionAsync(_factory.Store);
        var before = await GetStaffAsync(_client, SessionId);

        var logout = await PostLogoutAsync(_client, SessionId);
        var after = await GetStaffAsync(_client, SessionId);

        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Null(await _factory.Store.FindAsync(SessionId, CancellationToken.None));
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
        Assert.DoesNotContain(UserId, after.Body);
    }

    [Fact]
    public async Task Rapid_sequential_replays_after_logout_are_all_rejected()
    {
        await SeedActiveSessionAsync(_factory.Store);
        await PostLogoutAsync(_client, SessionId);

        for (var i = 0; i < RapidRequests; i++)
        {
            var replay = await GetStaffAsync(_client, SessionId);

            Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
            Assert.DoesNotContain(UserId, replay.Body);
        }
    }

    [Fact]
    public async Task Parallel_replays_after_logout_are_all_rejected_and_return_no_staff_data()
    {
        await SeedActiveSessionAsync(_factory.Store);
        await PostLogoutAsync(_client, SessionId);

        var replies = await Task.WhenAll(
            Enumerable.Range(0, ParallelRequests).Select(_ => GetStaffAsync(_client, SessionId)));

        Assert.Equal(ParallelRequests, replies.Length);
        Assert.All(replies, reply => Assert.Equal(HttpStatusCode.Unauthorized, reply.StatusCode));
        Assert.All(replies, reply => Assert.DoesNotContain(UserId, reply.Body));
    }

    [Fact]
    public async Task Parallel_logouts_of_one_session_all_succeed_and_leave_no_session()
    {
        await SeedActiveSessionAsync(_factory.Store);

        var replies = await Task.WhenAll(
            Enumerable.Range(0, ParallelRequests).Select(_ => PostLogoutAsync(_client, SessionId)));

        Assert.All(replies, reply => Assert.Equal(HttpStatusCode.NoContent, reply.StatusCode));
        Assert.Null(await _factory.Store.FindAsync(SessionId, CancellationToken.None));
    }

    [Fact]
    public async Task Requests_racing_a_logout_are_never_served_once_it_has_completed()
    {
        await SeedActiveSessionAsync(_factory.Store);

        var logoutTask = PostLogoutAsync(_client, SessionId);
        var racingTasks = Enumerable.Range(0, ParallelRequests)
            .Select(_ => GetStaffAsync(_client, SessionId))
            .ToArray();
        var racing = await Task.WhenAll(racingTasks);
        var logout = await logoutTask;

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.All(racing, reply =>
            Assert.Contains(reply.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Unauthorized }));

        var afterLogout = await Task.WhenAll(
            Enumerable.Range(0, ParallelRequests).Select(_ => GetStaffAsync(_client, SessionId)));

        Assert.All(afterLogout, reply => Assert.Equal(HttpStatusCode.Unauthorized, reply.StatusCode));
        Assert.All(afterLogout, reply => Assert.DoesNotContain(UserId, reply.Body));
    }
}
