using System.Diagnostics;
using System.Text;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using Microsoft.Data.Sqlite;

namespace org.kdtnet.CAAPI.Implementation;

public class SqliteDataStore : IDataStore
{
    private readonly object _lockObject = new();
    private IConfigurationSource ConfigurationSource { get; set; }

    private SqliteConnection? InternalConnection { get; set; }
    private SqliteTransaction? CurrentTransaction { get; set; }

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
            CreateUserRoleTableIfNeeded(tx);
            CreateRolePrivilegeTableIfNeeded(tx);

            tx.Commit();
        }
    }

    #region DDL SQL

    #region Create Table Statements

    private const string c__Sql_Ddl_CreateTable_User =
        @"CREATE TABLE ""User"" (
	        ""UserId""	TEXT NOT NULL,
	        ""FriendlyName""	TEXT NOT NULL,
	        ""IsActive""	INTEGER NOT NULL,
	        PRIMARY KEY(""UserId"")
                )  ";

    private const string c__Sql_Ddl_CreateTable_Role = @"CREATE TABLE ""Role"" (
	        ""RoleId""	TEXT NOT NULL,
	        ""FriendlyName""	TEXT NOT NULL,
	        PRIMARY KEY(""RoleId"")
            );";

    private const string c__Sql_Ddl_CreateTable_UserRole = @"CREATE TABLE ""UserRole"" (
	        ""UserId""	TEXT NOT NULL,
	        ""RoleId""	TEXT NOT NULL,
	        PRIMARY KEY(""UserId"",""RoleId"" )
            );";

    private const string c__Sql_Ddl_CreateTable_RolePrivilege = @"CREATE TABLE ""RolePrivilege"" (
	        ""RoleId""	TEXT NOT NULL,
	        ""PrivilegeId""	TEXT NOT NULL,
	        PRIMARY KEY(""RoleId"",""PrivilegeId"" )
            );";

    #endregion

    #endregion

    private void CreateUserTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("User", tx)) RunDdl(c__Sql_Ddl_CreateTable_User, tx);
    }

    private void CreateRoleTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("Role", tx)) RunDdl(c__Sql_Ddl_CreateTable_Role, tx);
    }

    private void CreateUserRoleTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("UserRole", tx)) RunDdl(c__Sql_Ddl_CreateTable_UserRole, tx);
    }

    private void CreateRolePrivilegeTableIfNeeded(SqliteTransaction tx)
    {
        if (!ExistsTable("RolePrivilege", tx)) RunDdl(c__Sql_Ddl_CreateTable_RolePrivilege, tx);
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

    public void TransactionWrap(Func<bool> callback)
    {
        // bool iCreatedTransaction = false;
        //
        // if (CurrentTransaction == null)
        // {
        //     CurrentTransaction = InternalConnection!.BeginTransaction();
        //     iCreatedTransaction = true;
        // }
        //
        // try
        // {
        //     var callbackResult = callback();
        //     if (iCreatedTransaction)
        //     {
        //         if (callbackResult)
        //             CurrentTransaction.Commit();
        //         else
        //             CurrentTransaction.Rollback();
        //     }
        // }
        // catch
        // {
        //     if (iCreatedTransaction)
        //         CurrentTransaction.Rollback();
        //     throw;
        // }
        // finally
        // {
        //     if (iCreatedTransaction)
        //         CurrentTransaction = null;
        // }

        CoreTransactionWrap(iCreatedTransaction =>
        {
            var callbackResult = callback();
            if (iCreatedTransaction)
            {
                Debug.Assert(CurrentTransaction != null);
                if (callbackResult)
                    CurrentTransaction.Commit();
                else
                    CurrentTransaction.Rollback();
            }
        });
    }

    public void TransactionWrap(Action callback)
    {
        CoreTransactionWrap(iCreatedTransaction =>
        {
            callback();
            if (iCreatedTransaction)
            {
                Debug.Assert(CurrentTransaction != null);
                CurrentTransaction.Commit();
            }
        });
    }

    private void CoreTransactionWrap(Action<bool> internalCallback)
    {
        bool iCreatedTransaction = false;

        if (CurrentTransaction == null)
        {
            CurrentTransaction = InternalConnection!.BeginTransaction();
            iCreatedTransaction = true;
        }

        try
        {
            internalCallback(iCreatedTransaction);
        }
        catch
        {
            if (iCreatedTransaction)
                CurrentTransaction.Rollback();
            throw;
        }
        finally
        {
            if (iCreatedTransaction)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }
    }

    public void UpdateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "update User SET FriendlyName=@friendlyName, IsActive=@isActive where UserId=@userId";
            cmd.Parameters.AddWithValue("@friendlyName", user.FriendlyName);
            cmd.Parameters.AddWithValue("@isActive", user.IsActive);
            cmd.Parameters.AddWithValue("@userId", user.UserId);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public void InsertUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText =
                "insert into User (UserId, FriendlyName, IsActive) VALUES (@userId, @friendlyName, @isActive)";
            cmd.Parameters.AddWithValue("@userId", user.UserId);
            cmd.Parameters.AddWithValue("@friendlyName", user.FriendlyName);
            cmd.Parameters.AddWithValue("@isActive", user.IsActive);

            cmd.Transaction = CurrentTransaction;

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

            cmd.Transaction = CurrentTransaction;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return DbUser.CreateFromDataReader(reader);
                else
                    return null;
            }
        }
    }

    public bool ExistsUser(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from User where UserId = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);

            cmd.Transaction = CurrentTransaction;

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

            cmd.Transaction = CurrentTransaction;

            var count = cmd.ExecuteNonQuery();
        }
    }

    public bool PersistRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        var returnValue = ExistsRole(role.RoleId);
        if (returnValue)
            UpdateRole(role);
        else
            InsertRole(role);

        return returnValue;
    }

    private void InsertRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "insert into Role (RoleId, FriendlyName) VALUES (@roleId, @friendlyName)";
            cmd.Parameters.AddWithValue("@roleId", role.RoleId);
            cmd.Parameters.AddWithValue("@friendlyName", role.FriendlyName);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    private void UpdateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "update Role SET FriendlyName=@friendlyName where RoleId=@roleId";
            cmd.Parameters.AddWithValue("@friendlyName", role.FriendlyName);
            cmd.Parameters.AddWithValue("@roleId", role.RoleId);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public bool ExistsRole(string roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from Role where RoleId = @roleId";
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.Transaction = CurrentTransaction;

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

            cmd.Transaction = CurrentTransaction;

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

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    #region UserRole

    public bool PersistUserRole(DbUserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        Init();

        var returnValue = ExistsUserRole(userRole.UserId, userRole.RoleId);
        if (!returnValue)
            InsertUserRole(userRole);

        return returnValue;
    }

    public bool ExistsUserRole(string userId, string roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select count(1) from UserRole where UserId = @userId and RoleId = @roleId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@roleId", roleId);

            cmd.Transaction = CurrentTransaction;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    private void InsertUserRole(DbUserRole userRole)
    {
        ArgumentNullException.ThrowIfNull(userRole);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "insert into UserRole (UserId, RoleId) VALUES (@userId, @roleId)";
            cmd.Parameters.AddWithValue("@userId", userRole.UserId);
            cmd.Parameters.AddWithValue("@roleId", userRole.RoleId);

            cmd.Transaction = CurrentTransaction;

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

            cmd.Transaction = CurrentTransaction;

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                    return DbUserRole.CreateFromDataReader(reader);
                else
                    return null;
            }
        }
    }

    public IEnumerable<DbUserRole> FetchAllUserRoles()
    {
        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "select * from UserRole";

            cmd.Transaction = CurrentTransaction;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    yield return DbUserRole.CreateFromDataReader(reader);
            }
        }
    }

    public void DeleteUserRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "delete from UserRole where UserId = @userId and RoleId = @roleId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@roleId", roleId);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public IEnumerable<string> AllUserIdsWithPrivilege(string privilegeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privilegeId);
        var strSql = new StringBuilder();
        strSql.Append(" select userId");
        strSql.Append(" from RolePrivilege rp");
        strSql.Append("   inner join UserRole ur on (rp.RoleId = ur.RoleId)");
        strSql.Append(" where rp.PrivilegeId = @privilegeId");

        using (var cmd = InternalConnection!.CreateCommand())
        {
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    yield return rdr.GetString(0);
            }
        }
    }

    #endregion

    #region RolePrivilege

    public void InsertRolePrivilege(DbRolePrivilege rolePrivilege)
    {
        ArgumentNullException.ThrowIfNull(rolePrivilege);
        rolePrivilege.Validate();

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = "insert into RolePrivilege (RoleId, PrivilegeId) VALUES (@roleId, @privilegeId)";
            cmd.Parameters.AddWithValue("@roleId", rolePrivilege.RoleId);
            cmd.Parameters.AddWithValue("@privilegeId", rolePrivilege.PrivilegeId);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public bool ExistsUserInRoleWithPrivilege(string userId, string privilegeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(privilegeId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            var strSql = new StringBuilder();
            strSql.Append(" select count(1)");
            strSql.Append(" from RolePrivilege rp");
            strSql.Append(
                "   inner join UserRole ur on (rp.RoleId=ur.RoleId and rp.PrivilegeId=@privilegeId and ur.UserId=@userId)");

            cmd.CommandText = strSql.ToString();
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@privilegeId", privilegeId);

            cmd.Transaction = CurrentTransaction;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public void DeleteRolePrivilege(string roleId, string privilegeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(privilegeId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            var strSql = new StringBuilder();
            strSql.Append(" delete from RolePrivilege where RoleId=@roleId and PrivilegeId=@privilegeId");

            cmd.CommandText = strSql.ToString();
            cmd.Parameters.AddWithValue("@roleId", roleId);
            cmd.Parameters.AddWithValue("@privilegeId", privilegeId);

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    #endregion
}