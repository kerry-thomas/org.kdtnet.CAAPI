using System.Data;
using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.DbEntity;

public class DbRolePrivilege : IValidateable
{
    public required string RoleId { get; set; }
    public required string PrivilegeId { get; set; } 

    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(RoleId, true);
        ValidationHelper.AssertStringNotNull(PrivilegeId, true);
    }
}