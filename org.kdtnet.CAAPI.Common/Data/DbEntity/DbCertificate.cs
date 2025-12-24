using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbCertificate: IValidateable
{
    public required string CertificateId { get; set; }
    public required bool IsActive { get; set; }
    public required long SerialNumber { get; set; }
    public required string Description { get; set; }
    public required string CommonName { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }
    public string? Locale { get; set; }
    public string? Organization { get; set; }
    public string? OrganizationalUnit { get; set; }

    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(CertificateId, true);
        ValidationHelper.AssertCondition(() => SerialNumber > 0, "must be greater than zero", nameof(SerialNumber));
        ValidationHelper.AssertStringNotNull(Description, true);
        ValidationHelper.AssertStringNotNull(CommonName, true);
    }

    public static DbCertificate CreateFromDataReader(IDataReader reader)
    {
        var returnValue = new DbCertificate()
        {
            CertificateId = reader.GetStringNotNull(nameof(CertificateId),  true),
            IsActive = reader.GetBoolNotNull(nameof(IsActive)),
            SerialNumber = reader.GetInt64NotNull(nameof(SerialNumber)),
            Description = reader.GetStringNotNull(nameof(Description), true),
            CommonName = reader.GetStringNotNull(nameof(CommonName),  true),
            CountryCode = reader.GetStringNull(nameof(CountryCode),  true),
            StateCode = reader.GetStringNull(nameof(StateCode),  true),
            Locale = reader.GetStringNull(nameof(Locale),  true),
            Organization = reader.GetStringNull(nameof(Organization),  true),
            OrganizationalUnit = reader.GetStringNull(nameof(OrganizationalUnit),  true),
        };
        
        returnValue.Validate();
        return returnValue;
    }
}