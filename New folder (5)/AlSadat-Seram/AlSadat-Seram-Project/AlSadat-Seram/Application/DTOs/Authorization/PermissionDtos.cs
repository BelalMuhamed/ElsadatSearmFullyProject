using System.Collections.Generic;

namespace Application.DTOs.Authorization
{
    public class ModuleDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class PermissionDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string qualifiedName { get; set; } = string.Empty;
        public string? description { get; set; }
        public bool isGranted { get; set; }
    }

    public class ModulePermissionsDto
    {
        public int moduleId { get; set; }
        public string moduleName { get; set; } = string.Empty;
        public List<PermissionDto> permissions { get; set; } = new();
    }

    public class PermissionCatalogDto
    {
        public List<ModulePermissionsDto> modules { get; set; } = new();
    }

    /// <summary>Full-replace semantics: an empty permissionIds list revokes everything (deny-by-default, Decision 8).</summary>
    public class AssignUserPermissionsRequest
    {
        public string userId { get; set; } = string.Empty;
        public List<int> permissionIds { get; set; } = new();
    }

    public class UserPermissionsViewDto
    {
        public string userId { get; set; } = string.Empty;
        public List<ModulePermissionsDto> modules { get; set; } = new();
    }
}