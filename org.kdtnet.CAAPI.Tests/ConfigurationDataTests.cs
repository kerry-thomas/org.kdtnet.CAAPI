using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.Configuration;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ConfigurationDataTests
{
    #region Constructor Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("ConfigurationData.Ctor.HappyPath")]
    public void ConstructEngine()
    {
        var cfg = new ApplicationConfiguration()
        {
            Logging = new ApplicationConfigurationLogging()
            {
                Level = ELogLevel.Info,
            }
        };
    }

    #endregion

    #region Grumpy Path

    #endregion

    #endregion
    
    #region Validation Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("ConfigurationData.Validation.HappyPath")]
    public void ValidateNormally()
    {
        var cfg = new ApplicationConfiguration()
        {
            Logging = new ApplicationConfigurationLogging()
            {
                Level = ELogLevel.Info,
            }
        };
        cfg.Validate();
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("ConfigurationData.Validation.GrumpyPath")]
    public void ValidateFailure()
    {
        var cfg = new ApplicationConfiguration()
        {
            Logging = null!
        };
        Assert.ThrowsException<ValidationException>(() => cfg.Validate());
    }

    #endregion

    #endregion

}