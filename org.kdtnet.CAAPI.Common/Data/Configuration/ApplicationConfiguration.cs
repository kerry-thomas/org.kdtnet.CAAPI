using org.kdtnet.CAAPI.Common.Abstraction;
using org.kdtnet.CAAPI.Common.Utility;

namespace org.kdtnet.CAAPI.Common.Data.Configuration;

public class ApplicationConfiguration : IValidateable
{
    public required ApplicationConfigurationLogging Logging { get; init; }
    public required ApplicationConfigurationDataStore DataStore { get; init; }
    
    public void Validate()
    {
        ValidationHelper.AssertObjectNotNull(Logging);
        ValidationHelper.AssertObjectNotNull(DataStore);
        Logging.Validate();
        DataStore.Validate();
    }
}