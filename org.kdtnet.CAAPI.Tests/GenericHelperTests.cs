using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.Configuration;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class GenericHelperTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region Happy Path
    
    [TestMethod]
    [TestCategory("GenericHelper.AssertValidPassphrase.HappyPath")]
    public void AssertValidPassphrase()
    {
        var mandates = new ApplicationConfigurationEnginePassphraseMandates()
        {
            MinLength = 8,
            MinDigit = 1,
            MinUpperCase = 1,
            MinLowerCase = 1,
            MinSpecial = 1,
        };
        
        GenericHelper.AssertValidPassphrase("Pa$$word1", mandates);
        Assert.ThrowsException<ApiBadPassphraseException>(() => GenericHelper.AssertValidPassphrase("pa$$", mandates));
        Assert.ThrowsException<ApiBadPassphraseException>(() => GenericHelper.AssertValidPassphrase("pa$$word1", mandates));
        Assert.ThrowsException<ApiBadPassphraseException>(() => GenericHelper.AssertValidPassphrase("Password1", mandates));
        Assert.ThrowsException<ApiBadPassphraseException>(() => GenericHelper.AssertValidPassphrase("Pa$$words", mandates));
        Assert.ThrowsException<ApiBadPassphraseException>(() => GenericHelper.AssertValidPassphrase("PA$$WORD1", mandates));
    }

    #endregion
    
}