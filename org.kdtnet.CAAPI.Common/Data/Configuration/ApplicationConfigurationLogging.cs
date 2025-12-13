using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Common.Data.Configuration;

public class ApplicationConfigurationLogging : IValidateable
{
    public required ELogLevel Level { get; init; }
    public void Validate()
    {
    }
}