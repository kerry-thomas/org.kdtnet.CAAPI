using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.Configuration;

public class ApplicationConfigurationDataStore : IValidateable
{
    public required string ConnectionString { get; init; }
    
    public void Validate()
    {
        ValidationHelper.AssertStringNotNull(ConnectionString);
    }
}