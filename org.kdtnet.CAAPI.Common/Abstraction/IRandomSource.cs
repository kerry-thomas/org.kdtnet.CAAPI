namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IRandomSource : IDisposable
{
    byte[] GetBytes(int nBytes);
    long GetInt64();
}