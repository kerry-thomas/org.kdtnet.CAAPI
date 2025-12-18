using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.Configuration;
using org.kdtnet.CAAPI.Common.Data.DbEntity;
using org.kdtnet.CAAPI.Engine;
using org.kdtnet.CAAPI.Implementation;

namespace org.kdtnet.CAAPI.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApplicationEngineTests
    {
        private Mock<ILogger>? MockLogger { get; set; }
        private Mock<IConfigurationSource>? MockConfigurationSource { get; set; }
        private SqliteDataStore? TestDataStore { get; set; }

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
                    }
                });
            TestDataStore = new SqliteDataStore(MockConfigurationSource.Object);
            Debug.WriteLine("test initialized");
        }


        #region Constructor Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.HappyPath")]
        public void ConstructEngine()
        {
            _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.GrumpyPath")]
        public void ConstructEngineWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(null!, MockConfigurationSource!.Object, TestDataStore!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, null!, TestDataStore!));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, null!));
        }

        #endregion

        #endregion

        #region Initialization Tests

        [TestMethod]
        [TestCategory("ApplicationEngine.Init.HappyPath")]
        public void InitializeEngine()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            engine.Initialize();
        }

        #endregion

        #region User Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateUser()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.Initialize();
            engine.CreateUser(testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateAndFetchUser()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testUser = new DbUser() { UserId = "charlie.brown@peanuts.com", FriendlyName = "charlie brown", IsActive = true };
            engine.Initialize();
            engine.CreateUser(testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
            var user = engine.FetchUser(testUser.UserId);
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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testUser = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            var testUserDupe = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            engine.Initialize();
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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.Initialize();
            engine.CreateRole(testRole);
            Assert.IsTrue(engine.ExistsRole(testRole.RoleId));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateAndFetchRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.Initialize();
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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            var testRoleDupe = new DbRole() { RoleId = "r.test.1", FriendlyName = "role test 1"};
            engine.Initialize();
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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.Initialize();
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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };

            engine.Initialize();
            engine.CreateRole(testRole);

            engine.AddUserIdsToRole(testRole.RoleId, null!);
            engine.AddUserIdsToRole(testRole.RoleId , []);
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.HappyPath")]
        public void FetchAllUserRoles()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            engine.Initialize();

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
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);

            engine.Initialize();
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            engine.CreateRole(testRole);
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole("r.test", ["user.nonexistent"]));
        }

        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddUserToNonexistentRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.Initialize();
            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId + "X", ["charlie.brown", "sally.brown"]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddNullUserIdToRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };
            var testUser2 = new DbUser() { UserId = "sally.brown", FriendlyName = "Sally Brown", IsActive = true };

            engine.Initialize();
            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            engine.CreateUser(testUser2);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId , ["charlie.brown", null!]));
        }
        
        [TestMethod]
        [TestCategory("ApplicationEngine.UserRole.GrumpyPath")]
        public void AddDuplicateUserIdToRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testRole = new DbRole() { RoleId = "r.test", FriendlyName = "Test Role" };
            var testUser1 = new DbUser() { UserId = "charlie.brown", FriendlyName = "Charlie Brown", IsActive = true };

            engine.Initialize();
            engine.CreateRole(testRole);
            engine.CreateUser(testUser1);
            
            Assert.ThrowsException<ApiGenericException>(() => engine.AddUserIdsToRole(testRole.RoleId , [testUser1.UserId, testUser1.UserId]));
        }
        
        #endregion

        #endregion
    }
}