using Application.DTOs.Audit;
using Application.Services.contract.AuditTrail;
using Domain.Entities.Audit;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using System.Text.Json;

namespace Infrastructure.Services.AuditServices;

/// <summary>
/// Queues an <see cref="AuditAction.Event"/> line on the request's <see cref="AppDbContext"/>;
/// <see cref="AuditEntriesInterceptor"/> stamps who/when and writes it with the next save.
/// </summary>
public sealed class AuditTrail(AppDbContext context) : IAuditTrail
{
    /// <inheritdoc />
    public void Record(AuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        context.AuditEntries.Add(new AuditEntry
        {
            Action = AuditAction.Event,
            Area = auditEvent.Area,
            EventKey = auditEvent.EventKey,
            EntityType = auditEvent.EntityType,
            EntityId = AuditFormatting.Truncate(auditEvent.EntityId, AuditEntry.EntityIdMaxLength),
            Reference = AuditFormatting.Truncate(auditEvent.Reference, AuditEntry.ReferenceMaxLength),
            Note = AuditFormatting.Truncate(auditEvent.Note, AuditEntry.NoteMaxLength),
            Changes = auditEvent.Changes is { Count: > 0 }
                ? JsonSerializer.Serialize(auditEvent.Changes, AuditFormatting.Json)
                : null
        });
    }
}
