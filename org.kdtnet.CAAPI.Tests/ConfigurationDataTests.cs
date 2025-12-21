using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Abstraction.Logging;
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
            },
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
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
            },
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
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
        var cfg1 = new ApplicationConfiguration()
        {
            Logging = null!,
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
            }
        };
        Assert.ThrowsException<ValidationException>(() => cfg1.Validate());
        
        var cfg2 = new ApplicationConfiguration()
        {
            Logging = new ApplicationConfigurationLogging()
                { Level =  ELogLevel.Info },
            DataStore = null!
        };
        Assert.ThrowsException<ValidationException>(() => cfg2.Validate());
    }

    #endregion

    #endregion

}