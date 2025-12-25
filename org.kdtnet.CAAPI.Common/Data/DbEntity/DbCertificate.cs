using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbCertificate: IValidateable
{
    public required string CertificateId { get; set; }
    public string? IssuerCertificateId { get; set; }
    public required bool IsActive { get; set; }
    public required long SerialNumber { get; set; }
    public required string Subject { get; set; }
    public required string Issuer { get; set; }
    public required string Description { get; set; }
    public required string CommonName { get; set; }
    public string? CountryCode { get; set; }
    public string? StateCode { get; set; }
    public string? Locale { get; set; }
    public string? Organization { get; set; }
    public string? OrganizationalUnit { get; set; }
    public required byte[] Pkcs12BinaryWithPrivateKey { get; set; }

    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(CertificateId, true);
        ValidationHelper.AssertCondition(() => SerialNumber > 0, "must be greater than zero", nameof(SerialNumber));
        ValidationHelper.AssertStringNotNull(Description, true);
        ValidationHelper.AssertStringNotNull(CommonName, true);
        ValidationHelper.AssertObjectNotNull(Pkcs12BinaryWithPrivateKey);
        ValidationHelper.AssertCondition(() => Pkcs12BinaryWithPrivateKey.Length > 0, "binary length must be greater than zero", nameof(Pkcs12BinaryWithPrivateKey));
    }

    public static DbCertificate CreateFromDataReader(IDataReader reader)
    {
        var returnValue = new DbCertificate()
        {
            CertificateId = reader.GetStringNotNull(nameof(CertificateId),  true),
            IssuerCertificateId = reader.GetStringNotNull(nameof(IssuerCertificateId),  true),
            IsActive = reader.GetBoolNotNull(nameof(IsActive)),
            SerialNumber = reader.GetInt64NotNull(nameof(SerialNumber)),
            Subject = reader.GetStringNotNull(nameof(Subject), true),
            Issuer = reader.GetStringNotNull(nameof(Issuer), true),
            Description = reader.GetStringNotNull(nameof(Description), true),
            CommonName = reader.GetStringNotNull(nameof(CommonName),  true),
            CountryCode = reader.GetStringNull(nameof(CountryCode),  true),
            StateCode = reader.GetStringNull(nameof(StateCode),  true),
            Locale = reader.GetStringNull(nameof(Locale),  true),
            Organization = reader.GetStringNull(nameof(Organization),  true),
            OrganizationalUnit = reader.GetStringNull(nameof(OrganizationalUnit),  true),
            Pkcs12BinaryWithPrivateKey = reader.GetBinaryNotNull(nameof(Pkcs12BinaryWithPrivateKey), true),
        };
        
        returnValue.Validate();
        return returnValue;
    }
}