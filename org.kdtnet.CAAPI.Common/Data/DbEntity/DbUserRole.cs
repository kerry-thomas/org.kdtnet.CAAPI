using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbUserRole: IValidateable
{
    public required string UserId { get; set; }
    public required string RoleId { get; set; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(UserId, true);
        ValidationHelper.AssertStringNotNull(RoleId, true);
    }

    public static DbUserRole CreateFromDataReader(IDataReader reader)
    {
        var returnValue = new DbUserRole()
        {
            UserId = reader.GetStringNotNull(nameof(UserId),  true),
            RoleId = reader.GetStringNotNull(nameof(RoleId),  true),
        };
                
        returnValue.Validate();
        return returnValue;

    }
}