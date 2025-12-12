using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Engine;

public class ApplicationEngine
{
    private ILogger Logger { get; }
    private IConfigurationSource ConfigurationSource { get; }
    private IDataStore DataStore { get; }

    public ApplicationEngine(ILogger logger, IConfigurationSource configurationSource, IDataStore dataStore)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigurationSource = configurationSource ?? throw new ArgumentNullException(nameof(configurationSource));
        DataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }
}