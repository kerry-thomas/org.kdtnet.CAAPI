namespace org.kdtnet.CAAPI.Common.Abstraction.Logging;

public interface ILogger
{
    ELogLevel Level { get; set; }
    
    void Trace(Func<string>  message);
    void Trace(Exception ex);
    void Debug(Func<string> message);
    void Debug(Exception ex);
    void Info(Func<string> message);
    void Info(Exception ex);
    void Warn(Func<string> message);
    void Warn(Exception ex);
    void Error(Func<string> message);
    void Error(Exception ex);
    void Fatal(Func<string> message);
    void Fatal(Exception ex);
}