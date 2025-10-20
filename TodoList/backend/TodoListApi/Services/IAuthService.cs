using TodoListApi.DTOs;

namespace TodoListApi.Services;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateGoogleAsync(string idToken);
    Task<AuthResponse> AuthenticateMicrosoftAsync(string code);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    string GenerateJwtToken(UserInfo user);
}
