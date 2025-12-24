using org.kdtnet.CAAPI.Common.Data.DbEntity;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IDataStore : IDisposable
{
    #region Transaction
    
    /// <summary>
    /// Used when the transaction is committed or rolled back depending on whether application logic
    /// succeeds or fails. The transaction is rolled back if any exception is thrown by the callback
    /// function. 
    /// </summary>
    /// <param name="callback">A function containing DataStore operations that returns true if the
    /// application logic succeeds, or false otherwise.</param>
    void TransactionWrap(Func<bool> callback);
    
    /// <summary>
    /// Used when the transaction is committed or rolled back depending on whether any exception
    /// is thrown by the callback function. 
    /// </summary>
    /// <param name="callback">An action containing DataStore operations.</param>
    void TransactionWrap(Action callback);
    
    #endregion
    
    #region User
    
    bool ExistsUser(string userId);
    public void InsertUser(DbUser user);
    public void UpdateUser(DbUser user);
    DbUser? FetchUser(string userId);
    void DeleteUser(string userId);
    IEnumerable<string> GetUserRoleMemberships(string userId);
    
    #endregion
    
    #region Role
    
    bool ExistsRole(string roleId);
    void InsertRole(DbRole role);
    void UpdateRole(DbRole role);
    DbRole? FetchRole(string roleId);
    void DeleteRole(string roleId);
    
    #endregion
    
    #region UserRole
    
    public bool ExistsUserRole(string userId, string roleId);
    bool PersistUserRole(DbUserRole userRole);
    void DeleteUserRole(string userId, string roleId);
    IEnumerable<DbUserRole> FetchAllUserRoles();
    bool ExistsUsersInRole(string roleId);
    
    #endregion
    
    #region RolePrivilege

    void InsertRolePrivilege(DbRolePrivilege rolePrivilege);
    bool ExistsUserInRoleWithPrivilege(string userId, string privilegeId);
    void DeleteRolePrivilege(string roleId, string privilegeId);
    IEnumerable<string> AllUserIdsWithPrivilege(string privilegeId);
    bool ExistsRolePrivilegesForRole(string roleId);
    
    #endregion

    void Initialize();
    void Zap();

    #region Certificates

    bool ExistsCertificate(string certificateId);
    void InsertCertificate(DbCertificate dbCertificate);
    
    #endregion

}