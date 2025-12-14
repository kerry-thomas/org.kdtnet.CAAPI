using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Engine;

namespace org.kdtnet.CAAPI.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ApplicationEngineTests
    {
        private Mock<ILogger>? MockLogger { get; set; }
        private Mock<IConfigurationSource>? MockConfigurationSource { get; set; }
        private Mock<IDataStore>? MockDataStore { get; set; }

        [TestInitialize]
        public void BeforeEachTest()
        {
            MockLogger = new Mock<ILogger>();
            MockConfigurationSource = new Mock<IConfigurationSource>();
            MockDataStore = new Mock<IDataStore>();
            Debug.WriteLine("test initialized");
        }


        #region Constructor Tests

        #region Happy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.HappyPath")]
        public void ConstructEngine()
        {
            _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, MockDataStore!.Object);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        [TestCategory("ApplicationEngine.Ctor.GrumpyPath")]
        public void ConstructEngineWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(null!, MockConfigurationSource!.Object, MockDataStore!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, null!, MockDataStore!.Object));
            Assert.ThrowsException<ArgumentNullException>(() =>
                _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object, null!));
        }

        #endregion

        #endregion
        
        #region Initialization Tests
        
        #warning Use SQLite in-memory DB
        
        #endregion
    }
}