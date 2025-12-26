// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Abstraction.Logging;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using org.kdtnet.CAAPI.Common.Data.RestApi;
using org.kdtnet.CAAPI.Common.Domain.Audit;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Engine;

public class ApplicationEngine
{
    public const string c__SystemAdmin_Builtin_User = "u.system.admin";
    public const string c__SystemAdmin_Builtin_Role = "r.system.admin";

    #region Internal Properties

    private ILogger Logger { get; }
    private IConfigurationSource ConfigurationSource { get; }
    private IDataStore DataStore { get; }
    private IActingUserIdentitySource ActingUserIdentitySource { get; }
    private AuditWrapper AuditWrapper { get; }
    private ITimeStampSource TimeStampSource { get; }
    private IRandomSource RandomSource { get; }

    #endregion

    #region Constructor

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource, IDataStore dataStore,
        IActingUserIdentitySource actingUserIdentitySource, AuditWrapper auditWrapper, ITimeStampSource timeStampSource,
        IRandomSource randomSource)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
        DataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        ActingUserIdentitySource = actingUserIdentitySource ??
                                   throw new ArgumentNullException(nameof(actingUserIdentitySource));
        AuditWrapper = auditWrapper ?? throw new ArgumentNullException(nameof(auditWrapper));
        TimeStampSource = timeStampSource ?? throw new ArgumentNullException(nameof(timeStampSource));
        RandomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    #endregion

    public void Initialize()
    {
        DataStore.Initialize();
        DataStore.TransactionWrap(() =>
        {
            DataStore.InsertUser(new DbUser()
                { UserId = c__SystemAdmin_Builtin_User, FriendlyName = "System Admin User", IsActive = true });
            DataStore.InsertRole(new DbRole()
                { RoleId = c__SystemAdmin_Builtin_Role, FriendlyName = "System Admin Role" });
            DataStore.InsertRolePrivilege(
                new DbRolePrivilege() { PrivilegeId = nameof(EPrivilege.SystemAdmin), RoleId = c__SystemAdmin_Builtin_Role });
            DataStore.PersistUserRole(new DbUserRole()
                { UserId = c__SystemAdmin_Builtin_User, RoleId = c__SystemAdmin_Builtin_Role });

            return true;
        });
    }

    #region Private Methods

    private void AssertPrivilege(EPrivilege privilegeAsserted)
    {
        Debug.Assert(ActingUserIdentitySource != null);
        Debug.Assert(!string.IsNullOrWhiteSpace(ActingUserIdentitySource.ActingUserId));

        var user = DataStore.FetchUser(ActingUserIdentitySource.ActingUserId);
        if (user == null || !user.IsActive)
            throw new ApiAccessDeniedException();

        if (ActingUserIdentitySource.ActingUserId == c__SystemAdmin_Builtin_User)
            return;
        if (DataStore.ExistsUserRole(ActingUserIdentitySource.ActingUserId, c__SystemAdmin_Builtin_Role))
            return;
        if (!DataStore.ExistsUserInRoleWithPrivilege(ActingUserIdentitySource.ActingUserId,
                privilegeAsserted.ToString()))
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

    private static string GetCurrentMethodName([CallerMemberName] string methodName = "UNKNOWN")
    {
        return methodName;
    }

    #endregion

    #region Administration

    #region User

    public void CreateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.Create,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is creating user:[{user.UserId}]",
            (adcc) =>
            {
                user.Validate();

                AssertPrivilege(EPrivilege.SystemAdmin);

                DataStore.TransactionWrap(() =>
                {
                    if (DataStore.ExistsUser(user.UserId))
                        throw new ApiGenericException($"User {user.UserId} already exists");
                    DataStore.InsertUser(user);
                });
                adcc.DetailCallback($"user {user.UserId} IsActive has been set to {user.IsActive}");
            });
    }

    public void DeleteUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.Delete,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is deleting user:[{userId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                DataStore.TransactionWrap(() =>
                {
                    var currentRoleMemberships = DataStore.GetUserRoleMemberships(userId);
                    if (currentRoleMemberships.Any())
                        throw new ApiGenericException($"User {userId} belongs to role(s): [{StringList(currentRoleMemberships)}]");

                    if (!DataStore.ExistsUser(userId))
                        throw new ApiGenericException($"User {userId} does not exist");

                    DataStore.DeleteUser(userId);
                });
            });
    }

    public void UpdateUser(DbUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.Update,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is updating user:[{user.UserId}]",
            (adcc) =>
            {
                user.Validate();
                AssertPrivilege(EPrivilege.SystemAdmin);
                DataStore.UpdateUser(user);
                adcc.DetailCallback($"user {user.UserId} IsActive has been set to {user.IsActive}");
            });
    }

    public void AddUserIdsToRole(string roleId, IEnumerable<string> userIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        if (userIds == null! || userIds.Count() == 0)
            return;

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Update,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is adding user ids [{StringList(userIds)}] to role:[{roleId}]",
            (adcc) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                if (!DataStore.ExistsRole(roleId))
                    throw new ApiGenericException($"Role {roleId} does not exist");

                IEnumerable<string>? adminsBefore = null;
                IEnumerable<string>? adminsAfter = null;

                DataStore.TransactionWrap(() =>
                {
                    adminsBefore = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                    foreach (var userId in userIds)
                    {
                        if (string.IsNullOrWhiteSpace(userId))
                            throw new ApiGenericException(
                                $"the userid list contains at least one userid that is null/empty/blank");
                        if (!DataStore.ExistsUser(userId))
                            throw new ApiGenericException($"User {userId} does not exist");
                        if (DataStore.ExistsUserRole(userId, roleId))
                            throw new ApiGenericException($"user {userId} already exists in role {roleId}");

                        var newUserRole = new DbUserRole() { UserId = userId, RoleId = roleId };
                        newUserRole.Validate();
                        DataStore.PersistUserRole(newUserRole);
                    }

                    adminsAfter = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                });

                Debug.Assert(adminsBefore != null);
                Debug.Assert(adminsAfter != null);
                
                var newAdmins = adminsAfter.Except(adminsBefore);
                foreach (var newAdmin in newAdmins)
                    adcc.DetailCallback($"user {newAdmin} has been granted admin via role {roleId}");
            });
    }

    public void RemoveUserIdsFromRole(string roleId, IEnumerable<string> userIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        if (userIds == null! || userIds.Count() == 0)
            return;

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Update,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is removing user ids [{StringList(userIds)}] from role:[{roleId}]",
            (adcc) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                if (!DataStore.ExistsRole(roleId))
                    throw new ApiGenericException($"Role {roleId} does not exist");

                IEnumerable<string>? adminsBefore = null;
                IEnumerable<string>? adminsAfter = null;

                DataStore.TransactionWrap(() =>
                {
                    adminsBefore = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                    foreach (var userId in userIds)
                    {
                        if (string.IsNullOrWhiteSpace(userId))
                            throw new ApiGenericException(
                                $"the userid list contains at least one userid that is null/empty/blank");
                        if (!DataStore.ExistsUser(userId))
                            throw new ApiGenericException($"User {userId} does not exist");
                        if (!DataStore.ExistsUserRole(userId, roleId))
                            throw new ApiGenericException($"user {userId} does not exist in role {roleId}");

                        var newUserRole = new DbUserRole() { UserId = userId, RoleId = roleId };
                        newUserRole.Validate();
                        DataStore.DeleteUserRole(userId, roleId);
                    }

                    adminsAfter = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                });

                Debug.Assert(adminsBefore != null);
                Debug.Assert(adminsAfter != null);
                
                var noLongerAdmins = adminsBefore.Except(adminsAfter);
                foreach (var noLongerAdmin in noLongerAdmins)
                    adcc.DetailCallback($"user {noLongerAdmin} has been revoked admin via role {roleId}");
            });
    }

    public bool ExistsUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        bool returnValue = false;

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is checking existence of user:[{userId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsUser(userId);
            });

        return returnValue;
    }

    public DbUser? FetchUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        DbUser? returnValue = null;
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is fetching user:[{userId}]",
            (_) =>
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
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is checking existence of role:[{roleId}]",
            (_) =>
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
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is checking existence of user [{userId}] in role:[{roleId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsUserRole(userId, roleId);
            });

        return returnValue;
    }

    public void UpdateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Update,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is updating role:[{role.RoleId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                role.Validate();

                DataStore.TransactionWrap(() =>
                {
                    if (!DataStore.ExistsRole(role.RoleId))
                        throw new ApiGenericException($"Role {role.RoleId} does not exist");

                    DataStore.UpdateRole(role);
                });
            });
    }

    public void CreateRole(DbRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Create,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is creating role:[{role.RoleId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                role.Validate();

                DataStore.TransactionWrap(() =>
                {
                    if (DataStore.ExistsRole(role.RoleId))
                        throw new ApiGenericException($"Role {role.RoleId} already exists");

                    DataStore.InsertRole(role);
                });
            });
    }

    public void DeleteRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Delete,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is deleting role:[{roleId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                DataStore.TransactionWrap(() =>
                {
                    if (!DataStore.ExistsRole(roleId))
                        throw new ApiGenericException($"Role {roleId} does not exist");

                    if (DataStore.ExistsUsersInRole(roleId))
                        throw new ApiGenericException($"Role {roleId} contains users");

                    if (DataStore.ExistsRolePrivilegesForRole(roleId))
                        throw new ApiGenericException($"Role {roleId} contains privilege grants");

                    DataStore.DeleteRole(roleId);
                });
            });
    }

    public DbRole? FetchRole(string roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        DbRole? returnValue = null;
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is fetching role:[{roleId}]",
            (_) =>
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
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.UserRole.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is fetching all user-role relationships",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.FetchAllUserRoles();
            });

        return returnValue;
    }

    #endregion

    #region RolePrivilege

    public void GrantRolePrivilege(string roleId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Grant,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is granting privilege:[{privilege.ToString()}] to role:[{roleId}]",
            (adcc) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                IEnumerable<string>? adminsBefore = null;
                IEnumerable<string>? adminsAfter = null;
                DataStore.TransactionWrap(() =>
                {
                    adminsBefore = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                    DataStore.InsertRolePrivilege(new DbRolePrivilege()
                        { RoleId = roleId, PrivilegeId = privilege.ToString() });
                    adminsAfter = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                });

                Debug.Assert(adminsBefore != null);
                Debug.Assert(adminsAfter != null);

                var newAdmins = adminsAfter.Except(adminsBefore);
                foreach (var newAdmin in newAdmins)
                    adcc.DetailCallback($"user {newAdmin} has been granted admin via role {roleId}");
            });
    }

    public bool UserHasPrivilege(string userId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        bool returnValue = false;
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.User.CheckPrivilege,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is checking privilege:[{privilege.ToString()}] for user:[{userId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);
                returnValue = DataStore.ExistsUserInRoleWithPrivilege(userId, privilege.ToString());
            });
        return returnValue;
    }

    public void RevokeRolePrivilege(string roleId, EPrivilege privilege)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Administration.Role.Revoke,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is revoking privilege:[{privilege.ToString()}] from role:[{roleId}]",
            (adcc) =>
            {
                AssertPrivilege(EPrivilege.SystemAdmin);

                IEnumerable<string>? adminsBefore = null;
                IEnumerable<string>? adminsAfter = null;
                DataStore.TransactionWrap(() =>
                {
                    adminsBefore = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                    DataStore.DeleteRolePrivilege(roleId, privilege.ToString());
                    adminsAfter = DataStore.AllUserIdsWithPrivilege(nameof(EPrivilege.SystemAdmin));
                });

                Debug.Assert(adminsBefore != null);
                Debug.Assert(adminsAfter != null);

                var noLongerAdmins = adminsBefore.Except(adminsAfter);
                foreach (var noLongerAdmin in noLongerAdmins)
                    adcc.DetailCallback($"user {noLongerAdmin} has been revoked admin via role {roleId}");
            });
    }

    #endregion

    #endregion

    #region Certificates

    public void CreateRootCertificate(CreateCertificateAuthorityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Certificates.Certificate.Create,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is creating creating root certificate:[{request.SubjectNameElements}]",
            (_) =>
            {
                request.Validate();
                GenericHelper.AssertValidPassphrase(request.PrivateKeyPassphrase, ConfigurationSource.ConfigObject.Engine.PassphraseMandates);
                AssertPrivilege(EPrivilege.CertificateAdmin);

                var rightNow = TimeStampSource.UtcNowOffset();
                var RootDN = new X500DistinguishedName(request.SubjectNameElements.ToString());

                CertificateRequest rootCertRequest;
                switch (request.AsymmetricKeyType)
                {
                    case EAsymmetricKeyType.Rsa4096:
                        rootCertRequest = new CertificateRequest(RootDN, RSA.Create(4096), ExtractHashAlgorithmName(request.HashAlgorithm),
                            RSASignaturePadding.Pkcs1);
                        break;
                    case EAsymmetricKeyType.Rsa2048:
                        rootCertRequest = new CertificateRequest(RootDN, RSA.Create(2048), ExtractHashAlgorithmName(request.HashAlgorithm),
                            RSASignaturePadding.Pkcs1);
                        break;
                    case EAsymmetricKeyType.NistP256:
                        rootCertRequest = new CertificateRequest(RootDN, ECDsa.Create(ECCurve.NamedCurves.nistP256), ExtractHashAlgorithmName(request.HashAlgorithm));
                        break;
                    case EAsymmetricKeyType.NistP384:
                        rootCertRequest = new CertificateRequest(RootDN, ECDsa.Create(ECCurve.NamedCurves.nistP384), ExtractHashAlgorithmName(request.HashAlgorithm));
                        break;
                    case EAsymmetricKeyType.NistP521:
                        rootCertRequest = new CertificateRequest(RootDN, ECDsa.Create(ECCurve.NamedCurves.nistP521), ExtractHashAlgorithmName(request.HashAlgorithm));
                        break;
                    default:
                        throw new InvalidOperationException($"unaccounted enum: {request.AsymmetricKeyType}");
                }
                
                rootCertRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, request.PathLength, true));
                rootCertRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(rootCertRequest.PublicKey,
                    X509SubjectKeyIdentifierHashAlgorithm.Sha1, false));
                rootCertRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign,
                    false));

                var rootCert = rootCertRequest.CreateSelfSigned(rightNow, rightNow.AddYears(request.YearsUntilExpire));
                var rootCertPkcs12Bytes = rootCert.Export(X509ContentType.Pfx, request.PrivateKeyPassphrase);
               
                DataStore.TransactionWrap(() =>
                {
                    if (DataStore.ExistsCertificate(request.CertificateId))
                        throw new ApiGenericException($"Certificate {request.CertificateId} already exists");

                    DataStore.InsertCertificate(new DbCertificate()
                    {
                        CertificateId = request.CertificateId,
                        IssuerCertificateId = null,
                        IsActive =  true,
                        SerialNumber = RandomSource.GetInt64(),
                        ThumbPrint = rootCert.Thumbprint,
                        Subject = rootCert.Subject,
                        Issuer = rootCert.Issuer,
                        CommonName = request.SubjectNameElements.CommonName,
                        CountryCode = request.SubjectNameElements.CountryCode,
                        StateCode = request.SubjectNameElements.StateCode,
                        Locale =  request.SubjectNameElements.Locale,
                        Organization = request.SubjectNameElements.Organization,
                        OrganizationalUnit = request.SubjectNameElements.OrganizationalUnit,
                        Description = request.Description,
                        Pkcs12BinaryWithPrivateKey = rootCertPkcs12Bytes,
                    });
                });
            });
    }

    public bool CertificateExists(string certificateId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(certificateId);

        bool returnValue = false;
        AuditWrapper.Wrap(ActingUserIdentitySource.ActingUserId,
            ApplicationLocus.Certificates.Certificate.Fetch,
            $"{ActingUserIdentitySource.ActingUserId}:{GetCurrentMethodName()}",
            $"{ActingUserIdentitySource.ActingUserId} is checking existence of certificate:[{certificateId}]",
            (_) =>
            {
                AssertPrivilege(EPrivilege.CertificateReader);
                returnValue = DataStore.ExistsCertificate(certificateId);
            });

        return returnValue;
    }


    private static HashAlgorithmName ExtractHashAlgorithmName(EHashAlgorithm hashAlgorithm)
    {
        switch (hashAlgorithm)
        {
            // case EHashAlgorithm.Md5: return HashAlgorithmName.MD5;
            // case EHashAlgorithm.Sha1: return HashAlgorithmName.SHA1;
            case EHashAlgorithm.Sha256: return HashAlgorithmName.SHA256;
            case EHashAlgorithm.Sha384: return HashAlgorithmName.SHA384;
            case EHashAlgorithm.Sha512: return HashAlgorithmName.SHA512;
            case EHashAlgorithm.Sha3_256: return HashAlgorithmName.SHA3_256;
            case EHashAlgorithm.Sha3_384: return HashAlgorithmName.SHA3_384;
            case EHashAlgorithm.Sha3_512: return HashAlgorithmName.SHA3_512;
            default: throw new InvalidOperationException($"unaccounted enum: {hashAlgorithm}");
        }
    }

    #endregion
}

// ReSharper restore InconsistentNaming
// ReSharper restore PossibleMultipleEnumeration