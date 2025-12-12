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

        [TestInitializeAttribute]
        public void BeforeEachTest()
        {
            MockLogger = new Mock<ILogger>();
            MockConfigurationSource = new Mock<IConfigurationSource>();
            Debug.WriteLine("test initialized");
        }


        #region Constructor Tests

        #region Happy Path

        [TestMethod]
        public void ConstructEngine()
        {
            _ = new ApplicationEngine(MockLogger!.Object, MockConfigurationSource!.Object);
        }

        #endregion

        #region Grumpy Path

        [TestMethod]
        public void ConstructEngineWithNulls()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _ = new ApplicationEngine(null!, MockConfigurationSource!.Object ));
            Assert.ThrowsException<ArgumentNullException>(() => _ = new ApplicationEngine(MockLogger!.Object, null! ));
        }

        #endregion

        #endregion
    }
}