using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.RestApi;

public class CreateCertificateAuthorityRequest : IValidateable
{
    public required string UniqueId { get; set; }
    public required EAsymmetricKeyType EAsymmetricKeyType { get; set; }
    public required string AsymmetricPrivateKeyPassphrase { get; set; } 
    public required bool CreateIntermediate { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(UniqueId, true);
        ValidationHelper.AssertStringNotNull(AsymmetricPrivateKeyPassphrase, true);
    }
}