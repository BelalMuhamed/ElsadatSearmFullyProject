using Application.DTOs.Audit;

namespace Application.Services.contract.AuditTrail;

/// <summary>
/// Adds business events to the change log. Field changes of <c>[Audited]</c> entities are recorded
/// automatically; use this only for what a diff can't say.
/// </summary>
public interface IAuditTrail
{
    /// <summary>
    /// Queues the event on the current unit of work. It is written by the caller's next
    /// <c>SaveChanges</c>, in the same transaction as the change it describes, so a rolled-back
    /// operation leaves no line behind. Nothing is written if the caller never saves.
    /// </summary>
    void Record(AuditEvent auditEvent);
}
