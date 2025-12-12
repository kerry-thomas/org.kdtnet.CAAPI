using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.RestApi;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class RequestValidationTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region CreateCertificateAuthority Tests
    
    #region Happy Path

    [TestMethod]
    [TestCategory("RequestValidation.CreateCertificateAuthorityRequest.HappyPath")]
    public void CreateCertificateAuthorityRequest_Validate()
    {
        var value = Create();
        value.Validate();

        var v2 = Create(); v2.UniqueId = null!;
        Assert.ThrowsException<ValidationException>(() => v2.Validate());
        
        var v3 = Create(); v3.AsymmetricPrivateKeyPassphrase = null!;
        Assert.ThrowsException<ValidationException>(() => v3.Validate());
        return;

        CreateCertificateAuthorityRequest Create()
        {
            return new CreateCertificateAuthorityRequest()
            {
                UniqueId = "TextCa",
                EAsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                AsymmetricPrivateKeyPassphrase = "Test123$",
                CreateIntermediate = false,
            };
        }
    }
    
    #endregion
    
    #region Grumpy Path
    

    #endregion

    #endregion
}