namespace Api.Sessions;

public sealed record Session(
    string SessionId,
    string UserId,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastActivity);
