namespace Api.Sessions;

// Raised when the session store cannot complete an operation, for example when its backend is unreachable.
public sealed class SessionStoreException : Exception
{
    public SessionStoreException(string message)
        : base(message)
    {
    }

    public SessionStoreException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
