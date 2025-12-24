// ReSharper disable InconsistentNaming

using System.Data.Common;
using System.Diagnostics;
using System.Text;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Implementation;

public abstract class DataStoreBase : IDisposable
{
    private readonly object _lockObject = new();

    protected DbConnection? InternalConnection { get; private set; }
    private DbTransaction? CurrentTransaction { get; set; }

    protected abstract DbConnection GetConnection();
    protected abstract DbParameter CreateParameter(string? parameterName, object? parameterValue);
    protected abstract void PreInitDdl();
    protected abstract void PostInitDdl();
    protected abstract bool ExistsTable(string tableName, DbTransaction tx);
    protected abstract string SetIdentifierTicks(string sql);

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

            DbConnection? tInternalConnection = null;
            try
            {
                tInternalConnection = GetConnection();
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

    #region Initialize/DDL

    public void Initialize()
    {
        CreateTablesIfNeeded();
    }

    public void Zap()
    {
        Init();
        DropTableIfExists("Certificate");
        DropTableIfExists("RolePrivilege");
        DropTableIfExists("UserRole");
        DropTableIfExists("Role");
        DropTableIfExists("User");
    }

    public void CreateTablesIfNeeded()
    {
        Init();
        PreInitDdl();
        using (var tx = InternalConnection!.BeginTransaction())
        {
            CreateUserTableIfNeeded(tx);
            CreateRoleTableIfNeeded(tx);
            CreateUserRoleTableIfNeeded(tx);
            CreateRolePrivilegeTableIfNeeded(tx);
            CreateCertificateTableIfNeeded(tx);

            tx.Commit();
        }

        PostInitDdl();
    }

    #region DDL SQL

    #region Create Table Statements

    protected virtual string c__Sql_Ddl_CreateTable_User { get; } = """
                                                                    CREATE TABLE "User" (
                                                                    	        "UserId"        VARCHAR(100) NOT NULL,
                                                                    	        "FriendlyName"  VARCHAR(100) NOT NULL,
                                                                    	        "IsActive"      INTEGER NOT NULL,
                                                                    	        PRIMARY KEY("UserId")
                                                                                    )  
                                                                    """;

    protected virtual string c__Sql_Ddl_CreateTable_Role { get; } = """
                                                                    CREATE TABLE "Role" (
                                                                    	        "RoleId"       VARCHAR(100) NOT NULL,
                                                                    	        "FriendlyName" VARCHAR(100) NOT NULL,
                                                                    	        PRIMARY KEY("RoleId") 
                                                                                );
                                                                    """;

    protected virtual string c__Sql_Ddl_CreateTable_UserRole { get; } = """
                                                                        CREATE TABLE "UserRole" (
                                                                        	        "UserId" VARCHAR(100) NOT NULL REFERENCES "User"("UserId"),
                                                                        	        "RoleId" VARCHAR(100) NOT NULL REFERENCES "Role"("RoleId"),
                                                                        	        PRIMARY KEY("UserId","RoleId" )
                                                                                    );
                                                                        """;

    protected virtual string c__Sql_Ddl_CreateTable_RolePrivilege { get; } = """
                                                                             CREATE TABLE "RolePrivilege" (
                                                                             	        "RoleId"      VARCHAR(100) NOT NULL REFERENCES "Role"("RoleId"),
                                                                             	        "PrivilegeId" VARCHAR(100) NOT NULL,
                                                                             	        PRIMARY KEY("RoleId","PrivilegeId" )
                                                                                         );
                                                                             """;

    protected virtual string c__Sql_Ddl_CreateTable_Certificate { get; } = """
                                                                           CREATE TABLE "Certificate" (
                                                                                      "CertificateId" VARCHAR(100) NOT NULL,
                                                                                      "IsActive" INTEGER NOT NULL,
                                                                                      "SerialNumber" BIGINT NOT NULL,
                                                                                      "Description" VARCHAR(256) NOT NULL,
                                                                                      "CommonName" VARCHAR(100) NOT NULL,
                                                                                      "CountryCode" VARCHAR(100) NULL,
                                                                                      "StateCode" VARCHAR(100) NULL,
                                                                                      "Locale" VARCHAR(100) NULL,
                                                                                      "Organization" VARCHAR(100) NULL,
                                                                                      "OrganizationalUnit" VARCHAR(100) NULL,
                                                                           	        PRIMARY KEY("CertificateId" )
                                                                                       );
                                                                           """;

    #endregion

    #endregion

    #region Drop Table

    private void DropTableIfExists(string tableName)
    {
        Debug.Assert(tableName != null);
        if (ExistsTable(tableName, null!)) RunDdl($"drop table \"{tableName}\"", null!);
    }

    #endregion

    private void CreateUserTableIfNeeded(DbTransaction tx)
    {
        if (!ExistsTable("User", tx)) RunDdl(c__Sql_Ddl_CreateTable_User, tx);
    }

    private void CreateRoleTableIfNeeded(DbTransaction tx)
    {
        if (!ExistsTable("Role", tx)) RunDdl(c__Sql_Ddl_CreateTable_Role, tx);
    }

    private void CreateUserRoleTableIfNeeded(DbTransaction tx)
    {
        if (!ExistsTable("UserRole", tx)) RunDdl(c__Sql_Ddl_CreateTable_UserRole, tx);
    }

    private void CreateRolePrivilegeTableIfNeeded(DbTransaction tx)
    {
        if (!ExistsTable("RolePrivilege", tx)) RunDdl(c__Sql_Ddl_CreateTable_RolePrivilege, tx);
    }

    private void CreateCertificateTableIfNeeded(DbTransaction tx)
    {
        if (!ExistsTable("Certificate", tx)) RunDdl(c__Sql_Ddl_CreateTable_Certificate, tx);
    }

    protected void RunDdl(string ddlSql, DbTransaction tx)
    {
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = ddlSql;
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Transaction = tx;
            cmd.ExecuteNonQuery();
        }
    }

