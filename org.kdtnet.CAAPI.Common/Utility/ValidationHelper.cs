using System.Runtime.CompilerServices;
using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Common.Utility;

public static class ValidationHelper
{
    public static void AssertStringNotNull(string? value, [CallerArgumentExpression(nameof(value))] string? propertyName = null )
    {
        AssertStringNotNull(value, true, propertyName);
    }

    public static void AssertStringNotNull(string? value, bool blankOrEmptyIsNull, [CallerArgumentExpression(nameof(value))] string? propertyName = null )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        if (value == null)
            throw new ValidationException($"{propertyName} may not be null");
        
        if(blankOrEmptyIsNull && string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{propertyName} may not be null/empty/blank");
    }

    public static void AssertCondition(Func<bool> action, string moniker, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(moniker);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (!action())
            throw new ValidationException($"{propertyName} failed custom condition {moniker}");
    }

    public static void AssertObjectNotNull(object value, [CallerArgumentExpression(nameof(value))] string? propertyName = null )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        if (value == null)
            throw new ValidationException($"{propertyName} cannot be null object.");
    }
}