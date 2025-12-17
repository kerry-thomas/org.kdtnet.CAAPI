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
            DataStore.PersistUser(new DbUser()
                { UserId = "u.system.admin", FriendlyName = "System Admin User", IsActive = false });
            DataStore.PersistRole(new DbRole() { RoleId = "r.system.admin", FriendlyName = "System Admin Role" });
            DataStore.PersistUserRole(new DbUserRole() { UserId = "u.system.admin", RoleId = "r.system.admin" });

            return true;
        });
    }

    public void CreateUserInRole(string roleId, DbUser dbUser)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        ArgumentNullException.ThrowIfNull(dbUser);
        
        DataStore.TransactionWrap(() =>
        {
            if (DataStore.ExistsUser(dbUser.UserId))
                throw new ApiGenericException($"User {dbUser.UserId} already exists");
            if(!DataStore.ExistsRole(roleId))
                throw new ApiGenericException($"Role {roleId} does not exist");
            
            DataStore.PersistUser(dbUser);
            DataStore.PersistUserRole(new DbUserRole() { UserId = dbUser.UserId, RoleId = roleId });

            return true;
        });
    }

    public bool ExistsUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return DataStore.ExistsUser(userId);
    }
}