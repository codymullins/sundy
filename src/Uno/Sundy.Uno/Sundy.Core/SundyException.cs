namespace Sundy.Core;

public class SundyException : Exception
{
    public SundyException() : base()
    {
    }

    public SundyException(string message) : base(message)
    {
    }

    public SundyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class EventNotFoundException(string? eventId) : SundyException($"Event with ID '{eventId}' was not found.");