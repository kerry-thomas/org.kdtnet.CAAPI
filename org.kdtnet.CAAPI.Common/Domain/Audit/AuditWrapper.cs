using System.Diagnostics;
using org.kdtnet.CAAPI.Common.Abstraction.Audit;
using org.kdtnet.CAAPI.Common.Data.Audit;

namespace org.kdtnet.CAAPI.Common.Domain.Audit;

public class AuditDetailCallbackContext
{
    public required Action<string> DetailCallback { get; set; }
}

public class AuditWrapper
{
    private IAuditLogWriter AuditLogWriter { get; }

    public AuditWrapper(IAuditLogWriter auditLogWriter)
    {
        AuditLogWriter = auditLogWriter ??  throw new ArgumentNullException(nameof(auditLogWriter));
    }

    public void Wrap(string actingUserId, string locus, string summary, string beginDetail, Action<AuditDetailCallbackContext> callback)
    {
        if(string.IsNullOrWhiteSpace(actingUserId)) throw new InvalidOperationException($"acting user id is null or empty");
        ArgumentException.ThrowIfNullOrWhiteSpace(locus);
        ArgumentException.ThrowIfNullOrEmpty(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(beginDetail);
        ArgumentNullException.ThrowIfNull(callback);
        
        Debug.Assert(callback != null);
        
        var correlationId = Guid.NewGuid().ToString();

        void LocalDetailCallback(string detailMessage)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(detailMessage);
            AuditLogWriter.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = actingUserId,
                EntryType = EAuditLogEntryType.Processing,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = detailMessage,
            });
        }
        try
        {
            AuditLogWriter.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = actingUserId,
                EntryType = EAuditLogEntryType.Begin,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = beginDetail,
            });

            callback(new AuditDetailCallbackContext()
            {
                DetailCallback = LocalDetailCallback,
            });

            AuditLogWriter.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = actingUserId,
                EntryType = EAuditLogEntryType.Success,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = "SUCCESS",
            });
        }
        catch (Exception ex)
        {
            AuditLogWriter.Audit(new AuditLogEntry()
            {
                CorrelationId = correlationId,
                ActingUserId = actingUserId,
                EntryType = EAuditLogEntryType.Failure,
                OccurrenceUtc = DateTime.UtcNow,
                Locus = locus,
                Summary = summary,
                Detail = $"EXCEPTION: {ex}",
            });
            throw;
        }

    }
}