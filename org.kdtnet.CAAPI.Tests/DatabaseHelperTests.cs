using System.Data;
using System.Diagnostics.CodeAnalysis;
using Moq;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class DatabaseHelperTests
{
    private Mock<IDataReader>? MockDataReader { get; set; }
    
    [TestInitialize]
    public void BeforeEachTest()
    {
        MockDataReader = new Mock<IDataReader>();
    }
    
    #region String Tests
    
    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetStringNotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);
        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns("test");

        var s = MockDataReader.Object.GetStringNotNull("somecolumn", true);
        Assert.AreEqual("test", s);
    }
    
    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetStringNotNull_DataValueIsNullBlankEmpty()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);
        
        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns("");
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", true));
        MockDataReader.Object.GetStringNotNull("somecolumn", false);
            
        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns(" ");
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", true));
        MockDataReader.Object.GetStringNotNull("somecolumn", false);
        
                    
        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns((string) null!);
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", true));
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", false));
    }
    
    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetStringNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", true));
    }

    #endregion
    
    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetStringNotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetStringNotNull("somecolumn", true));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetStringNotNull(null!, true));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetStringNotNull("", true));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetStringNotNull(" ", true));
    }

    #endregion
    
    #endregion
    
    #region Guid Tests
    
    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetGuidNotNull()
    {
        var testValue = Guid.NewGuid();
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);
        MockDataReader!.Setup(x => x.GetGuid(It.IsAny<int>())).Returns(testValue);

        var v = MockDataReader.Object.GetGuidNotNull("somecolumn");
        Assert.AreEqual(testValue, v);
    }
    
    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetGuidNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetGuidNotNull("somecolumn"));
    }

    #endregion
    
    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetGuidNotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetGuidNotNull("somecolumn"));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetGuidNotNull(null!));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetGuidNotNull(""));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetGuidNotNull(" "));
    }

    #endregion
    
    #endregion

    #region Bool Tests
    
    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetBoolNotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetBoolean(It.IsAny<int>())).Returns(true);
        var v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(true, v);
        
        MockDataReader!.Setup(x => x.GetBoolean(It.IsAny<int>())).Returns(false);
        v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(false, v);
    }
    
    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetBoolNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetBoolNotNull("somecolumn"));
    }

    #endregion
    
    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetBoolNotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetBoolNotNull("somecolumn"));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetBoolNotNull(null!));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetBoolNotNull(""));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetBoolNotNull(" "));
    }

    #endregion
    
    #endregion

}