using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.CurrentUserService;
public interface ICurrentUserService
{
    string? UserId { get; }
    ClaimsPrincipal? UserPrincipal { get; }
    public bool IsAuthenticated { get; set; }
}
