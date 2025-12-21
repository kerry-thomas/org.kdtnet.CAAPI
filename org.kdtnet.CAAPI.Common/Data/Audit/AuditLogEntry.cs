namespace org.kdtnet.CAAPI.Common.Data.Audit;

public class AuditLogEntry
{
    public required string CorrelationId { get; set; }
    public required EAuditLogEntryType EntryType { get; set; }
    public required DateTime OccurrenceUtc { get; set; }
    public required string ActingUserId { get; set; }
    public required string Locus { get; set; }
    public required string Summary { get; set; }
    public required string Detail { get; set; }
}