using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.JwtService;
public interface IJwtService
{
    Task<string> GenerateToken(ApplicationUser user);
}
