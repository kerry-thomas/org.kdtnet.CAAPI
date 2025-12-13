using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbRole: IValidateable
{
    public required string RoleId { get; set; }
    public required string FriendlyName { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(RoleId, true);
        ValidationHelper.AssertStringNotNull(FriendlyName, true);
    }

    public static DbRole CreateFromDataReader(IDataReader reader)
    {
        var returnValue = new DbRole()
        {
            RoleId = reader.GetStringNotNull(nameof(RoleId),  true),
            FriendlyName = reader.GetStringNotNull(nameof(FriendlyName), true),
        };
        
        returnValue.Validate();
        return returnValue;
    }
}