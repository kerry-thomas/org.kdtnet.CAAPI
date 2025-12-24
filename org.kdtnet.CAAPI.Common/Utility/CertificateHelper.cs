using System.Net;

namespace org.kdtnet.CAAPI.Common.Utility;

public static class CertificateHelper
{
    public static string DistinguishedNameCondition(this string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Replace(",", "\\,");
    }

    public static byte[] CertificateSerialNumberBytes(long serialNumber)
    {
        var networkLong = IPAddress.HostToNetworkOrder(serialNumber);
        var bytes = BitConverter.GetBytes(networkLong);
        return bytes;
    }
}