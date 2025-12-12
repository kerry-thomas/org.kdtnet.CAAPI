using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Engine;

public class ApplicationEngine
{
    private ILogger Logger { get; }
    private IConfigurationSource ConfigurationSource { get; }

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
    }
}