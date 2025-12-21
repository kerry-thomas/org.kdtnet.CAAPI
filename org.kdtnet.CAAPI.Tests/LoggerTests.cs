using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Abstraction.Logging;
using org.kdtnet.CAAPI.Common.Data.Configuration;
using org.kdtnet.CAAPI.Common.Domain;
using org.kdtnet.CAAPI.Common.Domain.Logging;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class LoggerTests
{
    private Mock<IConfigurationSource>? MockConfigurationSource { get; set; }
    private Mock<ILogFormatter>? MockLogFormatter { get; set; }
    private Mock<ILogWriter>? MockLogWriter { get; set; }

    [TestInitialize]
    public void BeforeEachTest()
    {
        MockConfigurationSource = new Mock<IConfigurationSource>();
        MockConfigurationSource.Setup(s => s.ConfigObject).Returns(
            new ApplicationConfiguration()
            {
                Logging = new ApplicationConfigurationLogging()
                {
                    Level = ELogLevel.Info
                },
                DataStore = new ApplicationConfigurationDataStore()
                {
                    ConnectionString = "test",
                }
            }
        );
        MockLogFormatter = new Mock<ILogFormatter>();
        MockLogWriter = new Mock<ILogWriter>();
    }

    #region Constructor Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("Logger.Ctor.HappyPath")]
    public void Constructor()
    {
        _ = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, MockLogWriter!.Object);
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("Logger.Ctor.HappyPath")]
    public void Constructor_NullArguments()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            _ = new Logger(null!, MockLogFormatter!.Object, MockLogWriter!.Object));
        Assert.ThrowsException<ArgumentNullException>(() =>
            _ = new Logger(MockConfigurationSource!.Object, null!, MockLogWriter!.Object));
        Assert.ThrowsException<ArgumentNullException>(() =>
            _ = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, null!));
    }

    #endregion

    #endregion

    #region Class Method Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("Logger.Methods.HappyPath")]
    public void LogMethods()
    {
        var logger = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, MockLogWriter!.Object);
        logger.Trace(() => "test message");
        logger.Debug(() => "test message");
        logger.Info(() => "test message");
        logger.Warn(() => "test message");
        logger.Error(() => "test message");
        logger.Fatal(() => "test message");

        var ex = new Exception("TEST EXCEPTION");
        logger.Trace(ex);
        logger.Debug(ex);
        logger.Info(ex);
        logger.Warn(ex);
        logger.Error(ex);
        logger.Fatal(ex);
    }

    [TestMethod]
    [TestCategory("Logger.Methods.HappyPath")]
    public void FormatterIsNeverSentANull()
    {
        int formatterCallCount = 0;
        MockLogFormatter!.Setup(l => l.FormatMessage(It.IsAny<ELogLevel>(), It.IsAny<string>()))
            .Returns<ELogLevel, string>((l, s) =>
            {
                formatterCallCount++;
                Assert.IsNotNull(s);
                return s;
            });
        var logger = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, MockLogWriter!.Object);
        logger.Fatal(() => null!);
        Assert.AreEqual(1, formatterCallCount);
    }

    [TestMethod]
    [TestCategory("Logger.Methods.HappyPath")]
    public void FormatterOrWriterNotCalledWhenNotLogging()
    {
        int formatterCallCount = 0;
        int writerCallCount = 0;
        MockLogFormatter!.Setup(l => l.FormatMessage(It.IsAny<ELogLevel>(), It.IsAny<string>()))
            .Returns<ELogLevel, string>((l, s) =>
            {
                formatterCallCount++;
                return s;
            });
        MockLogWriter!.Setup(w => w.WriteMessage(It.IsAny<string>())).Callback<string>(s =>
        {
            writerCallCount++;
        });
        var logger = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, MockLogWriter!.Object)
            {
                Level = ELogLevel.Info
            };
        logger.Debug(() => "xxx");
        Assert.AreEqual(0, formatterCallCount);
        Assert.AreEqual(0, writerCallCount);
        logger.Info(() => "xxx");
        Assert.AreEqual(1, formatterCallCount);
        Assert.AreEqual(1, writerCallCount);
        logger.Warn(() => "xxx");
        Assert.AreEqual(2, formatterCallCount);
        Assert.AreEqual(2, writerCallCount);
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("Logger.Methods.GrumpyPath")]
    public void LogMethods_NullParameters()
    {
        var logger = new Logger(MockConfigurationSource!.Object, MockLogFormatter!.Object, MockLogWriter!.Object);
        var nullCallback = (Func<string>) null!;
        Assert.ThrowsException<ArgumentNullException>(() => logger.Trace(nullCallback!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Debug(nullCallback!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Info(nullCallback!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Warn(nullCallback!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Error(nullCallback!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Fatal(nullCallback!));

        Exception ex = null!;
        Assert.ThrowsException<ArgumentNullException>(() => logger.Trace(ex!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Debug(ex!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Info(ex!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Warn(ex!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Error(ex!));
        Assert.ThrowsException<ArgumentNullException>(() => logger.Fatal(ex!));
    }

    #endregion

    #endregion
}