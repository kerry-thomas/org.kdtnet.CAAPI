namespace org.kdtnet.CAAPI.Common.Abstraction.Logging;

public interface ILogFormatter
{
    string FormatMessage(ELogLevel level, string msg);
}