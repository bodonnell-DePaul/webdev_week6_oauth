using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoListApi.Data;
using TodoListApi.DTOs;
using TodoListApi.Models;

namespace TodoListApi.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly TodoDbContext _dbContext;

    public AuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthService> logger,
        TodoDbContext dbContext)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<AuthResponse> AuthenticateGoogleAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"]! }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Check if user exists in database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderUserId == payload.Subject);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Provider = "Google",
                    ProviderUserId = payload.Subject,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("New user created: {Email} via Google", user.Email);
            }
            else
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("User logged in: {Email} via Google", user.Email);
            }

            var userInfo = new UserInfo(
                Id: user.Id,
                Email: user.Email,
                Name: user.Name,
                Picture: user.Picture,
                Provider: user.Provider
            );

            var token = GenerateJwtToken(userInfo);
            var refreshTokenString = GenerateRefreshToken();
            
            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResponse(
                Token: token,
                RefreshToken: refreshTokenString,
                ExpiresAt: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
                User: userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google authentication failed");
            throw new UnauthorizedAccessException("Invalid Google token", ex);
        }
    }

    public async Task<AuthResponse> AuthenticateMicrosoftAsync(string code)
    {
        try
        {
            // Exchange authorization code for access token
            var tokenResponse = await ExchangeMicrosoftCodeForTokenAsync(code);
            
            // Get user info from Microsoft Graph
            var microsoftUser = await GetMicrosoftUserInfoAsync(tokenResponse.AccessToken);

            // Check if user exists in database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Provider == "Microsoft" && u.ProviderUserId == microsoftUser.Id);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = microsoftUser.Email,
                    Name = microsoftUser.Name,
                    Picture = microsoftUser.Picture,
                    Provider = "Microsoft",
                    ProviderUserId = microsoftUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("New user created: {Email} via Microsoft", user.Email);
            }
            else
            {
                // Update last login time
                user.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("User logged in: {Email} via Microsoft", user.Email);
            }

            var userInfo = new UserInfo(
                Id: user.Id,
                Email: user.Email,
                Name: user.Name,
                Picture: user.Picture,
                Provider: user.Provider
            );

            var token = GenerateJwtToken(userInfo);
            var refreshTokenString = GenerateRefreshToken();
            
            // Store refresh token in database
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResponse(
                Token: token,
                RefreshToken: refreshTokenString,
                ExpiresAt: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
                User: userInfo
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft authentication failed");
            throw new UnauthorizedAccessException("Invalid Microsoft code", ex);
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Find the refresh token in database with user
        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (storedToken.IsRevoked)
        {
            throw new UnauthorizedAccessException("Refresh token has been revoked");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        var user = storedToken.User;
        var userInfo = new UserInfo(
            Id: user.Id,
            Email: user.Email,
            Name: user.Name,
            Picture: user.Picture,
            Provider: user.Provider
        );

        // Generate new tokens
        var newAccessToken = GenerateJwtToken(userInfo);
        var newRefreshTokenString = GenerateRefreshToken();

        // Revoke old refresh token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Store new refresh token
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenString,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Token refreshed for user: {Email}", user.Email);

        return new AuthResponse(
            Token: newAccessToken,
            RefreshToken: newRefreshTokenString,
            ExpiresAt: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            User: userInfo
        );
    }

    public string GenerateJwtToken(UserInfo user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim("provider", user.Provider),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<MicrosoftTokenResponse> ExchangeMicrosoftCodeForTokenAsync(string code)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var tenantId = _configuration["Authentication:Microsoft:TenantId"];
        var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = _configuration["Authentication:Microsoft:ClientId"]!,
            ["client_secret"] = _configuration["Authentication:Microsoft:ClientSecret"]!,
            ["code"] = code,
            ["redirect_uri"] = "http://localhost:5173/login",  // Must match the frontend redirect URI exactly
            ["grant_type"] = "authorization_code"
        };

        _logger.LogInformation("Exchanging code for token with redirect_uri: {RedirectUri}", requestData["redirect_uri"]);

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestData));
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Microsoft token exchange failed with status {StatusCode}: {Error}", 
                response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode(); // This will throw with the status code
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();
        return tokenResponse!;
    }

    private async Task<UserInfo> GetMicrosoftUserInfoAsync(string accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Access token is null or empty");
            throw new ArgumentNullException(nameof(accessToken), "Access token cannot be null or empty");
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var tokenPrefix = accessToken.Length > 20 ? accessToken.Substring(0, 20) : accessToken;
        _logger.LogInformation("Fetching user info from Microsoft Graph with token: {TokenPrefix}...", tokenPrefix);
        
        var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Microsoft Graph API failed with status {StatusCode}: {Error}", 
                response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode(); // This will throw with the status code
        }

        var userProfile = await response.Content.ReadFromJsonAsync<MicrosoftUserProfile>();
        
        return new UserInfo(
            Id: userProfile!.Id,
            Email: userProfile.Mail ?? userProfile.UserPrincipalName,
            Name: userProfile.DisplayName,
            Picture: null, // Microsoft Graph requires additional call for photo
            Provider: "Microsoft"
        );
    }

    private record MicrosoftTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("scope")] string Scope
    );

    private record MicrosoftUserProfile(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("mail")] string? Mail,
        [property: JsonPropertyName("userPrincipalName")] string UserPrincipalName
    );
}
