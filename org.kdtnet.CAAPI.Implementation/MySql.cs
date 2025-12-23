using System.Data.Common;
using MySql.Data.MySqlClient;
using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Implementation;

public class MySqlDataStore : DataStoreBase, IDataStore
{
    private IConfigurationSource ConfigurationSource { get; }

    public MySqlDataStore(IConfigurationSource configurationSource)
    {
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
    }

    protected override DbConnection GetConnection()
    {
        return new MySqlConnection(ConfigurationSource.ConfigObject.DataStore.ConnectionString);
    }

    protected override DbParameter CreateParameter(string? parameterName, object? parameterValue)
    {
        return new MySqlParameter(parameterName, parameterValue);
    }

    protected override void PreInitDdl()
    {
    }

    protected override void PostInitDdl()
    {
    }

    protected override string SetIdentifierTicks(string sql) => sql.Replace("\"", "`");

    protected override bool ExistsTable(string tableName, DbTransaction tx)
    {
        var sql = $"SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = '{ConfigurationSource.ConfigObject.DataStore.TableSchema}' AND table_name = '{tableName}'";
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}