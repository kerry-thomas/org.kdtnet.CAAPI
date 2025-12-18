using org.kdtnet.CAAPI.Common.Data.DbEntity;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IDataStore : IDisposable
{
    #region Transaction
    
    void TransactionWrap(Func<bool> callback);
    
    #endregion
    
    #region User
    
    bool ExistsUser(string dbUserUserId);
    public void InsertUser(DbUser user);
    public void UpdateUser(DbUser user);
    DbUser? FetchUser(string userId);
    void DeleteUser(string userId);
    
    #endregion
    
    #region Role
    
    bool ExistsRole(string dbUserUserId);
    bool PersistRole(DbRole role);
    DbRole? FetchRole(string roleId);
    void DeleteRole(string roleId);
    
    #endregion
    
    #region UserRole
    
    public bool ExistsUserRole(string userId, string roleId);
    bool PersistUserRole(DbUserRole userRole);
    DbUserRole? FetchUserRole(string userId, string roleId);
    void DeleteUserRole(string userId, string roleId);
    IEnumerable<DbUserRole> FetchAllUserRoles();
    
    #endregion

    void Initialize();

}