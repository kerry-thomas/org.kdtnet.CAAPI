namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IDataStoreTransaction : IDisposable
{
    void Commit();
    void Rollback();
}