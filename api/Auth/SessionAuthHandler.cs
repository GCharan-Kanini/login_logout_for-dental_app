using System.Security.Claims;
using System.Text.Encodings.Web;
using Api.Sessions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Auth;

public sealed class SessionAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory loggerFactory,
    UrlEncoder encoder,
    ISessionService sessions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string SchemeName = "Session";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var sessionId = BearerToken.Read(Request);
        if (sessionId is null)
        {
            return AuthenticateResult.NoResult();
        }

        var session = await sessions.ResolveAsync(sessionId, Context.RequestAborted);
        if (session is null)
        {
            return AuthenticateResult.Fail("The session is not active.");
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, session.UserId) },
            SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        return TypedResults.Problem(
                title: "Unauthorized",
                detail: "Sign in to continue.",
                statusCode: StatusCodes.Status401Unauthorized)
            .ExecuteAsync(Context);
    }
}
