using System.Diagnostics;
using System.Security.Cryptography;
using org.kdtnet.CAAPI.Common.Abstraction;

namespace org.kdtnet.CAAPI.Implementation;

public class DefaultRandomSource : IRandomSource
{
    private readonly object _lockObject = new object();
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public byte[] GetBytes(int nBytes)
    {
        var returnValue = new byte[nBytes];
        lock (_lockObject)
        {
            _rng.GetBytes(returnValue);
        }

        return returnValue;
    }

    public long GetInt64()
    {
        var buffer = GetBytes(8);
        var returnValue = BitConverter.ToInt64(buffer, 0) & 0x7FFFFFFFFFFFFFFF;
        Debug.Assert(returnValue > 0);
        return returnValue;
    }

    public void Dispose()
    {
        _rng.Dispose();
    }
}