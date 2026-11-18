namespace Api.Auth;

public static class BearerToken
{
    private const string Prefix = "Bearer ";

    // Returns the session identifier from the Authorization header, or null when there is none.
    public static string? Read(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        if (!header.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = header[Prefix.Length..].Trim();
        return token.Length == 0 ? null : token;
    }
}
