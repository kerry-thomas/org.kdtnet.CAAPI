using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using Microsoft.Data.Sqlite;

namespace org.kdtnet.CAAPI.Implementation;

public class SqliteDataStoreTransaction : IDataStoreTransaction
{
    private SqliteTransaction InternalTx { get; set; }

    internal SqliteDataStoreTransaction(SqliteConnection connection)
    {
        InternalTx = connection.BeginTransaction();
    }

    public void Dispose()
    {
        InternalTx.Dispose();
        InternalTx = null!;
    }

    public void Commit()
    {
        InternalTx.Commit();
    }

    public void Rollback()
    {
        InternalTx.Rollback();
    }
}

public class SqliteDataStore : IDataStore
{
    private readonly object _lockObject = new();
    private IConfigurationSource ConfigurationSource { get; set; }

    private SqliteConnection? InternalConnection { get; set; }

    public SqliteDataStore(IConfigurationSource configurationSource)
    {
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
    }

    public void Dispose()
    {
        if (InternalConnection == null)
            return;
        InternalConnection.Dispose();
        InternalConnection = null;
    }

    private void Init()
    {
        lock (_lockObject)
        {
            if (InternalConnection != null)
                return;

            SqliteConnection? tInternalConnection = null;
            try
            {
                tInternalConnection = new SqliteConnection(ConfigurationSource.ConfigObject.DataStore.ConnectionString);
                tInternalConnection.Open();
                InternalConnection = tInternalConnection;
            }
            catch
            {
                tInternalConnection?.Dispose();
                throw;
            }
        }
    }

    public void Initialize()
    {
        CreateTablesIfNeeded();
    }

    public void CreateTablesIfNeeded()
    {
        Init();
        using (var tx = InternalConnection!.BeginTransaction())
        {
            CreateUserTableIfNeeded(tx);
            CreateRoleTableIfNeeded(tx);

            tx.Commit();
        }
    }

    #region DDL SQL

    #region Create Table Statements

