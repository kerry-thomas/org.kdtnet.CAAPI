using org.kdtnet.CAAPI.Common.Data.Configuration;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IConfigurationSource
{
    ApplicationConfiguration ConfigObject { get; }
}