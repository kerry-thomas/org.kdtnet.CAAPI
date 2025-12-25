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
                Level = ELogLevel.Info,
            },
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
                TableSchema = "test_schema",
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
                Level = ELogLevel.Info,
            },
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
                TableSchema = "test_schema",
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
            Logging = null!,
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
                TableSchema =  "test_schema",
            }
        };
        Assert.ThrowsException<ValidationException>(() => cfg1.Validate());
        
        var cfg2 = new ApplicationConfiguration()
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
                { Level =  ELogLevel.Info },
            DataStore = null!
        };
        Assert.ThrowsException<ValidationException>(() => cfg2.Validate());
        
        var cfg3 = new ApplicationConfiguration()
        {
            Engine = null!,
            Logging = new ApplicationConfigurationLogging()
                { Level =  ELogLevel.Info },
            DataStore = new ApplicationConfigurationDataStore()
            {
                ConnectionString = "test",
                TableSchema =  "test_schema",
            }
        };
        Assert.ThrowsException<ValidationException>(() => cfg3.Validate());
    }

    #endregion

    #endregion

}