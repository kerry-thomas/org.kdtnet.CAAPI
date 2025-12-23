using System.Data.Common;
using Npgsql;
using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Implementation;

public class PostgresDataStore: DataStoreBase, IDataStore
{
    private IConfigurationSource ConfigurationSource { get; }

    public PostgresDataStore(IConfigurationSource configurationSource)
    {
        ConfigurationSource = configurationSource ??  throw new ArgumentNullException(nameof(configurationSource));
    }
    
    protected override DbConnection GetConnection()
    {
        return new NpgsqlConnection(ConfigurationSource.ConfigObject.DataStore.ConnectionString);
    }

    protected override DbParameter CreateParameter(string? parameterName, object? parameterValue)
    {
        return new NpgsqlParameter(parameterName, parameterValue);
    }

    protected override void PreInitDdl()
    {
    }

    protected override void PostInitDdl()
    {
    }

    protected override bool ExistsTable(string tableName, DbTransaction tx)
    {
        var sql = $"SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '{tableName}'";
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}