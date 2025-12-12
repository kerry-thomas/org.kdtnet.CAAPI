using System.Diagnostics;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public enum ElogLevel
{
    Trace = 5,
    Debug = 4,
    Info = 3,
    Warning = 2,
    Error = 1,
    Fatal = 0
}

public interface ILogger
{
    ElogLevel Level { get; set; }
    
    void Trace(Func<string>  message);
    void Trace(Exception ex);
    void Debug(Func<string> message);
    void Debug(Exception ex);
    void Info(Func<string> message);
    void Info(Exception ex);
    void Warning(Func<string> message);
    void Warning(Exception ex);
    void Error(Func<string> message);
    void Error(Exception ex);
    void Fatal(Func<string> message);
    void Fatal(Exception ex);
}