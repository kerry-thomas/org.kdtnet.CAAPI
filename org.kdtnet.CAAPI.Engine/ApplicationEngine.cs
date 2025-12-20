using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.AuditLogging;
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
    private IAuditLogProvider AuditLogProvider { get; }

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource, IDataStore dataStore, IActingUserIdentitySource actingUserIdentitySource, IAuditLogProvider  auditLogProvider)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
        DataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        ActingUserIdentitySource = actingUserIdentitySource ?? throw new ArgumentNullException(nameof(actingUserIdentitySource));
        AuditLogProvider = auditLogProvider ?? throw new ArgumentNullException(nameof(auditLogProvider));
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

    private void AuditWrap(string locus, string summary, string detail, Action<string> callback)
    {
        Debug.Assert(callback != null);
        
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            AuditLogProvider.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = ActingUserIdentitySource.UserId,
                EntryType = EAuditLogEntryType.Begin,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = detail,
            });

            callback(correlationId);

            AuditLogProvider.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = ActingUserIdentitySource.UserId,
                EntryType = EAuditLogEntryType.Success,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = detail,
            });
        }
        catch (Exception ex)
        {
            AuditLogProvider.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = ActingUserIdentitySource.UserId,
                EntryType = EAuditLogEntryType.Failure,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = detail,
            });
            throw;
        }

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
    
    private static string StringList(IEnumerable<string> userIds)
    {
        var sb = new StringBuilder();
        foreach (var userId in userIds)
        {
            sb.Append($"{userId}|");
        }

        if (sb.Length > 0)
            sb.Length = sb.Length - 1;
        
        return sb.ToString();
    }
    
    #region Administration
    
    #region User
    
    public void CreateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        AuditWrap(ApplicationLocus.Administration.User.Create,
            $"{ActingUserIdentitySource.UserId}:{nameof(CreateUser)}",
            $"{ActingUserIdentitySource.UserId} is creating user:[{user.UserId}]",
            (cid) =>
            {
                user.Validate();

                AssertPrivilege(EPrivilege.SystemAdmin);

                if (DataStore.ExistsUser(user.UserId))
                    throw new ApiGenericException($"User {user.UserId} already exists");

                DataStore.InsertUser(user);
            });
    }
    
    public void AddUserIdsToRole(string roleId, IEnumerable<string> userIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        if (userIds == null! || userIds.Count() == 0)
            return;

        AuditWrap(ApplicationLocus.Administration.Role.Update,
            $"{ActingUserIdentitySource.UserId}:{nameof(AddUserIdsToRole)}",
            $"{ActingUserIdentitySource.UserId} is adding user ids [{StringList(userIds)}] to role:[{roleId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                if (!DataStore.ExistsRole(roleId))
                    throw new ApiGenericException($"Role {roleId} does not exist");

                DataStore.TransactionWrap(() =>
                {
                    foreach (var userId in userIds)
                    {
                        if (string.IsNullOrWhiteSpace(userId))
                            throw new ApiGenericException(
                                $"the userid list contains at least one userid that is null/empty/blank");
                        if (!DataStore.ExistsUser(userId))
                            throw new ApiGenericException($"User {userId} does not exist");
                        if (DataStore.ExistsUserRole(userId, roleId))
                            throw new ApiGenericException($"user {userId} already exists in  role {roleId}");

                        var newUserRole = new DbUserRole() { UserId = userId, RoleId = roleId };
                        newUserRole.Validate();
                        DataStore.PersistUserRole(newUserRole);
                    }

                    return true;
                });
            });
    }

    public bool ExistsUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        bool returnValue = false;
        
        AuditWrap(ApplicationLocus.Administration.User.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(ExistsUser)}",
            $"{ActingUserIdentitySource.UserId} is checking existence of user:[{userId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue= DataStore.ExistsUser(userId);
            });
        
        return returnValue;
    }

    public DbUser? FetchUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        DbUser? returnValue = null;
        AuditWrap(ApplicationLocus.Administration.User.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(FetchUser)}",
            $"{ActingUserIdentitySource.UserId} is fetching user:[{userId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.FetchUser(userId);
            });
        
        return returnValue;
    }
    
    #endregion

    #region Role

    public bool ExistsRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        bool returnValue = false;
        AuditWrap(ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(ExistsRole)}",
            $"{ActingUserIdentitySource.UserId} is checking existence of role:[{roleId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsRole(roleId);
            });
        
        return returnValue;
    }

    public bool ExistsUserInRole(string userId, string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        bool returnValue = false;
        AuditWrap(ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(ExistsUserInRole)}",
            $"{ActingUserIdentitySource.UserId} is checking existence of user [{userId}] in role:[{roleId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsUserRole(userId, roleId);
            });
        
        return returnValue;
    }

    public void CreateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        AuditWrap(ApplicationLocus.Administration.Role.Create,
            $"{ActingUserIdentitySource.UserId}:{nameof(CreateRole)}",
            $"{ActingUserIdentitySource.UserId} is creating role:[{role.RoleId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                role.Validate();

                if (DataStore.ExistsRole(role.RoleId))
                    throw new ApiGenericException($"Role {role.RoleId} already exists");

                DataStore.PersistRole(role);
            });
    }

    public DbRole? FetchRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        DbRole? returnValue = null;
        AuditWrap(ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(FetchRole)}",
            $"{ActingUserIdentitySource.UserId} is fetching role:[{roleId}]",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.FetchRole(roleId);
            });
        return returnValue;
    }
    
    #endregion
    
    #region UserRole

    public IEnumerable<DbUserRole> FetchAllUserRoles()
    {
        IEnumerable<DbUserRole> returnValue = [];
        AuditWrap(ApplicationLocus.Administration.UserRole.Fetch,
            $"{ActingUserIdentitySource.UserId}:{nameof(FetchAllUserRoles)}",
            $"{ActingUserIdentitySource.UserId} is fetching all user-role relationships",
            (cid) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.FetchAllUserRoles();
            });
        
        return returnValue;
    }
    
    #endregion
    
    #endregion

    public void GrantRolePrivilege(string roleId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        AuditWrap(ApplicationLocus.Administration.Role.Grant,
            $"{ActingUserIdentitySource.UserId}:{nameof(GrantRolePrivilege)}",
            $"{ActingUserIdentitySource.UserId} is granting privilege:[{privilege.ToString()}] to role:[{roleId}]",
            (cid) =>
            {
#warning Do a Process Audit if role is granted system admin 
                AssertPrivilege(EPrivilege.SystemAdmin);
                DataStore.InsertRolePrivilege(new DbRolePrivilege()
                    { RoleId = roleId, PrivilegeId = privilege.ToString() });
            });
    }

    public bool UserHasPrivilege(string userId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        bool returnValue = false;
        AuditWrap(ApplicationLocus.Administration.User.CheckPrivilege,
            $"{ActingUserIdentitySource.UserId}:{nameof(UserHasPrivilege)}",
            $"{ActingUserIdentitySource.UserId} is checking privilege:[{privilege.ToString()}] for user:[{userId}]",
            (cid) =>
            {
#warning Do a Process Audit if role is granted system admin
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsUserInRoleWithPrivilege(userId, privilege.ToString());
            });
        return returnValue;
    }

    public void RevokeRolePrivilege(string roleId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        AuditWrap(ApplicationLocus.Administration.Role.Revoke,
            $"{ActingUserIdentitySource.UserId}:{nameof(RevokeRolePrivilege)}",
            $"{ActingUserIdentitySource.UserId} is revoking privilege:[{privilege.ToString()}] from role:[{roleId}]",
            (cid) =>
            {
#warning Do a Process Audit if role is revoked system admin
                AssertPrivilege(EPrivilege.SystemAdmin);
                DataStore.TransactionWrap(() =>
                {
                    DataStore.DeleteRolePrivilege(roleId, privilege.ToString());
                    return true;
                });
            });
    }
}