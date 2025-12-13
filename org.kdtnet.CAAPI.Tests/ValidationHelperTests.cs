using System.Diagnostics.CodeAnalysis;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class ValidationHelperTests
{
    [TestInitialize]
    public void BeforeEachTest()
    {
    }

    #region String Validator Tests
    
    #region Happy Path

    [TestMethod]
    [TestCategory("ValidationHelper.String.HappyPath")]
    public void AssertStringNotNull()
    {
        ValidationHelper.AssertStringNotNull("xxx", false);
        ValidationHelper.AssertStringNotNull(string.Empty, false);
        ValidationHelper.AssertStringNotNull(" ", false);
        
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(null, true));
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(null, false));
        
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(string.Empty, true));
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertStringNotNull(" ", true));
    }
    
    #endregion
    
    #region Grumpy Path
    
    [TestMethod]
    [TestCategory("ValidationHelper.String.HappyPath")]
    public void AssertStringNotNull_NullPropertyName()
    {
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertStringNotNull("XXXX", true, null!));
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertStringNotNull("XXXX", false, null!));
    }
    
    #endregion

    #endregion
    
    #region ConditionValidator Tests
    
    #region Happy Path
    
    [TestMethod]
    [TestCategory("ValidationHelper.Condition.HappyPath")]
    public void AssertCondition()
    {
        ValidationHelper.AssertCondition(() => true, "test condition", "testValue");
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertCondition(() => false, "test condition", "testValue"));
    }

    
    #endregion
    
    #region Grumpy Path
    
    [TestMethod]
    [TestCategory("ValidationHelper.Condition.GrumpyPath")]
    public void AssertCondition_NullParameters()
    {
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertCondition(null!, "test condition", "testValue"));
        
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertCondition(() => true, null!, "testValue"));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertCondition(() => true, string.Empty, "testValue"));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertCondition(() => true, " ", "testValue"));
        
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertCondition(() => true, "test condition", null!));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertCondition(() => true, "test condition", string.Empty));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertCondition(() => true, "test condition", " "));
    }
    
    #endregion
    
    #endregion
    
    #region ObjectNotNullValidator Tests
    
    #region Happy Path
    
    [TestMethod]
    [TestCategory("ValidationHelper.ObjectNotNull.HappyPath")]
    public void AssertObjectNotNull()
    {
        ValidationHelper.AssertObjectNotNull(new object(), "testObject");
        Assert.ThrowsException<ValidationException>(() => ValidationHelper.AssertObjectNotNull(null!, "testObject"));
    }

    
    #endregion
    
    #region Grumpy Path
    
    [TestMethod]
    [TestCategory("ValidationHelper.ObjectNotNull.GrumpyPath")]
    public void AssertObjectNotNull_NullParameters()
    {
        Assert.ThrowsException<ArgumentNullException>(() => ValidationHelper.AssertObjectNotNull(new object(), null!));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertObjectNotNull(() => true, string.Empty));
        Assert.ThrowsException<ArgumentException>(() => ValidationHelper.AssertObjectNotNull(() => true, " "));
    }
    
    #endregion
    
    #endregion
}