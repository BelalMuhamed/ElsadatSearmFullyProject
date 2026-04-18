using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authentcation
{
    public record RoleDTO
    (
         string RoleID ,
         string RoleName ,
         DateTime CreatedAt ,
         bool isDeleted


    );

    public record CreateRoleRequestDTO
    (
         string RoleName 
    );
}
