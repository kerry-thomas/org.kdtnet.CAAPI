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

        var v2 = Create(); v2.CertificateId = null!;
        Assert.ThrowsException<ValidationException>(() => v2.Validate());
        
        var v3 = Create(); v3.AsymmetricPrivateKeyPassphrase = null!;
        Assert.ThrowsException<ValidationException>(() => v3.Validate());
        return;
        
#warning should throw exceptions on invalid SubjectName elements
#warning should throw exceptions on invalid YearsUntilExpire

        CreateCertificateAuthorityRequest Create()
        {
            return new CreateCertificateAuthorityRequest()
            {
                CertificateId = "TextCa",
                Description = "Test Cert Description",
                SubjectNameElements = new DistinguishedNameElements()
                {
                    CommonName = "MyCommonName",
                    CountryCode = "US",
                    StateCode = "MT",
                    Locale = "Helena",
                    Organization = "MyCompany",
                    OrganizationalUnit = "MyOrganizationalUnit",
                },
                AsymmetricKeyType = EAsymmetricKeyType.Rsa4096,
                HashAlgorithm = EHashAlgorithm.Sha256,
                AsymmetricPrivateKeyPassphrase = "Test123$",
                CreateIntermediate = false,
                YearsUntilExpire = 5,
                PathLength = 2,
            };
        }
    }
    
    #endregion
    
    #region Grumpy Path
    

    #endregion

    #endregion
}