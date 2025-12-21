using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Abstraction.Audit;
using org.kdtnet.CAAPI.Common.Abstraction.Logging;
using org.kdtnet.CAAPI.Common.Data.Audit;
using org.kdtnet.CAAPI.Common.Data.Configuration;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using org.kdtnet.CAAPI.Common.Domain.Audit;
using org.kdtnet.CAAPI.Engine;
using org.kdtnet.CAAPI.Implementation;

namespace org.kdtnet.CAAPI.Tests
{
    [ExcludeFromCodeCoverage]
    public class TestingActingUserIdentitySource : IActingUserIdentitySource
    {
        public string ActingUserId { get; set; } = ApplicationEngine.c__SystemAdmin_Builtin_User;
    }

    [ExcludeFromCodeCoverage]
    public class TestingAuditLogWriter : IAuditLogWriter
    {
        public List<AuditLogEntry> LogEntries { get; set; } = [];
        public void Audit(AuditLogEntry auditLogEntry)
        {
            LogEntries.Add(auditLogEntry);
        }
    }
    
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApplicationEngineTests
    {
        private Mock<ILogger>? MockLogger { get; set; }
        private Mock<IConfigurationSource>? MockConfigurationSource { get; set; }
        private SqliteDataStore? TestDataStore { get; set; }
        private TestingActingUserIdentitySource? TestActingUserIdentitySource { get; set; }
        private TestingAuditLogWriter? TestAuditLogWriter { get; set; }
        private AuditWrapper? TestAuditWrapper { get; set; }

        [TestInitialize]
        public void BeforeEachTest()
        {
            MockLogger = new Mock<ILogger>();
            MockConfigurationSource = new Mock<IConfigurationSource>();
            MockConfigurationSource.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Data Source=:memory:"
                        //ConnectionString = "Data Source=/home/kdt/caapi.db"
                    }
                });
            TestDataStore = new SqliteDataStore(MockConfigurationSource.Object);
            TestActingUserIdentitySource = new TestingActingUserIdentitySource();
            TestAuditLogWriter = new TestingAuditLogWriter();
            TestAuditWrapper = new AuditWrapper(TestAuditLogWriter);
            Debug.WriteLine("test initialized");
        }

        private ApplicationEngine CreateDefaultEngine()
        {
            var returnValue = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!);
            returnValue.Initialize();

            return returnValue;
        }

        private void AssertAuditLogExists(string locus, string? actingUserId = null)
        {
            actingUserId ??= TestActingUserIdentitySource!.ActingUserId;

            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any((l) => l.ActingUserId == actingUserId && l.Locus == locus),
                    $"Audit log does not exist for userid: [{actingUserId}] and locus: [{locus}]");
        }


        #region Constructor Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.HappyPath")]
        public void ConstructEngine()
        {
            _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.GrumpyPath")]
        public void ConstructEngineWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(null!, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, null!, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, null!, TestActingUserIdentitySource!, TestAuditWrapper!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, null!, TestAuditWrapper!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!,  TestActingUserIdentitySource!, null!));
        }

        #endregion

        #endregion

        #region Initialization Tests

        [TestMethod]
        [TestCategory("ApplicationEngine.Init.HappyPath")]
        public void InitializeEngine()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!);
            engine.Initialize();
        }

        #endregion

        #region User Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.CreateUser(testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
            AssertAuditLogExists(ApplicationLocus.Administration.User.Create);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateAndFetchUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.CreateUser(testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
            var user = engine.FetchUser(testUser.UserId);
            AssertAuditLogExists(ApplicationLocus.Administration.User.Fetch);
            Assert.IsNotNull(user);
            Assert.AreEqual(testUser.UserId, user.UserId);
            Assert.AreEqual(testUser.FriendlyName, user.FriendlyName);
            Assert.AreEqual(testUser.IsActive, user.IsActive);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.GrumpyPath")]
        public void CreateDuplicateUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            var testUserDupe = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            engine.CreateUser(testUser);
            Assert.ThrowsException<ApiGenericException>(() => engine.CreateUser(testUserDupe));
        }

        #endregion

        #endregion
        
        #region Role Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateAndFetchRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
            var role = engine.FetchRole(testRole.RoleId);
            Assert.IsNotNull(role);
            Assert.AreEqual(role.RoleId, testRole.RoleId);
            Assert.AreEqual(role.FriendlyName,  testRole.FriendlyName);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.GrumpyPath")]
        public void CreateDuplicateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            var testRoleDupe = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.ThrowsException<ApiGenericException>(() => engine.CreateRole(testRoleDupe));
        }

        #endregion

        #endregion

        #region UserRole Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.HappyPath")]
        public void AddUsersToRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole("r.test", ["charlie.brown", "sally.brown"]);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
            Assert.IsTrue(engine.ExistsUser(testUser1.UserId));
            Assert.IsTrue(engine.ExistsUser(testUser2.UserId));
            Assert.IsTrue(engine.ExistsUserInRole(testUser1.UserId, testRole.RoleId));
            Assert.IsTrue(engine.ExistsUserInRole(testUser2.UserId, testRole.RoleId));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.HappyPath")]
        public void AddNullAndEmptyListToRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };

            engine.CreateRole(testRole);

            engine.AddUserIdsToRole(testRole.RoleId, null!);
            engine.AddUserIdsToRole(testRole.RoleId , []);
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.HappyPath")]
        public void FetchAllUserRoles()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole("r.test", ["charlie.brown", "sally.brown"]);

            var urs = engine.FetchAllUserRoles();
            
            Assert.IsNotNull(urs);
            Assert.AreEqual(3, urs.Count());

        }
        
        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddNonexistentUserToRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            engine.CreateRole(testRole);
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole("r.test", ["user.nonexistent"]));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddUserToNonexistentRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId + "X", ["charlie.brown", "sally.brown"]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddNullUserIdToRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId , ["charlie.brown", null!]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddDuplicateUserIdToRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId , [testUser1.UserId, testUser1.UserId]));
        }
        
        #endregion

        #endregion
        
        #region Role Privilege Tests
        
        #region Happy Path
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RolePrivilege.HappyPath")]
        public void RolePrivilegeBasicFunction()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "lucy.vanpelt", FriendlyName = "Lucy Van Pelt", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser1.UserId, testUser2.UserId]);
            engine.GrantRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            Assert.IsTrue(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsTrue(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
            engine.RevokeRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            Assert.IsFalse(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RolePrivilege.HappyPath")]
        public void AuditPrivilegeElevationBuiltinSystemAdminRole()
        {
            var engine = CreateDefaultEngine();

            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "lucy.vanpelt", FriendlyName = "Lucy Van Pelt", IsActive = true };

            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole(ApplicationEngine.c__SystemAdmin_Builtin_Role, [testUser1.UserId, testUser2.UserId]);
            
            Assert.IsTrue(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsTrue(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
            
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser1.UserId) && a.Detail.Contains("granted admin")));
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser2.UserId) && a.Detail.Contains("granted admin")));
            Assert.IsFalse(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser3.UserId) && a.Detail.Contains("granted admin")));
            
            engine.RemoveUserIdsFromRole(ApplicationEngine.c__SystemAdmin_Builtin_Role, [testUser1.UserId, testUser2.UserId]);
            
            Assert.IsFalse(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
            
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser1.UserId) && a.Detail.Contains("revoked admin")));
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser2.UserId) && a.Detail.Contains("revoked admin")));
            Assert.IsFalse(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser3.UserId) && a.Detail.Contains("revoked admin")));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RolePrivilege.HappyPath")]
        public void AuditPrivilegeElevationSubordinateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "lucy.vanpelt", FriendlyName = "Lucy Van Pelt", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser1.UserId, testUser2.UserId]);
            engine.GrantRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            
            Assert.IsTrue(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsTrue(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
            
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser1.UserId) && a.Detail.Contains("granted admin")));
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser2.UserId) && a.Detail.Contains("granted admin")));
            Assert.IsFalse(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser3.UserId) && a.Detail.Contains("granted admin")));
            
            engine.RevokeRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            Assert.IsFalse(engine.UserHasPrivilege(testUser1.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser2.UserId, EPrivilege.SystemAdmin));
            Assert.IsFalse(engine.UserHasPrivilege(testUser3.UserId, EPrivilege.SystemAdmin));
            
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser1.UserId) && a.Detail.Contains("revoked admin")));
            Assert.IsTrue(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser2.UserId) && a.Detail.Contains("revoked admin")));
            Assert.IsFalse(TestAuditLogWriter!.LogEntries.Any(a => a.Detail.Contains(testUser3.UserId) && a.Detail.Contains("revoked admin")));
        }
        
        #endregion
        
        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.RolePrivilege.GrumpyPath")]
        public void RolePrivilegeBadArguments()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "lucy.vanpelt", FriendlyName = "Lucy Van Pelt", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser1.UserId, testUser2.UserId]);
            Assert.ThrowsException<ArgumentNullException>(() => engine.GrantRolePrivilege(null!, EPrivilege.SystemAdmin));
            Assert.ThrowsException<ArgumentException>(() => engine.GrantRolePrivilege(string.Empty, EPrivilege.SystemAdmin));
            Assert.ThrowsException<ArgumentException>(() => engine.GrantRolePrivilege(" ", EPrivilege.SystemAdmin));
            engine.GrantRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            Assert.ThrowsException<ArgumentNullException>(() => engine.RevokeRolePrivilege(null!, EPrivilege.SystemAdmin));
            Assert.ThrowsException<ArgumentException>(() => engine.RevokeRolePrivilege(string.Empty, EPrivilege.SystemAdmin));
            Assert.ThrowsException<ArgumentException>(() => engine.RevokeRolePrivilege(" ", EPrivilege.SystemAdmin));
        }
        
        #endregion
        
        #endregion
        
        #region Privilege Assertion Tests
        
        #region Happy Path
        
        [TestMethod]
        [TestCategory("ApplicationEngine.PrivilegeAssertion.HappyPath")]
        public void PrivilegeAssertionBasicFunction()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "lucy.vanpelt", FriendlyName = "Lucy Van Pelt", IsActive = true };
            var testUser4 = new DbUser() { UserId = "linus.vanpelt", FriendlyName = "Linus Van Pelt", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.AddUserIdsToRole(ApplicationEngine.c__SystemAdmin_Builtin_Role, [testUser1.UserId]);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser2.UserId]);
            engine.GrantRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            
            TestActingUserIdentitySource!.ActingUserId = testUser2.UserId;
            engine.CreateUser(testUser3);
            
            TestActingUserIdentitySource!.ActingUserId = testUser3.UserId;
            Assert.ThrowsException<ApiAccessDeniedException>(() => engine.CreateUser(testUser4));
            
            TestActingUserIdentitySource!.ActingUserId = testUser1.UserId;
            engine.CreateUser(testUser4);
        }
        
        #endregion
        
        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.PrivilegeAssertion.GrumpyPath")]
        public void RolePrivilegeNullActingUserId()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };

            TestActingUserIdentitySource!.ActingUserId = null!;

            Assert.ThrowsException<InvalidOperationException>(() => engine.CreateRole(testRole));
        }
        
        #endregion
        
        #endregion

    }
}