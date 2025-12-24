using System.Text;
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
    public required string AsymmetricPrivateKeyPassphrase { get; set; } 
    public required bool CreateIntermediate { get; set; }
    public required int YearsUntilExpire { get; set; }
    public required int PathLength { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(CertificateId, true);
        ValidationHelper.AssertStringNotNull(Description, true);
        ValidationHelper.AssertObjectNotNull(SubjectNameElements);
        ValidationHelper.AssertStringNotNull(AsymmetricPrivateKeyPassphrase, true);
        ValidationHelper.AssertCondition(() => YearsUntilExpire > 0, "must be greater than 0", nameof(YearsUntilExpire));
        ValidationHelper.AssertCondition(() => PathLength > 0, "must be greater than 0", nameof(PathLength));

        SubjectNameElements.Validate();
    }
}

public class DistinguishedNameElements : IValidateable
{
    public required string CommonName { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }
    public string? Locale { get; set; }
    public string? Organization { get; set; }
    public string? OrganizationalUnit { get; set; }

    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(CommonName, true);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"cn={CommonName.DistinguishedNameCondition()}");
        if (CountryCode != null) sb.Append($",c={CountryCode.DistinguishedNameCondition()}");
        if (StateCode != null) sb.Append($",c={StateCode.DistinguishedNameCondition()}");
        if (Locale != null) sb.Append($",l={Locale.DistinguishedNameCondition()}");
        if (Organization != null) sb.Append($",o={Organization.DistinguishedNameCondition()}");
        if (OrganizationalUnit != null) sb.Append($",ou={OrganizationalUnit.DistinguishedNameCondition()}");
        return sb.ToString();
    }
}

public enum EHashAlgorithm
{
    Md5,
    Sha1,
    Sha256,
    Sha384,
    Sha512,
    Sha3_256,
    Sha3_384,
    Sha3_512,
}
