using org.kdtnet.CAAPI.Common.Data.Audit;

namespace org.kdtnet.CAAPI.Common.Abstraction.Audit;

public interface IAuditLogWriter
{
    void Audit(AuditLogEntry auditLogEntry);
}