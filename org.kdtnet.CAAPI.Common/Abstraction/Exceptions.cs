namespace org.kdtnet.CAAPI.Common.Abstraction;

public abstract class ApiDisplayableException : Exception
{
    public ApiDisplayableException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public class ValidationException : ApiDisplayableException
{
    public ValidationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public ValidationException(string message)
        : this(message, null)
    {
    }
}