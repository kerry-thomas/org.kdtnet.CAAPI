using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Data.RestApi;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ValidationHelperTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region String Validator Tests

    [TestMethod]
    [TestCategory("ValidationHelper.String")]
    public void AssertStringNotNull()
    {
        ValidationHelper.AssertStringNotNull("xxx", "testValue", false);
        ValidationHelper.AssertStringNotNull(string.Empty, "testValue", false);
        ValidationHelper.AssertStringNotNull(" ", "testValue", false);
        
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(null, "testValue", true));
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(null, "testValue", false));
        
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(string.Empty, "testValue", true));
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(" ", "testValue", true));
    }

    #endregion
}

[ExcludeFromCodeCoverage]
[TestClass]
public class RequestValidationTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region CreateCertificateAuthority Tests

    [TestMethod]
    [TestCategory("RequestValidation.CreateCertificateAuthorityRequest")]
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
}