    private const string c__Sql_Ddl_CreateTableUser =
        @"CREATE TABLE ""User"" (
	        ""UserId""	TEXT NOT NULL,
	        ""FriendlyName""	TEXT NOT NULL,
	        ""IsActive""	INTEGER NOT NULL,
	        PRIMARY KEY(""UserId"")
                )  ";

    private const string c__Sql_Ddl_CreateTableRole = @"CREATE TABLE ""Role"" (
	        ""RoleId""	TEXT NOT NULL,
	        ""FriendlyName""	TEXT NOT NULL,
	        PRIMARY KEY(""RoleId"")
            );";

    #endregion

    #endregion

    private void CreateUserTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("User", tx)) RunDdl(c__Sql_Ddl_CreateTableUser, tx);
    }

    private void CreateRoleTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("Role", tx)) RunDdl(c__Sql_Ddl_CreateTableRole, tx);
    }

    private void RunDdl(string ddlSql, SqliteTransaction tx)
    {
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = ddlSql;
            cmd.Transaction = tx;
            cmd.ExecuteNonQuery();
        }
    }

    private bool ExistsTable(string tableName, SqliteTransaction tx)
    {
        var sql = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public IDataStoreTransaction BeginTransaction()
    {
        Init();

        return new SqliteDataStoreTransaction(InternalConnection!);
    }

    public bool PersistUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var tx = InternalConnection!.BeginTransaction())
        {
            var returnValue = ExistsUser(user.UserId, tx);
            if (returnValue)
                UpdateUser(user, tx);
            else
                InsertUser(user, tx);

            tx.Commit();

            return returnValue;
        }
    }

    private void InsertUser(DbUser user, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText =
                "insert into User (UserId, FriendlyName, IsActive) VALUES (@userId, @friendlyName, @isActive)";
            cmd.Parameters.AddWithValue("@userId", user.UserId);
            cmd.Parameters.AddWithValue("@friendlyName", user.FriendlyName);
            cmd.Parameters.AddWithValue("@isActive", user.UserId);

            cmd.Transaction = tx;

            cmd.ExecuteNonQuery();
        }
    }

    private void UpdateUser(DbUser user, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "update User SET FriendlyName=@friendlyName, IsActive=@isActive where UserId=@userId";
            cmd.Parameters.AddWithValue("@friendlyName", user.FriendlyName);
            cmd.Parameters.AddWithValue("@isActive", user.UserId);
            cmd.Parameters.AddWithValue("@userId", user.UserId);

            cmd.Transaction = tx;

            cmd.ExecuteNonQuery();
        }
    }

    public DbUser? FetchUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select * from User where UserId = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return DbUser.CreateFromDataReader(reader);
                else
                    return null;
            }
        }
    }

    private bool ExistsUser(string userId, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(userId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from User where UserId = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Transaction = tx;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public void DeleteUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "delete from User where UserId = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);

            var count = cmd.ExecuteNonQuery();
        }
    }

    public bool PersistRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var tx = InternalConnection!.BeginTransaction())
        {
            var returnValue = ExistsRole(role.RoleId, tx);
            if (returnValue)
                UpdateRole(role, tx);
            else
                InsertRole(role, tx);

            tx.Commit();

            return returnValue;
        }
    }

    private void InsertRole(DbRole role, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "insert into Role (RoleId, FriendlyName) VALUES (@roleId, @friendlyName)";
            cmd.Parameters.AddWithValue("@roleId", role.RoleId);
            cmd.Parameters.AddWithValue("@friendlyName", role.FriendlyName);

            cmd.Transaction = tx;

            cmd.ExecuteNonQuery();
        }
    }

    private void UpdateRole(DbRole role, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "update Role SET FriendlyName=@friendlyName where RoleId=@roleId";
            cmd.Parameters.AddWithValue("@friendlyName", role.FriendlyName);
            cmd.Parameters.AddWithValue("@roleId", role.RoleId);

            cmd.Transaction = tx;

            cmd.ExecuteNonQuery();
        }
    }

    private bool ExistsRole(string roleId, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from Role where RoleId = @roleId";
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.Transaction = tx;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public DbRole? FetchRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select * from Role where RoleId = @roleId";
            cmd.Parameters.AddWithValue("@roleId", roleId);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return DbRole.CreateFromDataReader(reader);
                else
                    return null;
            }
        }
    }

    public void DeleteRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "delete from Role where RoleId = @roleId";
            cmd.Parameters.AddWithValue("@roleId", roleId);

            cmd.ExecuteNonQuery();
        }
    }

    public bool PersistUserRole(DbUserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        Init();

        using (var tx = InternalConnection!.BeginTransaction())
        {
            var returnValue = ExistsUserRole(userRole.UserId, userRole.RoleId, tx);
            if (!returnValue)
                InsertUserRole(userRole, tx);

            tx.Commit();

            return returnValue;
        }
    }
    
    private bool ExistsUserRole(string userId, string roleId, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from UserRole where UserId = @userId and RoleId = @roleId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.Transaction = tx;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
    
    private void InsertUserRole(DbUserRole userRole, SqliteTransaction? tx)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "insert into UserRole (UserId, RoleId) VALUES (@userId, @roleId)";
            cmd.Parameters.AddWithValue("@userId", userRole.UserId);
            cmd.Parameters.AddWithValue("@roleId", userRole.RoleId);

            cmd.Transaction = tx;

            cmd.ExecuteNonQuery();
        }
    }

    public DbUserRole? FetchUserRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select * from UserRole where UserId = @userId and RoleId = @roleId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@roleId", roleId);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return DbUserRole.CreateFromDataReader(reader);
                else
                    return null;
            }
        }
    }

    public void DeleteUserRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "delete from UserRole where UserId = @userId and RoleId = @roleId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@roleId", roleId);

            cmd.ExecuteNonQuery();
        }
    }
}