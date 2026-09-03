using System.Collections.Generic;

namespace Application.Services.contract.JwtService;

public sealed record TokenGenerationRequest(
    string UserId,
    string UserName,
    string Email,
    string Role,
    bool IsSuperAdmin,
    IReadOnlyCollection<string> Permissions);

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtService
{
    AccessTokenResult GenerateToken(TokenGenerationRequest request);
}