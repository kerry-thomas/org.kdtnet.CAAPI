using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.RestApi;

public enum EAsymmetricKeyType
{
    Rsa2048,
    Rsa4096,
}

public class CreateCertificateAuthorityRequest : IValidateable
{
    public required string UniqueId { get; set; }
    public required EAsymmetricKeyType EAsymmetricKeyType { get; set; }
    public required string AsymmetricPrivateKeyPassphrase { get; set; } 
    public required bool CreateIntermediate { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(UniqueId, nameof(UniqueId), true);
        ValidationHelper.AssertStringNotNull(AsymmetricPrivateKeyPassphrase, nameof(AsymmetricPrivateKeyPassphrase), true);
    }
}

public class CreateCertificateAuthorityResponse
{
}