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

    #region DataReader Tests
    
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


        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns((string)null!);
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetStringNotNull("somecolumn", true));
        Assert.ThrowsException<DbNullColumnException>(() =>
            MockDataReader.Object.GetStringNotNull("somecolumn", false));
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
    [TestCategory("DatabaseHelper.Guid.HappyPath")]
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
    [TestCategory("DatabaseHelper.Guid.HappyPath")]
    public void GetGuidNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetGuidNotNull("somecolumn"));
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Guid.GrumpyPath")]
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
    [TestCategory("DatabaseHelper.Bool.HappyPath")]
    public void GetBoolNotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(1);
        var v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(true, v);

        MockDataReader!.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(2);
        v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(true, v);

        MockDataReader!.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(-2);
        v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(true, v);
        
        MockDataReader!.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(0);
        v = MockDataReader.Object.GetBoolNotNull("somecolumn");
        Assert.AreEqual(false, v);
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.Bool.HappyPath")]
    public void GetBoolNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetBoolNotNull("somecolumn"));
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Bool.GrumpyPath")]
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

    #region Int32 Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Int32.HappyPath")]
    public void GetInt32NotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetInt32(It.IsAny<int>())).Returns(5);
        var v = MockDataReader.Object.GetInt32NotNull("somecolumn");
        Assert.AreEqual(5, v);
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.Int32.HappyPath")]
    public void GetInt32NotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetInt32NotNull("somecolumn"));
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Int32.GrumpyPath")]
    public void GetInt32NotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetInt32NotNull("somecolumn"));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetInt32NotNull(null!));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetInt32NotNull(""));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetInt32NotNull(" "));
    }

    #endregion

    #endregion

    #region Int64 Tests

    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Int64.HappyPath")]
    public void GetInt64NotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetInt64(It.IsAny<int>())).Returns(5);
        var v = MockDataReader.Object.GetInt64NotNull("somecolumn");
        Assert.AreEqual(5, v);
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.Int64.HappyPath")]
    public void GetInt64NotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetInt64NotNull("somecolumn"));
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.Int64.GrumpyPath")]
    public void GetInt64NotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetInt64NotNull("somecolumn"));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetInt64NotNull(null!));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetInt64NotNull(""));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetInt64NotNull(" "));
    }

    #endregion

    #endregion
    
    #region Enum Tests

    private enum EDbHelperTesting
    {
        Value1,
        Value2,
        Value3,
    }

    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetEnumNotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns(nameof(EDbHelperTesting.Value1));
        var v = MockDataReader.Object.GetEnumNotNull<EDbHelperTesting>("somecolumn");
        Assert.AreEqual(EDbHelperTesting.Value1, v);

        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns(nameof(EDbHelperTesting.Value2));
        v = MockDataReader.Object.GetEnumNotNull<EDbHelperTesting>("somecolumn");
        Assert.AreEqual(EDbHelperTesting.Value2, v);

        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns(nameof(EDbHelperTesting.Value3));
        v = MockDataReader.Object.GetEnumNotNull<EDbHelperTesting>("somecolumn");
        Assert.AreEqual(EDbHelperTesting.Value3, v);
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetEnumNotNull_IsDbBull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() =>
            MockDataReader.Object.GetEnumNotNull<EDbHelperTesting>("somecolumn"));
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetEnumNotNull_BadDbValue()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetString(It.IsAny<int>())).Returns("BAD VALUE");
        Assert.ThrowsException<DbEnumFormatException>(() =>
            MockDataReader.Object.GetEnumNotNull<EDbHelperTesting>("somecolumn"));
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.String.HappyPath")]
    public void GetEnumNotNull_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetEnumNotNull<EDbHelperTesting>("somecolumn"));
        Assert.ThrowsException<ArgumentNullException>(() =>
            MockDataReader!.Object.GetEnumNotNull<EDbHelperTesting>(null!));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetEnumNotNull<EDbHelperTesting>(""));
        Assert.ThrowsException<ArgumentException>(() => MockDataReader!.Object.GetEnumNotNull<EDbHelperTesting>(" "));
    }

    #endregion

    #endregion

    #region List Tests

    private class TestRecord
    {
        public required string StringValue { get; set; }
        public required int IntValue { get; set; }
        public required EDbHelperTesting EnumValue { get; set; }
    }

    #region Happy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.List.HappyPath")]
    public void GetList()
    {
        int recordCount = 0;
        int recordLimit = 3;
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);
        MockDataReader!.Setup(x => x.Read()).Returns(() =>
        {
            recordCount++;
            return recordCount <= recordLimit;
        });

        var v = MockDataReader.Object.GetList<TestRecord>((rdr) => new TestRecord { StringValue = "xxx",  IntValue = 1, EnumValue = EDbHelperTesting.Value1 });
        Assert.IsNotNull(v);
        Assert.AreEqual(recordLimit, v.Count());
    }

    [TestMethod]
    [TestCategory("DatabaseHelper.List.HappyPath")]
    public void GetList_NoRecordsReturnsEmptyList()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);
        MockDataReader!.Setup(x => x.Read()).Returns(false);

        var v = MockDataReader.Object.GetList<TestRecord>((rdr) => new TestRecord { StringValue = "xxx",  IntValue = 1, EnumValue = EDbHelperTesting.Value1 });
        Assert.IsNotNull(v);
        Assert.AreEqual(0, v.Count());
    }

    #endregion

    #region Grumpy Path

    [TestMethod]
    [TestCategory("DatabaseHelper.List.GrumpyPath")]
    public void GetList_BadParams()
    {
        var reader = (IDataReader)null!;

        Assert.ThrowsException<ArgumentNullException>(() => reader.GetList<TestRecord>((rdr) => new TestRecord { StringValue = "xxx",  IntValue = 1, EnumValue = EDbHelperTesting.Value1 }));
        Assert.ThrowsException<ArgumentNullException>(() => MockDataReader!.Object.GetList<TestRecord>(null!));
    }
    
    #endregion

    #endregion
    
    #region Binary Tests
    
    #region  Happy Path
    
    [TestMethod]
    [TestCategory("DatabaseHelper.Binary.HappyPath")]
    public void GetBinaryNotNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        byte[] dataToReturn = [1, 2, 3];
        int callSeq = 0;
        MockDataReader!.Setup(x => x.GetBytes(
            It.IsAny<int>(),
            It.IsAny<long>(),
            It.IsAny<byte[]>(),
            It.IsAny<int>(),
            It.IsAny<int>()
            )).Returns<int, long, byte[], int, int>((a,b,c,d,e) =>
        {
            callSeq++;
            if (callSeq == 1)
            {
                c[0]=dataToReturn[0];
                c[1]=dataToReturn[1];
                c[2]=dataToReturn[2];
                return 3;
            }
            else
            {
                return 0;
            }
        });
        var bytesReturned = MockDataReader.Object.GetBinaryNotNull("somecolumn", true);
        Assert.AreEqual(3, bytesReturned.Length);
        Assert.AreEqual(dataToReturn[0], bytesReturned[0]);
        Assert.AreEqual(dataToReturn[1], bytesReturned[1]);
        Assert.AreEqual(dataToReturn[2], bytesReturned[2]);
    }
    
    [TestMethod]
    [TestCategory("DatabaseHelper.Binary.HappyPath")]
    public void GetBinaryNotNullEmptyArray()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(false);

        MockDataReader!.Setup(x => x.GetBytes(
            It.IsAny<int>(),
            It.IsAny<long>(),
            It.IsAny<byte[]>(),
            It.IsAny<int>(),
            It.IsAny<int>()
        )).Returns<int, long, byte[], int, int>((a, b, c, d, e) => 0);

        var bytesReturned = MockDataReader.Object.GetBinaryNotNull("somecolumn", false);
        Assert.AreEqual(0, bytesReturned.Length);
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetBinaryNotNull("somecolumn", true));
    }

     
    [TestMethod]
    [TestCategory("DatabaseHelper.Binary.HappyPath")]
    public void GetBinaryNotNullDbNull()
    {
        MockDataReader!.Setup(x => x.Read()).Returns(true);
        MockDataReader!.Setup(x => x.IsDBNull(It.IsAny<int>())).Returns(true);

        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetBinaryNotNull("somecolumn", true));
        Assert.ThrowsException<DbNullColumnException>(() => MockDataReader.Object.GetBinaryNotNull("somecolumn", false));
    }

    #endregion
    
    #endregion
    
    #endregion

    #region Parameter Tests
    
    #region Happy Path
    
    [TestMethod]
    [TestCategory("DatabaseHelper.Parameter.HappyPath")]
    public void NullDbParam()
    {
        Assert.AreEqual("xxx", DatabaseHelper.NullDbParam("xxx", true));
        Assert.AreEqual("xxx", DatabaseHelper.NullDbParam("xxx", false));
        
        Assert.AreEqual(DBNull.Value, DatabaseHelper.NullDbParam(null, true));
        Assert.AreEqual(DBNull.Value, DatabaseHelper.NullDbParam(null, false));
        
        Assert.AreEqual(string.Empty, DatabaseHelper.NullDbParam(string.Empty, false));
        Assert.AreEqual(" ", DatabaseHelper.NullDbParam(" ", false));
        Assert.AreEqual(DBNull.Value, DatabaseHelper.NullDbParam(string.Empty, true));
        Assert.AreEqual(DBNull.Value, DatabaseHelper.NullDbParam(" ", true));
    }
    
    #endregion
    
    #region Grumpy Path
    
    #endregion

    #endregion
}