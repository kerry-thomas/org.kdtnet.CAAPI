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

    protected override string c__Sql_Ddl_CreateTable_User { get; } = """
                                                                    CREATE TABLE "User" (
                                                                    	        "UserId"        VARCHAR(100) NOT NULL,
                                                                    	        "FriendlyName"  VARCHAR(100) NOT NULL,
                                                                    	        "IsActive"      INTEGER NOT NULL,
                                                                    	        PRIMARY KEY("UserId")
                                                                                    )  
                                                                    """;

    protected override string c__Sql_Ddl_CreateTable_Role { get; } = """
                                                                    CREATE TABLE "Role" (
                                                                    	        "RoleId"       VARCHAR(100) NOT NULL,
                                                                    	        "FriendlyName" VARCHAR(100) NOT NULL,
                                                                    	        PRIMARY KEY("RoleId") 
                                                                                );
                                                                    """;

    protected override string c__Sql_Ddl_CreateTable_UserRole { get; } = """
                                                                        CREATE TABLE "UserRole" (
                                                                        	        "UserId" VARCHAR(100) NOT NULL REFERENCES "User"("UserId"),
                                                                        	        "RoleId" VARCHAR(100) NOT NULL REFERENCES "Role"("RoleId"),
                                                                        	        PRIMARY KEY("UserId","RoleId" )
                                                                                    );
                                                                        """;

    protected override string c__Sql_Ddl_CreateTable_RolePrivilege { get; } = """
                                                                             CREATE TABLE "RolePrivilege" (
                                                                             	        "RoleId"      VARCHAR(100) NOT NULL REFERENCES "Role"("RoleId"),
                                                                             	        "PrivilegeId" VARCHAR(100) NOT NULL,
                                                                             	        PRIMARY KEY("RoleId","PrivilegeId" )
                                                                                         );
                                                                             """;

    protected override void PreInitDdl()
    {
    }

    protected override void PostInitDdl()
    {
    }

    protected override string SetIdentifierTicks(string sql) => sql.Replace("\"", "`");

    protected override bool ExistsTable(string tableName, DbTransaction tx)
    {
        var sql = $"SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = 'caapi' AND table_name = '{tableName}'";
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}