using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Abstraction.Audit;
using org.kdtnet.CAAPI.Common.Abstraction.Logging;
using org.kdtnet.CAAPI.Common.Data.Audit;
using org.kdtnet.CAAPI.Common.Data.Configuration;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using org.kdtnet.CAAPI.Common.Data.RestApi;
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
            Debug.WriteLine($"{auditLogEntry.OccurrenceUtc:yyyy/MM/dd HH:mm:ss} {auditLogEntry.CorrelationId} {auditLogEntry.ActingUserId} {auditLogEntry.EntryType} {auditLogEntry.Locus} {auditLogEntry.Summary} {auditLogEntry.Detail} ");
        }
    }
    
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApplicationEngineTests
    {
        private Mock<ILogger>? MockLogger { get; set; }
        private Mock<IConfigurationSource>? MockConfigurationSource { get; set; }
        private IDataStore? TestDataStore { get; set; }
        private TestingActingUserIdentitySource? TestActingUserIdentitySource { get; set; }
        private TestingAuditLogWriter? TestAuditLogWriter { get; set; }
        private AuditWrapper? TestAuditWrapper { get; set; }
        private Mock<ITimeStampSource>? MockTimeStampSource { get; set; }

        private void SetForMySql()
        {
            MockConfigurationSource!.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Engine = new ApplicationConfigurationEngine()
                    {
                        PassphraseMandates = new ApplicationConfigurationEnginePassphraseMandates()
                        {
                            MinLength = 8,
                            MinUpperCase = 1,
                            MinLowerCase = 1,
                            MinDigit = 1,
                            MinSpecial = 1,
                        }
                    },
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Server=localhost;Database=caapi;Uid=ucaapi;Pwd=pa$$word;", //mysql
                        TableSchema = "caapi",
                    }
                });
            
            TestDataStore = new MySqlDataStore(MockConfigurationSource.Object);
        }

        private void SetForSqlServer()
        {
            MockConfigurationSource!.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Engine = new ApplicationConfigurationEngine()
                    {
                        PassphraseMandates = new ApplicationConfigurationEnginePassphraseMandates()
                        {
                            MinLength = 8,
                            MinUpperCase = 1,
                            MinLowerCase = 1,
                            MinDigit = 1,
                            MinSpecial = 1,
                        }
                    },
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Server=localhost;Database=caapi;User Id=ucaapi;Password=p@$$w0rd;TrustServerCertificate=True;", //sql server
                        TableSchema = "dbo",
                    }
                });
            
            TestDataStore = new SqlServerDataStore(MockConfigurationSource.Object);
        }

        private void SetForPostgres()
        {
            MockConfigurationSource!.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Engine = new ApplicationConfigurationEngine()
                    {
                        PassphraseMandates = new ApplicationConfigurationEnginePassphraseMandates()
                        {
                            MinLength = 8,
                            MinUpperCase = 1,
                            MinLowerCase = 1,
                            MinDigit = 1,
                            MinSpecial = 1,
                        }
                    },
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Host=127.0.0.1;Port=5432;Database=caapi;Username=ucaapi;Password=pa$$word", //postgres
                        TableSchema = "public",
                    }
                });
            
            TestDataStore = new PostgresDataStore(MockConfigurationSource.Object);
        }

        private void SetForSqlitePhysical()
        {
            MockConfigurationSource!.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Engine = new ApplicationConfigurationEngine()
                    {
                        PassphraseMandates = new ApplicationConfigurationEnginePassphraseMandates()
                        {
                            MinLength = 8,
                            MinUpperCase = 1,
                            MinLowerCase = 1,
                            MinDigit = 1,
                            MinSpecial = 1,
                        }
                    },
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Data Source=/home/kdt/caapi.db", //sqlite physical
                        TableSchema = "public",
                    }
                });
            
            TestDataStore = new SqlitePhysicalDataStore(MockConfigurationSource.Object);
        }

        private void SetForSqliteInMemory()
        {
            MockConfigurationSource!.Setup(c => c.ConfigObject).Returns(
                new ApplicationConfiguration()
                {
                    Engine = new ApplicationConfigurationEngine()
                    {
                        PassphraseMandates = new ApplicationConfigurationEnginePassphraseMandates()
                        {
                            MinLength = 8,
                            MinUpperCase = 1,
                            MinLowerCase = 1,
                            MinDigit = 1,
                            MinSpecial = 1,
                        }
                    },
                    Logging = new ApplicationConfigurationLogging()
                    {
                        Level = ELogLevel.Trace
                    },
                    DataStore = new ApplicationConfigurationDataStore()
                    {
                        ConnectionString = "Data Source=:memory:", //sqlite in-memory
                        TableSchema = "public",
                    }
                });
            
            TestDataStore = new SqliteInMemoryDataStore();
        }

        [TestInitialize]
        public void BeforeEachTest()
        {
            MockLogger = new Mock<ILogger>();
            MockConfigurationSource = new Mock<IConfigurationSource>();
            //SetForMySql();
            //SetForPostgres();
            SetForSqliteInMemory();
            //SetForSqlitePhysical();
            //SetForSqlServer();
            TestDataStore!.Zap();
            TestActingUserIdentitySource = new TestingActingUserIdentitySource();
            TestAuditLogWriter = new TestingAuditLogWriter();
            TestAuditWrapper = new AuditWrapper(TestAuditLogWriter);
            MockTimeStampSource = new Mock<ITimeStampSource>();
            MockTimeStampSource.Setup(tss => tss.UtcNow()).Returns(() => DateTime.UtcNow);
            MockTimeStampSource.Setup(tss => tss.UtcNowOffset()).Returns(() => DateTimeOffset.UtcNow);
            Debug.WriteLine("test initialized");
        }

        private ApplicationEngine CreateDefaultEngine()
        {
            var returnValue = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object);
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
            _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.GrumpyPath")]
        public void ConstructEngineWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(null!, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, null!, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, null!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, null!, TestAuditWrapper!, MockTimeStampSource!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!,  TestActingUserIdentitySource!, null!, MockTimeStampSource!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!,  TestActingUserIdentitySource!, TestAuditWrapper!, null!));
        }

        #endregion

        #endregion

        #region Initialization Tests

        [TestMethod]
        [TestCategory("ApplicationEngine.Init.HappyPath")]
        public void InitializeEngine()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!, TestActingUserIdentitySource!, TestAuditWrapper!, MockTimeStampSource!.Object);
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
        public void DeleteUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.CreateUser(testUser);
            engine.DeleteUser(testUser.UserId);
            Assert.IsFalse(engine.ExistsUser(testUser.UserId));
            AssertAuditLogExists(ApplicationLocus.Administration.User.Delete);
        }
        
        
        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void UpdateUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.CreateUser(testUser);
            testUser.IsActive = false;
            testUser.FriendlyName = "charlie brown 2";
            engine.UpdateUser(testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
            var updatedUser = engine.FetchUser(testUser.UserId);
            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(false, updatedUser.IsActive);
            Assert.AreEqual("charlie brown 2", updatedUser.FriendlyName);
            
            AssertAuditLogExists(ApplicationLocus.Administration.User.Update);
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

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void FetchNonexistentUserReturnsNull()
        {
            var engine = CreateDefaultEngine();

            var user = engine.FetchUser("abc");
            Assert.IsNull(user);
            AssertAuditLogExists(ApplicationLocus.Administration.User.Fetch);
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

        [TestMethod]
        [TestCategory("ApplicationEngine.User.GrumpyPath")]
        public void DeleteUserCurrentlyInRole()
        {
            var engine = CreateDefaultEngine();

            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testRole = new DbRole() {RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateUser(testUser1);
            engine.CreateRole(testRole);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser1.UserId]);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.DeleteUser(testUser1.UserId));
            Assert.IsTrue(engine.ExistsUser(testUser1.UserId));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.User.GrumpyPath")]
        public void DeleteNonExistentUser()
        {
            var engine = CreateDefaultEngine();

            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.CreateUser(testUser);
            Assert.ThrowsException<ApiGenericException>(() => engine.DeleteUser(testUser.UserId + "X"));
            AssertAuditLogExists(ApplicationLocus.Administration.User.Delete);
        }

        #endregion

        #endregion
        
        #region Role Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.HappyPath")]
        public void CreateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
            AssertAuditLogExists(ApplicationLocus.Administration.Role.Create);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.HappyPath")]
        public void UpdateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
            
            testRole.FriendlyName = "role test 1 ex";
            engine.UpdateRole(testRole);
            
            AssertAuditLogExists(ApplicationLocus.Administration.Role.Update);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.HappyPath")]
        public void DeleteRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));

            engine.DeleteRole(testRole.RoleId);
            Assert.IsFalse(engine.ExistsRole(testRole.RoleId));

            AssertAuditLogExists(ApplicationLocus.Administration.Role.Delete);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.HappyPath")]
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

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.HappyPath")]
        public void FetchNonexistentRoleReturnsNull()
        {
            var engine = CreateDefaultEngine();

            var role = engine.FetchRole("abc");
            Assert.IsNull(role);
            AssertAuditLogExists(ApplicationLocus.Administration.Role.Fetch);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.GrumpyPath")]
        public void UpdateNonexistentRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));

            testRole.RoleId = "r.test.2";
            testRole.FriendlyName = "role test 2 ex";
            Assert.ThrowsException<ApiGenericException>(() => engine.UpdateRole(testRole));
            
            AssertAuditLogExists(ApplicationLocus.Administration.Role.Update);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.GrumpyPath")]
        public void CreateDuplicateRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            var testRoleDupe = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.ThrowsException<ApiGenericException>(() => engine.CreateRole(testRoleDupe));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.GrumpyPath")]
        public void DeleteNonexistentRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() {RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));

            var nonexistentRoleId = testRole.RoleId + "XXX";
            Assert.IsFalse(engine.ExistsRole(nonexistentRoleId));
            
            Assert.ThrowsException<ApiGenericException>(() => engine.DeleteRole(nonexistentRoleId));

            AssertAuditLogExists(ApplicationLocus.Administration.Role.Delete);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.GrumpyPath")]
        public void DeleteRoleWithUsers()
        {
            var engine = CreateDefaultEngine();

            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testRole = new DbRole() {RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateUser(testUser1);
            engine.CreateRole(testRole);
            engine.AddUserIdsToRole(testRole.RoleId, [testUser1.UserId]);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.DeleteRole(testRole.RoleId));

            AssertAuditLogExists(ApplicationLocus.Administration.Role.Delete);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.Role.GrumpyPath")]
        public void DeleteRoleWithPrivileges()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() {RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.CreateRole(testRole);
            engine.GrantRolePrivilege(testRole.RoleId, EPrivilege.SystemAdmin);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.DeleteRole(testRole.RoleId));

            AssertAuditLogExists(ApplicationLocus.Administration.Role.Delete);
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
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void RemoveEmptyAndNullListOfUserIdFromRoleNoError()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            
            engine.RemoveUserIdsFromRole(testRole.RoleId , []);
            engine.RemoveUserIdsFromRole(testRole.RoleId, null!);
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
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId, ["user.nonexistent"]));
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
        public void RemoveUserFromNonexistentRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId + "X", ["charlie.brown", "sally.brown"]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void RemoveNonExistentUserFromRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            engine.CreateRole(testRole);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId, ["user.nonexistent"]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void RemoveNullBlankEmptyUserIdFromRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            engine.CreateRole(testRole);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId, [null!]));
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId, [string.Empty]));
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId, [" "]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void RemoveUserNotInRoleFromRole()
        {
            var engine = CreateDefaultEngine();

            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };
            var testUser3 = new DbUser() { UserId = "linus.vanpelt", FriendlyName = "Linus.VanPelt", IsActive = true };

            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            engine.CreateUser(testUser3);
            engine.AddUserIdsToRole(ApplicationEngine.c__SystemAdmin_Builtin_Role, [testUser1.UserId, testUser2.UserId]);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.RemoveUserIdsFromRole(testRole.RoleId, [testUser3.UserId]));
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
        public void InactiveUserAccessDenied()
        {
            var engine = CreateDefaultEngine();

            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = false };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.CreateUser(testUser1);
            engine.AddUserIdsToRole(ApplicationEngine.c__SystemAdmin_Builtin_Role, [testUser1.UserId]);
            
            TestActingUserIdentitySource!.ActingUserId = testUser1.UserId;
            Assert.ThrowsException<ApiAccessDeniedException>(() => engine.CreateUser(testUser2));
        }

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
        
        #region Certificate Tests
        
        #region Root Certificates
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.HappyPath")]
        public void CreateRootCertificateRsa2048()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = "US",
                    StateCode = "NY",
                    Locale = "Utica",
                    Organization = "Test Organization",
                    OrganizationalUnit = "Test Organization PKI Division",
                },
                AsymmetricKeyType = EAsymmetricKeyType.Rsa2048,
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "Pa$$word1",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            engine.CreateRootCertificate(newRootCert);
            Assert.IsTrue(engine.CertificateExists(newRootCert.CertificateId));
            
            
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.HappyPath")]
        public void CreateRootCertificateRsa4096()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = "US",
                    StateCode = "NY",
                    Locale = "Utica",
                    Organization = "Test Organization",
                    OrganizationalUnit = "Test Organization PKI Division",
                },
                AsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "Pa$$word1",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            engine.CreateRootCertificate(newRootCert);
            Assert.IsTrue(engine.CertificateExists(newRootCert.CertificateId));
            
            
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.HappyPath")]
        public void CreateDuplicateRootCertificate()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = "US",
                    StateCode = "NY",
                    Locale = "Utica",
                    Organization = "Test Organization",
                    OrganizationalUnit = "Test Organization PKI Division",
                },
                AsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "Pa$$word1",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            engine.CreateRootCertificate(newRootCert);
            Assert.IsTrue(engine.CertificateExists(newRootCert.CertificateId));
            Assert.ThrowsException<ApiGenericException>(() => engine.CreateRootCertificate(newRootCert));
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.HappyPath")]
        public void CreateRootCertificateVariousHashAlgorithms()
        {
            var engine = CreateDefaultEngine();

            CreateCertificateAuthorityRequest makeRequest(string certId, EHashAlgorithm hashAlgorithm)
            {
                return new CreateCertificateAuthorityRequest()
                {
                    CertificateId = certId,
                    Description = "Test Cert Description",
                    SubjectNameElements = new DistinguishedNameElements()
                    {
                        CommonName = "Testing CA Root",
                        CountryCode = "US",
                        StateCode = "NY",
                        Locale = "Utica",
                        Organization = "Test Organization",
                        OrganizationalUnit = "Test Organization PKI Division",
                    },
                    AsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                    HashAlgorithm = hashAlgorithm,
                    PrivateKeyPassphrase = "Pa$$word1",
                    CreateIntermediate = false,
                    YearsUntilExpire = 10,
                    PathLength = 2,
                };
            }

           // engine.CreateRootCertificate(makeRequest("cert00", EHashAlgorithm.Md5));
           // engine.CreateRootCertificate(makeRequest("cert01", EHashAlgorithm.Sha1));
            engine.CreateRootCertificate(makeRequest("cert02", EHashAlgorithm.Sha256));
            engine.CreateRootCertificate(makeRequest("cert03", EHashAlgorithm.Sha384));
            engine.CreateRootCertificate(makeRequest("cert04", EHashAlgorithm.Sha512));
            engine.CreateRootCertificate(makeRequest("cert05", EHashAlgorithm.Sha3_256));
            engine.CreateRootCertificate(makeRequest("cert06", EHashAlgorithm.Sha3_384));
            engine.CreateRootCertificate(makeRequest("cert07", EHashAlgorithm.Sha3_512));
            Assert.ThrowsException<InvalidOperationException>(() => engine.CreateRootCertificate(makeRequest("cert08", (EHashAlgorithm) 999)));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.HappyPath")]
        public void CreateRootCertificateWithNullCnElements()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = null,
                    StateCode = null,
                    Locale = null,
                    Organization = null,
                    OrganizationalUnit = null,
                },
                AsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "Pa$$word1",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            engine.CreateRootCertificate(newRootCert);
            Assert.IsTrue(engine.CertificateExists(newRootCert.CertificateId));
            
            
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }
        
        #region Grumpy Path
        
        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.GrumpyPath")]
        public void CreateRootCertificateBadEnum()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = "US",
                    StateCode = "NY",
                    Locale = "Utica",
                    Organization = "Test Organization",
                    OrganizationalUnit = "Test Organization PKI Division",
                },
                AsymmetricKeyType = ((EAsymmetricKeyType) 999),
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "Pa$$word1",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            Assert.ThrowsException<InvalidOperationException>(() => engine.CreateRootCertificate(newRootCert));
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.RootCertificates.GrumpyPath")]
        public void CreateRootCertificateBadPassphrase()
        {
            var engine = CreateDefaultEngine();

            var newRootCert = new CreateCertificateAuthorityRequest()
            {
                CertificateId = "my.rootcert",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "Testing CA Root",
                    CountryCode = "US",
                    StateCode = "NY",
                    Locale = "Utica",
                    Organization = "Test Organization",
                    OrganizationalUnit = "Test Organization PKI Division",
                },
                AsymmetricKeyType = ((EAsymmetricKeyType) 999),
                HashAlgorithm = EHashAlgorithm.Sha256,
                PrivateKeyPassphrase = "pa$$word",
                CreateIntermediate = false,
                YearsUntilExpire = 10,
                PathLength = 2,
            };
            Assert.ThrowsException<ApiBadPassphraseException>(() => engine.CreateRootCertificate(newRootCert));
            AssertAuditLogExists(ApplicationLocus.Certificates.Certificate.Create);
        }
        
        #endregion
        
        #endregion
        
        #endregion

    }
}