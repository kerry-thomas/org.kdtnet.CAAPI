using org.kdtnet.CAAPI.Common.Data.DbEntity;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IDataStore : IDisposable
{
    #region Transaction
    
    IDataStoreTransaction BeginTransaction();
    
    #endregion
    
    #region User
    
    bool PersistUser(DbUser user);
    DbUser? FetchUser(string userId);
    void DeleteUser(string userId);
    
    #endregion
    
    #region Role
    
    bool PersistRole(DbRole user);
    DbUser FetchRole(string userId);
    void DeleteRole(string userId);
    
    #endregion
    
    #region UserRole
    
    bool PersistUserRole(DbUserRole userRole);
    DbUserRole FetchUserRole(string userId, string roleId);
    void DeleteUserRole(string userId, string roleId);
    
    #endregion

    void Initialize();
}