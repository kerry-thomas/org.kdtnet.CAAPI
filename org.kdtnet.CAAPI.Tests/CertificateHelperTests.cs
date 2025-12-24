using System.Data;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class CertificateHelperTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region Happy Path
    
    [TestMethod]
    [TestCategory("CertificateHelper.DistinguishedNameCondition.HappyPath")]
    public void DistinguishedNameCondition()
    {
        Assert.AreEqual(string.Empty, ((string)null!).DistinguishedNameCondition());
        Assert.AreEqual(string.Empty, string.Empty.DistinguishedNameCondition());
        Assert.AreEqual(string.Empty, " ".DistinguishedNameCondition());

        Assert.AreEqual("xxx", "xxx".DistinguishedNameCondition());
        Assert.AreEqual(" xxx", " xxx".DistinguishedNameCondition());
        Assert.AreEqual("xxx ", "xxx ".DistinguishedNameCondition());
        Assert.AreEqual(" xxx ", " xxx ".DistinguishedNameCondition());
    }

    [TestMethod]
    [TestCategory("CertificateHelper.CertificateSerialNumberBytes.HappyPath")]
    public void CertificateSerialNumberBytes()
    {
        long testValue = 0x1122334455667788;
        var csnBytes = CertificateHelper.CertificateSerialNumberBytes(testValue);
        Assert.IsNotNull(csnBytes);
        Assert.IsTrue(csnBytes.Length == 8);
        ulong valueBack = BitConverter.ToUInt64(csnBytes, 0); 
        Assert.AreEqual(0x8877665544332211, valueBack);
    }

    #endregion
    
}