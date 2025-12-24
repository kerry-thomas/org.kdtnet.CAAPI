using System.Text;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.RestApi;

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