namespace Domain.Common;

/// <summary>
/// Keeps one property of an <see cref="AuditedAttribute"/> entity out of the change log's
/// before → after list (a secret, a hash, a large blob).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotAuditedAttribute : Attribute;
