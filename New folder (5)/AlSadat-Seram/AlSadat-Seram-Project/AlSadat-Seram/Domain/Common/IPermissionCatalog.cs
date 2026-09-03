namespace Domain.Common;

/// <summary>
/// The single, compile-time source of truth for every module and permission in
/// the system. Consumed by three places: Program.cs (registers one AddPolicy per
/// permission), DbInitializer (seeds Modules/Permissions by upsert-on-Code), and
/// the permission-management UI (via PermissionController.GetCatalog, which reads
/// what's actually IN the database — a separate, DB-backed read model that this
/// catalog is the origin of, not a duplicate of).
/// </summary>
public interface IPermissionCatalog
{
    IReadOnlyList<ModulePermissions> Modules { get; }
    IReadOnlyList<PermissionDefinition> AllPermissions { get; }
}