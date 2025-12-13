namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface ILogFormatter
{
    string FormatMessage(ELogLevel level, string msg);
}