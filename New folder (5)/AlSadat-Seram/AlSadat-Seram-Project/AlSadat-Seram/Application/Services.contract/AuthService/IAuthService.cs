using Application.DTOs.Authentcation;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.AuthService;
public interface IAuthService
{
    //Task<Result<string>> RegisterAsync(RegisterDto request);
    Task<Result<AuthResponse>> LoginAsync(string email,string password);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken,string ipAddress);
    Task<Result<AuthResponse>> LoginWithGoogleAsync(GoogleSignInVM model,string ipAddress);
    Task<Result<string>> LogoutAsync(LogoutDto request);
    Task<Result<IEnumerable<RoleDTO>>> GetInactiveRolesAsync(CancellationToken CancellationToken = default);
    Task<Result<IEnumerable<RoleDTO>>> GetSoftDeletedRolesAsync(CancellationToken CancellationToken = default);
    Task<Result<IEnumerable<RoleDTO>>> GetAllRolesAsync(CancellationToken CancellationToken = default);
    Task<Result<RoleDTO>> GetRoleByIdAsync(string RoleID, CancellationToken CancellationToken = default);
    Task<Result<string>> CreateRoleAsync(CreateRoleRequestDTO createRoleRequestDTO, CancellationToken CancellationToken = default);
    Task<Result<string>> UpdateRoleAsync(string RoleID, CreateRoleRequestDTO createRoleRequestDTO, CancellationToken CancellationToken = default);
    Task<Result<string>> SoftDeleteRoleAsync(string RoleID, CancellationToken CancellationToken = default);
    Task<Result<string>> RestoreRoleAsync(string RoleID, CancellationToken CancellationToken = default);
    Task<Result<string>> HardDeleteRoleAsync(string RoleID, CancellationToken CancellationToken = default);

}
