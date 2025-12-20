using org.kdtnet.CAAPI.Common.Data.AuditLogging;

namespace org.kdtnet.CAAPI.Common.Abstraction;

public interface IAuditLogProvider
{
    void Audit(AuditLogEntry auditLogEntry);
}