namespace Domain.Common;

/// <summary>One permission, as declared by a module's *Permissions class.
/// Code is the immutable identity — see Permission.Code in the entity.</summary>
public sealed record PermissionDefinition(string Code, string Name, string? Description, bool IsBaseline);

/// <summary>One module's full permission set, as declared by that module's
/// *Permissions class. ModuleCode is the immutable identity — see Module.Code.</summary>
public sealed class ModulePermissions
{
    public required string ModuleCode { get; init; }
    public required string ModuleName { get; init; }
    public int SortOrder { get; init; }
    public required IReadOnlyList<PermissionDefinition> Permissions { get; init; }
}