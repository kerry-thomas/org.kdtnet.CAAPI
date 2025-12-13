using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbUser: IValidateable
{
    public required string UserId { get; set; }
    public required string FriendlyName { get; set; }
    public required bool IsActive { get; set; }

    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(UserId, true);
        ValidationHelper.AssertStringNotNull(UserId, true);
    }

    public static DbUser CreateFromDataReader(IDataReader reader)
    {
        var returnValue = new DbUser()
        {
            UserId = reader.GetStringNotNull(nameof(UserId),  true),
            FriendlyName = reader.GetStringNotNull(nameof(FriendlyName), true),
            IsActive = reader.GetBoolNotNull(nameof(IsActive)),
        };
        
        returnValue.Validate();
        return returnValue;
    }
}