    #endregion

    #region Transaction Wrappers

    public void TransactionWrap(Func<bool> callback)
    {
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

    #endregion

    #region User

    public void UpdateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """update "User" SET "FriendlyName"=@friendlyName, "IsActive"=@isActive where "UserId"=@userId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@friendlyName", user.FriendlyName));
            cmd.Parameters.Add(CreateParameter("@isActive", user.IsActive ? 1 : 0));
            cmd.Parameters.Add(CreateParameter("@userId", user.UserId));

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
                """insert into "User" ("UserId", "FriendlyName", "IsActive") VALUES (@userId, @friendlyName, @isActive)""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", user.UserId));
            cmd.Parameters.Add(CreateParameter("@friendlyName", user.FriendlyName));
            cmd.Parameters.Add(CreateParameter("@isActive", user.IsActive ? 1 : 0));

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
            cmd.CommandText = """select * from "User" where "UserId" = @userId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));

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
            cmd.CommandText = """select count(1) from "User" where "UserId" = @userId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));

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
            cmd.CommandText = """delete from "User" where "UserId" = @userId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    #endregion

    #region Role

    public void InsertRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """insert into "Role" ("RoleId", "FriendlyName") VALUES (@roleId, @friendlyName)""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", role.RoleId));
            cmd.Parameters.Add(CreateParameter("@friendlyName", role.FriendlyName));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """update "Role" SET "FriendlyName"=@friendlyName where "RoleId"=@roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@friendlyName", role.FriendlyName));
            cmd.Parameters.Add(CreateParameter("@roleId", role.RoleId));

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
            cmd.CommandText = """select count(1) from "Role" where "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));
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
            cmd.CommandText = """select * from "Role" where "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

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
            cmd.CommandText = """delete from "Role" where "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    #endregion

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
            cmd.CommandText = """select count(1) from "UserRole" where "UserId" = @userId and "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

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
            cmd.CommandText = """insert into "UserRole" ("UserId", "RoleId") VALUES (@userId, @roleId)""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userRole.UserId));
            cmd.Parameters.Add(CreateParameter("@roleId", userRole.RoleId));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public IEnumerable<DbUserRole> FetchAllUserRoles()
    {
        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """select * from "UserRole" """;
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);

            cmd.Transaction = CurrentTransaction;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    yield return DbUserRole.CreateFromDataReader(reader);
            }
        }
    }

    public bool ExistsUsersInRole(string roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """select count(1) from "UserRole" where "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

            cmd.Transaction = CurrentTransaction;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public void DeleteUserRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """delete from "UserRole" where "UserId" = @userId and "RoleId" = @roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public IEnumerable<string> GetUserRoleMemberships(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """select distinct "RoleId" from "UserRole" where "UserId" = @userId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));

            cmd.Transaction = CurrentTransaction;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    yield return reader.GetString(0);
            }
        }
    }

    public IEnumerable<string> AllUserIdsWithPrivilege(string privilegeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(privilegeId);
        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """
                              select distinct ur."UserId"
                              from "RolePrivilege" rp
                                inner join "UserRole" ur on (rp."RoleId" = ur."RoleId")
                              where rp."PrivilegeId" = @privilegeId
                              """;
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@privilegeId", privilegeId));

            cmd.Transaction = CurrentTransaction;

            var returnValue = new List<string>();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    returnValue.Add(rdr.GetString(0));
            }

            return returnValue;
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
            cmd.CommandText = """insert into "RolePrivilege" ("RoleId", "PrivilegeId") VALUES (@roleId, @privilegeId)""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", rolePrivilege.RoleId));
            cmd.Parameters.Add(CreateParameter("@privilegeId", rolePrivilege.PrivilegeId));

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
            cmd.CommandText = """
                              select count(1)
                              from "RolePrivilege" rp
                                inner join "UserRole" ur on (rp."RoleId"=ur."RoleId" and rp."PrivilegeId"=@privilegeId and ur."UserId"=@userId)
                              """;
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@userId", userId));
            cmd.Parameters.Add(CreateParameter("@privilegeId", privilegeId));

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
            cmd.CommandText = """delete from "RolePrivilege" where "RoleId"=@roleId and "PrivilegeId"=@privilegeId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));
            cmd.Parameters.Add(CreateParameter("@privilegeId", privilegeId));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    public bool ExistsRolePrivilegesForRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """select count(1) from "RolePrivilege" where "RoleId"=@roleId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@roleId", roleId));

            cmd.Transaction = CurrentTransaction;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    #endregion

    #region Certificates

    public bool ExistsCertificate(string certificateId)
    {
        ArgumentNullException.ThrowIfNull(certificateId);

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """select count(1) from "Certificate" where "CertificateId" = @certificateId""";
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@certificateId", certificateId));

            cmd.Transaction = CurrentTransaction;

            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    public void InsertCertificate(DbCertificate dbCertificate)
    {
        ArgumentNullException.ThrowIfNull(dbCertificate);
        dbCertificate.Validate();

        Init();

        using (var cmd = InternalConnection!.CreateCommand())
        {
            cmd.CommandText = """
                              insert into "Certificate"
                                  ("CertificateId", "IsActive", "SerialNumber", "Description", "CommonName", "CountryCode", "StateCode", "Locale", "Organization", "OrganizationalUnit")
                              values
                                  (@CertificateId, @IsActive, @SerialNumber, @Description, @CommonName, @CountryCode, @StateCode, @Locale, @Organization, @OrganizationalUnit)
                              """;
            cmd.CommandText = SetIdentifierTicks(cmd.CommandText);
            cmd.Parameters.Add(CreateParameter("@CertificateId", dbCertificate.CertificateId));
            cmd.Parameters.Add(CreateParameter("@IsActive", dbCertificate.IsActive ? 1 : 0));
            cmd.Parameters.Add(CreateParameter("@SerialNumber", dbCertificate.SerialNumber));
            cmd.Parameters.Add(CreateParameter("@Description", dbCertificate.Description));
            cmd.Parameters.Add(CreateParameter("@CommonName", dbCertificate.CommonName));
            cmd.Parameters.Add(CreateParameter("@CountryCode", DatabaseHelper.NullDbParam(dbCertificate.CountryCode, true)));
            cmd.Parameters.Add(CreateParameter("@StateCode", DatabaseHelper.NullDbParam(dbCertificate.StateCode, true)));
            cmd.Parameters.Add(CreateParameter("@Locale", DatabaseHelper.NullDbParam(dbCertificate.Locale, true)));
            cmd.Parameters.Add(CreateParameter("@Organization", DatabaseHelper.NullDbParam(dbCertificate.Organization, true)));
            cmd.Parameters.Add(CreateParameter("@OrganizationalUnit", DatabaseHelper.NullDbParam(dbCertificate.OrganizationalUnit, true)));

            cmd.Transaction = CurrentTransaction;

            cmd.ExecuteNonQuery();
        }
    }

    #endregion
}

// ReSharper restore InconsistentNaming