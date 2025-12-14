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

    public IDataStoreTransaction BeginTransaction()
    {
        Init();

        return new SqliteDataStoreTransaction(InternalConnection!);
    }

    public bool PersistUser(DbUser user)
    {
        throw new NotImplementedException();
    }

    public DbUser FetchUser(string userId)
    {
        throw new NotImplementedException();
    }

    public void DeleteUser(string userId)
    {
        throw new NotImplementedException();
    }

    public bool PersistRole(DbRole user)
    {
        throw new NotImplementedException();
    }

    public DbUser FetchRole(string userId)
    {
        throw new NotImplementedException();
    }

    public void DeleteRole(string userId)
    {
        throw new NotImplementedException();
    }

    public bool PersistUserRole(DbUserRole userRole)
    {
        throw new NotImplementedException();
    }

    public DbUserRole FetchUserRole(string userId, string roleId)
    {
        throw new NotImplementedException();
    }

    public void DeleteUserRole(string userId, string roleId)
    {
        throw new NotImplementedException();
    }
}