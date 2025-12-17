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
        public void CreateUserInRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testUser = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            engine.Initialize();
            engine.CreateUserInRole("r.system.admin",testUser);
            Assert.IsTrue(engine.ExistsUser(testUser.UserId));
        }

        #endregion
        
        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.User.HappyPath")]
        public void CreateDuplicateUserInRole()
        {
            var engine = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, TestDataStore!);
            var testUser = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            var testUserDupe = new DbUser() { UserId = "kdt", FriendlyName = "kerry thomas", IsActive = true };
            engine.Initialize();
            engine.CreateUserInRole("r.system.admin",testUser);
            Assert.ThrowsException<ApiGenericException>(() => engine.CreateUserInRole("r.system.admin",testUserDupe));
        }
        
        #endregion
        
        #endregion
    }
}