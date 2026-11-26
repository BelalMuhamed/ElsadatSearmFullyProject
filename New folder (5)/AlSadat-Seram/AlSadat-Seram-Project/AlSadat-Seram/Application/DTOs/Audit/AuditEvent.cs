namespace Application.DTOs.Audit;

/// <summary>
/// A business event for the change log that a field diff can't express — "deactivated the account
/// of عبير بدوي", "confirmed INV-1042" (D28). Who and when are added by the save, never passed in.
/// </summary>
/// <param name="Area">Screen/module key from <c>Domain.Common.AuditAreas</c>.</param>
/// <param name="EventKey">Localization key of the sentence, e.g. <c>Audit.UserDeactivated</c>.</param>
/// <param name="EntityType">CLR name of the entity the event is about, for the «افتح» link.</param>
/// <param name="EntityId">Its primary key as text.</param>
/// <param name="Reference">Document number or name people search by.</param>
/// <param name="Note">Text the user gave (e.g. the reason); validate its length in the request DTO.</param>
/// <param name="Changes">Optional before → after values (e.g. «نشط ← موقوف»).</param>
public sealed record AuditEvent(
    string Area,
    string EventKey,
    string EntityType,
    string? EntityId,
    string? Reference = null,
    string? Note = null,
    IReadOnlyList<AuditFieldChange>? Changes = null);

/// <summary>One field's value before and after, as the change log shows it.</summary>
public sealed record AuditFieldChange(string Field, string? Old, string? New);
