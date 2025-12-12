using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Common.Utility;

public static class ValidationHelper
{
    public static void AssertStringNotNull(string? value, string name, bool blankOrEmptyIsNull)
    {
        if (value == null)
            throw new ValidationException($"{name} may not be null");
        
        if(blankOrEmptyIsNull && string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"{name} may not be null/empty/blank");
    }
}