using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.RestApi;

public class CreateCertificateAuthorityRequest : IValidateable
{
    public required string CertificateId { get; set; }
    public required string Description { get; set; }
    public required DistinguishedNameElements SubjectNameElements { get; set; }
    public required EAsymmetricKeyType AsymmetricKeyType { get; set; }
    public required EHashAlgorithm HashAlgorithm { get; set; }
    public required string PrivateKeyPassphrase { get; set; } 
    public required bool CreateIntermediate { get; set; }
    public required int YearsUntilExpire { get; set; }
    public required int PathLength { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(CertificateId, true);
        ValidationHelper.AssertStringNotNull(Description, true);
        ValidationHelper.AssertObjectNotNull(SubjectNameElements);
        ValidationHelper.AssertStringNotNull(PrivateKeyPassphrase, true);
        ValidationHelper.AssertCondition(() => YearsUntilExpire > 0, "must be greater than 0", nameof(YearsUntilExpire));
        ValidationHelper.AssertCondition(() => PathLength > 0, "must be greater than 0", nameof(PathLength));

        SubjectNameElements.Validate();
    }
}