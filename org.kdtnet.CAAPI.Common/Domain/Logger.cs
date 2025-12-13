using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Common.Domain;

public class Logger : ILogger
{
    public ELogLevel Level { get; set; }
    private ILogFormatter Formatter { get; }
    private ILogWriter Writer { get; }

    public Logger(IConfigurationSource configurationSource, ILogFormatter formatter, ILogWriter writer)
    {
        ArgumentNullException.ThrowIfNull(configurationSource);
        Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));

        Level = configurationSource.ConfigObject?.Logging?.Level ?? ELogLevel.Info;
    }

    public void Fatal(Func<string> msgCallback) => Write(ELogLevel.Fatal, msgCallback);
    public void Error(Func<string> msgCallback) => Write(ELogLevel.Error, msgCallback);
    public void Warn(Func<string> msgCallback) => Write(ELogLevel.Warn, msgCallback);
    public void Info(Func<string> msgCallback) => Write(ELogLevel.Info, msgCallback);
    public void Debug(Func<string> msgCallback) => Write(ELogLevel.Debug, msgCallback);
    public void Trace(Func<string> msgCallback) => Write(ELogLevel.Trace, msgCallback);

    public void Fatal(Exception ex) => Write(ELogLevel.Fatal, ex);
    public void Error(Exception ex) => Write(ELogLevel.Error, ex);
    public void Warn(Exception ex) => Write(ELogLevel.Warn, ex);
    public void Info(Exception ex) => Write(ELogLevel.Info, ex);
    public void Debug(Exception ex) => Write(ELogLevel.Debug, ex);
    public void Trace(Exception ex) => Write(ELogLevel.Trace, ex);

    private void Write(ELogLevel level, Func<string> msgCallback)
    {
        ArgumentNullException.ThrowIfNull(msgCallback);

        if (level > Level)
            return;
        
        var msg = msgCallback() ?? "[NULL]";
        var finalMessage = Formatter.FormatMessage(level, msg);
        Writer.WriteMessage(finalMessage);
    }

    private void Write(ELogLevel level, Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        Write(level, ex.ToString);
    }
}