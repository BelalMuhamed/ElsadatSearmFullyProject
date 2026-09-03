namespace Domain.Common;

/// <summary>
/// Aggregates every module's *Permissions.All into one collection. Adding a new
/// module means writing one constants class (see EmployeePermissions for the
/// pattern) and adding one line here — nothing else in the system needs editing.
/// </summary>
public sealed class PermissionCatalog : IPermissionCatalog
{
    public IReadOnlyList<ModulePermissions> Modules { get; }
    public IReadOnlyList<PermissionDefinition> AllPermissions { get; }

    public PermissionCatalog()
    {
        Modules = new List<ModulePermissions>
        {
            EmployeePermissions.Module,
            // Add one line per module as each is confirmed — see Part B.
        };

        AllPermissions = Modules.SelectMany(m => m.Permissions).ToList();
    }
}