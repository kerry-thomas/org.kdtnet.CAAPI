using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.DbEntity;

namespace org.kdtnet.CAAPI.Engine;

public class ApplicationEngine
{
    private ILogger Logger { get; }
    private IConfigurationSource ConfigurationSource { get; }
    private IDataStore DataStore { get; }

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource, IDataStore dataStore)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
        DataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    public void Initialize()
    {
        DataStore.Initialize();
        DataStore.TransactionWrap(() =>
        {
            DataStore.InsertUser(new DbUser()
                { UserId = "u.system.admin", FriendlyName = "System Admin User", IsActive = false });
            DataStore.PersistRole(new DbRole() { RoleId = "r.system.admin", FriendlyName = "System Admin Role" });
            DataStore.PersistUserRole(new DbUserRole() { UserId = "u.system.admin", RoleId = "r.system.admin" });

            return true;
        });
    }
    
    #region Administration
    
    #region User
    
    public void CreateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Validate();
        
        if(DataStore.ExistsUser(user.UserId))
            throw new ApiGenericException($"User {user.UserId} already exists");
        
        DataStore.InsertUser(user);
    }
    
    public void AddUserIdsToRole(string roleId, IEnumerable<string> userIds)
    {
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
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.ExistsUser(userId);
    }


    public DbUser? FetchUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.FetchUser(userId);
    }
    
    #endregion

    #region Role

    public bool ExistsRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return DataStore.ExistsRole(roleId);
    }

    public bool ExistsUserInRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        
        return DataStore.ExistsUserRole(userId, roleId);
    }

    public void CreateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);
        role.Validate();
        
        if(DataStore.ExistsRole(role.RoleId))
            throw new ApiGenericException($"Role {role.RoleId} already exists");
        
        DataStore.PersistRole(role);
    }

    public DbRole? FetchRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        return DataStore.FetchRole(roleId);
    }
    
    #endregion
    
    #region UserRole

    public IEnumerable<DbUserRole> FetchAllUserRoles()
    {
        return DataStore.FetchAllUserRoles();
    }
    
    #endregion
    
    #endregion
}