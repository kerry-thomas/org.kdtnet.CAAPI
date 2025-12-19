using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.DbEntity;

namespace org.kdtnet.CAAPI.Engine;

public class ApplicationEngine
{
    public const string c__SystemAdmin_Builtin_User = "u.system.admin";
    public const string c__SystemAdmin_Builtin_Role = "r.system.admin";
    
    private ILogger Logger { get; }
    private IConfigurationSource ConfigurationSource { get; }
    private IDataStore DataStore { get; }
    private IActingUserIdentitySource ActingUserIdentitySource { get; }

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource, IDataStore dataStore, IActingUserIdentitySource actingUserIdentitySource)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
        DataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        ActingUserIdentitySource = actingUserIdentitySource ?? throw new ArgumentNullException(nameof(actingUserIdentitySource));
    }

    public void Initialize()
    {
        DataStore.Initialize();
        DataStore.TransactionWrap(() =>
        {
            DataStore.InsertUser(new DbUser()
                { UserId = c__SystemAdmin_Builtin_User, FriendlyName = "System Admin User", IsActive = false });
            DataStore.PersistRole(new DbRole() { RoleId = c__SystemAdmin_Builtin_Role, FriendlyName = "System Admin Role" });
            DataStore.PersistUserRole(new DbUserRole() { UserId = c__SystemAdmin_Builtin_User, RoleId = c__SystemAdmin_Builtin_User });

            return true;
        });
    }

    private void AssertPrivilege(EPrivilege privilegeAsserted)
    {
        Debug.Assert(ActingUserIdentitySource != null);
        
        if(string.IsNullOrWhiteSpace(ActingUserIdentitySource.UserId))
            throw new InvalidOperationException($"UserId from ActingUserIdentitySource is null/empty/blank");
        
        if (ActingUserIdentitySource.UserId == c__SystemAdmin_Builtin_User)
            return;
        if (DataStore.ExistsUserRole(ActingUserIdentitySource.UserId, c__SystemAdmin_Builtin_Role))
            return;
        if (!DataStore.ExistsUserInRoleWithPrivilege(ActingUserIdentitySource.UserId, privilegeAsserted.ToString()))
            throw new ApiAccessDeniedException();
    }
    
    #region Administration
    
    #region User
    
    public void CreateUser(DbUser user)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentNullException.ThrowIfNull(user);
        user.Validate();
        
        if(DataStore.ExistsUser(user.UserId))
            throw new ApiGenericException($"User {user.UserId} already exists");
        
        DataStore.InsertUser(user);
    }
    
    public void AddUserIdsToRole(string roleId, IEnumerable<string> userIds)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        if (userIds == null! || userIds.Count() == 0)
            return;
        
        if(!DataStore.ExistsRole(roleId))
            throw new ApiGenericException($"Role {roleId} does not exist");
        
        DataStore.TransactionWrap(() =>
        {
            foreach (var userId in userIds)
            {
                if(string.IsNullOrWhiteSpace(userId))
                    throw new ApiGenericException($"the userid list contains at least one userid that is null/empty/blank");
                if (!DataStore.ExistsUser(userId))
                    throw new ApiGenericException($"User {userId} does not exist");
                if(DataStore.ExistsUserRole(userId, roleId))
                    throw new ApiGenericException($"user {userId} already exists in  role {roleId}");

                var newUserRole = new DbUserRole() { UserId = userId, RoleId = roleId };
                newUserRole.Validate();
                DataStore.PersistUserRole(newUserRole);
                //
                // DataStore.PersistUserRole(new DbUserRole() { UserId = userId, RoleId = roleId });
            }
        
            return true;
        });
    }


    public bool ExistsUser(string userId)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.ExistsUser(userId);
    }


    public DbUser? FetchUser(string userId)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.FetchUser(userId);
    }
    
    #endregion

    #region Role

    public bool ExistsRole(string roleId)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return DataStore.ExistsRole(roleId);
    }

    public bool ExistsUserInRole(string userId, string roleId)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        
        return DataStore.ExistsUserRole(userId, roleId);
    }

    public void CreateRole(DbRole role)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentNullException.ThrowIfNull(role);
        role.Validate();
        
        if(DataStore.ExistsRole(role.RoleId))
            throw new ApiGenericException($"Role {role.RoleId} already exists");
        
        DataStore.PersistRole(role);
    }

    public DbRole? FetchRole(string roleId)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return DataStore.FetchRole(roleId);
    }
    
    #endregion
    
    #region UserRole

    public IEnumerable<DbUserRole> FetchAllUserRoles()
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        return DataStore.FetchAllUserRoles();
    }
    
    #endregion
    
    #endregion

    public void GrantRolePrivilege(string roleId, EPrivilege privilege)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        DataStore.InsertRolePrivilege(new DbRolePrivilege() { RoleId= roleId, PrivilegeId = privilege.ToString() });
    }

    public bool UserHasPrivilege(string userId, EPrivilege privilege)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.ExistsUserInRoleWithPrivilege(userId, privilege.ToString());
    }

    public void RevokeRolePrivilege(string roleId, EPrivilege privilege)
    {
        AssertPrivilege(EPrivilege.SystemAdmin);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        DataStore.TransactionWrap(() =>
        {
            DataStore.DeleteRolePrivilege(roleId, privilege.ToString());
            return true;
        });
    }
}