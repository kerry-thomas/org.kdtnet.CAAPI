namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface ITimeStampSource
{
    DateTime UtcNow();
    DateTimeOffset UtcNowOffset();
}