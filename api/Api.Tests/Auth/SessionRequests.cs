using System.Net;
using System.Net.Http.Headers;
using Api.Sessions;
using Api.Tests.Fakes;

namespace Api.Tests.Auth;

// Shared request helpers for the auth tests. Each call reads the body and disposes the response, so tests
// can compare replies without holding open HTTP responses.
public static class SessionRequests
{
    public const string SessionId = "session-1";
    public const string UserId = "user-42";

    public sealed record ApiReply(HttpStatusCode StatusCode, string? MediaType, string Body);

    public static Task SeedActiveSessionAsync(FakeSessionStore store) =>
        store.SaveAsync(
            new Session(SessionId, UserId, DateTimeOffset.UtcNow.AddHours(8), DateTimeOffset.UtcNow),
            CancellationToken.None);

    public static Task<ApiReply> PostLogoutAsync(HttpClient client, string sessionId) =>
        SendAsync(client, HttpMethod.Post, "/api/auth/logout", sessionId);

    public static Task<ApiReply> GetStaffAsync(HttpClient client, string sessionId) =>
        SendAsync(client, HttpMethod.Get, "/api/staff/me", sessionId);

    private static async Task<ApiReply> SendAsync(HttpClient client, HttpMethod method, string path, string sessionId)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", sessionId) },
        };
        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        return new ApiReply(response.StatusCode, response.Content.Headers.ContentType?.MediaType, body);
    }
}
