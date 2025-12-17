namespace org.kdtnet.CAAPI.Common.Abstraction;

public abstract class ApiDisplayableException : Exception
{
    public ApiDisplayableException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public class ApiGenericException : ApiDisplayableException
{
    public ApiGenericException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public ApiGenericException(string message)
        : this(message, null)
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

public abstract class DbException : Exception
{
    public DbException(string message, Exception? innerException) : base(message, innerException) { }
}

public class DbNullColumnException : DbException
{
    public DbNullColumnException(string columnName, Exception? innerException)
        : base($"Null value in column: {columnName}", innerException) { }
    public DbNullColumnException(string columnName)
        : this(columnName, null) { }
}

public class DbEnumFormatException : DbException
{
    public DbEnumFormatException(string columnName, Type type, Exception? innerException)
        : base($"Cannot convert value in column: {columnName} to enum type {type}", innerException) { }

    public DbEnumFormatException(string columnName, Type type)
        : this(columnName, type, null) { }
}