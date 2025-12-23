using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Common.Utility;

public static class DatabaseHelper
{
    public static Guid GetGuidNotNull(this IDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal) ? throw new DbNullColumnException(columnName) : reader.GetGuid(ordinal);
    }

    public static bool GetBoolNotNull(this IDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            throw new DbNullColumnException(columnName);
                
        var intValue = reader.GetInt32(ordinal);
        return intValue != 0;

        //return reader.IsDBNull(ordinal) ? throw new DbNullColumnException(columnName) : reader.GetBoolean(ordinal);
    }

    public static int GetInt32NotNull(this IDataReader reader, string columnName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal) ? throw new DbNullColumnException(columnName) : reader.GetInt32(ordinal);
    }

    public static string GetStringNotNull(this IDataReader reader, string columnName, bool blankOrEmptyIsNull)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var ordinal = reader.GetOrdinal(columnName);

        if (reader.IsDBNull(ordinal))
            throw new DbNullColumnException(columnName);

        var returnValue = reader.GetString(ordinal);

        if(returnValue == null || (blankOrEmptyIsNull && string.IsNullOrWhiteSpace(returnValue)))
            throw new DbNullColumnException(columnName);

        return returnValue;
    }

    public static TEnum GetEnumNotNull<TEnum>(this IDataReader reader, string columnName) where TEnum : struct
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var rawDbValue = GetStringNotNull(reader, columnName, true);

        return !Enum.TryParse<TEnum>(rawDbValue, true, out var returnValue) ? throw new DbEnumFormatException(columnName, typeof(TEnum)) : returnValue;
    }
    
    public static IEnumerable<T> GetList<T>(this IDataReader reader, Func<IDataReader, T> loader)
    {
        return GetListNullable(reader, loader) ?? [];
    }

    public static IEnumerable<T>? GetListNullable<T>(this IDataReader reader, Func<IDataReader, T> loader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(loader);

        List<T>? returnValue = null;

        while (reader.Read())
        {
            returnValue ??= [];
            returnValue.Add(loader(reader));
        }

        return returnValue;
    }
}