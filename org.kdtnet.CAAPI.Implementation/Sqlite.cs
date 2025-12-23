// ReSharper disable InconsistentNaming

using System.Data.Common;
using org.kdtnet.CAAPI.Common.Abstraction;
using Microsoft.Data.Sqlite;

namespace org.kdtnet.CAAPI.Implementation;

public abstract class SqliteDataStoreBase : DataStoreBase, IDataStore, IDisposable
{
    protected abstract string GetConnectionString();
    
    protected override DbConnection GetConnection()
    {
        return new SqliteConnection(GetConnectionString());
    }

    protected override DbParameter CreateParameter(string? parameterName, object? parameterValue)
    {
        return new SqliteParameter(parameterName, parameterValue);
    }

    protected override void PreInitDdl()
    {
        EnforceForeignKeys();
    }
    
    protected override void PostInitDdl()
    {
    }
    
    private void EnforceForeignKeys()
    {
        RunDdl("PRAGMA foreign_keys = ON;", null!);
    }

    protected override string SetIdentifierTicks(string sql) => sql;

    protected override bool ExistsTable(string tableName, DbTransaction tx)
    {
        var sql = $"SELECT count(1) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}


public class SqliteInMemoryDataStore : SqliteDataStoreBase, IDataStore, IDisposable
{
    protected override string GetConnectionString() => "Data Source=:memory:";
}

public class SqlitePhysicalDataStore : SqliteDataStoreBase, IDataStore, IDisposable
{
    private IConfigurationSource ConfigurationSource { get; }

    public SqlitePhysicalDataStore(IConfigurationSource configurationSource)
    {
        ConfigurationSource = configurationSource ??  throw new ArgumentNullException(nameof(configurationSource));
    }
    
    protected override string GetConnectionString() => ConfigurationSource.ConfigObject.DataStore.ConnectionString;
}


// ReSharper restore InconsistentNaming
