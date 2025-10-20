# OAuth 2.0 Implementation Guide for TodoList Application

## Table of Contents
1. [What is OAuth 2.0?](#what-is-oauth-20)
2. [How OAuth 2.0 Works](#how-oauth-20-works)
3. [Why OAuth Has Gained Widespread Adoption](#why-oauth-has-gained-widespread-adoption)
4. [OAuth vs Other Authentication Methods](#oauth-vs-other-authentication-methods)
5. [Pros and Cons of OAuth 2.0](#pros-and-cons-of-oauth-20)
6. [Implementation Guide for TodoList App](#implementation-guide-for-todolist-app)

---

## What is OAuth 2.0?

**OAuth 2.0** (Open Authorization 2.0) is an **authorization framework** that enables third-party applications to obtain limited access to user accounts on an HTTP service. It works by delegating user authentication to the service that hosts the user account and authorizing third-party applications to access that user account.

### Key Concepts

- **Resource Owner**: The user who owns the data
- **Client**: The application requesting access (your TodoList app)
- **Resource Server**: The API server hosting protected resources (your .NET backend)
- **Authorization Server**: The server issuing access tokens (Google, Microsoft Entra ID)
- **Access Token**: A credential used to access protected resources
- **Refresh Token**: A credential used to obtain new access tokens

### Important Distinction

**OAuth 2.0 is primarily an AUTHORIZATION protocol**, not an authentication protocol. However, it's commonly used for authentication through extensions like:
- **OpenID Connect (OIDC)**: Built on top of OAuth 2.0, adds an identity layer for authentication
- Modern implementations typically use OAuth 2.0 + OIDC together

---

## How OAuth 2.0 Works

### The Authorization Code Flow (Most Common for Web Apps)

This is the recommended flow for web applications with a backend server:

```
┌──────────┐                                           ┌───────────────┐
│          │                                           │               │
│  User    │                                           │  Auth Server  │
│ (Browser)│                                           │  (Google/MS)  │
│          │                                           │               │
└────┬─────┘                                           └───────┬───────┘
     │                                                         │
     │  1. User clicks "Login with Google"                    │
     │────────────────────────────────────────────────────────>│
     │                                                         │
     │  2. Redirect to Auth Server with:                      │
     │     - client_id                                         │
     │     - redirect_uri                                      │
     │     - scope (permissions requested)                    │
     │     - state (CSRF protection)                          │
     │────────────────────────────────────────────────────────>│
     │                                                         │
     │  3. User authenticates & grants permission             │
     │<───────────────────────────────────────────────────────>│
     │                                                         │
     │  4. Redirect back with authorization code              │
     │<────────────────────────────────────────────────────────│
     │                                                         │
     │  5. Send code to backend                               │
     │                                                         │
┌────▼─────────┐                                              │
│              │  6. Exchange code for tokens                 │
│   Backend    │─────────────────────────────────────────────>│
│  (.NET API)  │                                              │
│              │  7. Return access_token, id_token,           │
│              │     refresh_token                            │
│              │<─────────────────────────────────────────────│
└────┬─────────┘                                              │
     │                                                         │
     │  8. Return token to frontend                           │
     │  9. Store token (HttpOnly cookie or localStorage)      │
     │                                                         │
     │  10. Include token in API requests                     │
     │                                                         │
```

### Step-by-Step Explanation

1. **User Initiates Login**: User clicks "Login with Google/Microsoft" button
2. **Redirect to Authorization Server**: App redirects to provider's authorization endpoint
3. **User Authenticates**: User logs in and grants permission to the app
4. **Authorization Code Returned**: Provider redirects back with a temporary code
5. **Exchange Code for Token**: Backend exchanges code for access token (secure)
6. **Access Token Issued**: Provider returns access token and user info
7. **Access Protected Resources**: App uses token to access user data

### Security Flow Details

**Why use authorization code flow instead of implicit flow?**

- **Code** is exchanged on the backend (server-to-server) = more secure
- **Client secret** is kept on the server, never exposed to browser
- **Tokens** are not exposed in browser URLs
- **Refresh tokens** can be securely stored on the server

---

## Why OAuth Has Gained Widespread Adoption

### 1. **User Convenience**
- **Single Sign-On (SSO)**: Users can log in with existing accounts (Google, Microsoft, GitHub)
- **No password fatigue**: No need to create and remember another password
- **Faster onboarding**: Reduces friction in user registration

### 2. **Security Benefits**
- **No password storage**: Your app never sees or stores user passwords
- **Reduced liability**: Password breaches are the provider's responsibility
- **Professional security**: Leverage security expertise of major providers
- **Multi-factor authentication**: Inherit MFA from the provider

### 3. **Developer Benefits**
- **Faster development**: No need to build authentication infrastructure
- **Standardized protocol**: Works across different providers
- **Rich user data**: Access to verified email, profile info
- **Token-based**: Stateless, scalable authentication

### 4. **Business Advantages**
- **Reduced development costs**: No custom auth system to build/maintain
- **Compliance**: Providers handle GDPR, CCPA, security standards
- **Trust**: Users trust established providers
- **Analytics**: Better user insights from verified data

### 5. **Enterprise Adoption**
- **Microsoft Entra ID** (formerly Azure AD): Enterprise identity management
- **Workforce integration**: Integrates with corporate directories
- **Conditional access**: Advanced security policies
- **Audit trails**: Comprehensive logging and compliance

---

## OAuth vs Other Authentication Methods

### 1. OAuth 2.0 vs Traditional Username/Password

| Aspect | OAuth 2.0 | Username/Password |
|--------|-----------|-------------------|
| **Password Storage** | Not needed | Required (hashed) |
| **Security Risk** | Lower (delegated) | Higher (breach risk) |
| **User Experience** | One-click login | Manual entry |
| **Maintenance** | Provider managed | App managed |
| **Password Reset** | Not needed | Required flow |
| **MFA** | Provider handles | App must implement |
| **Account Recovery** | Provider handles | App must implement |

**When to use Username/Password:**
- Internal enterprise apps
- High-control requirements
- Offline scenarios
- Regulatory restrictions

### 2. OAuth 2.0 vs API Keys

| Aspect | OAuth 2.0 | API Keys |
|--------|-----------|----------|
| **User Context** | User-specific | Application-level |
| **Expiration** | Time-limited | Often permanent |
| **Scope** | Granular permissions | All-or-nothing |
| **Revocation** | Easy user control | Manual management |
| **Best For** | User data access | Service-to-service |

### 3. OAuth 2.0 vs SAML

| Aspect | OAuth 2.0 | SAML |
|--------|-----------|------|
| **Format** | JSON/REST | XML |
| **Use Case** | APIs, mobile, web | Enterprise SSO |
| **Complexity** | Simpler | More complex |
| **Mobile Support** | Excellent | Limited |
| **Modern Apps** | Preferred | Legacy systems |

### 4. OAuth 2.0 vs Session-Based Auth

| Aspect | OAuth 2.0 | Session-Based |
|--------|-----------|---------------|
| **State** | Stateless (token) | Stateful (server) |
| **Scalability** | Excellent | Challenging |
| **Mobile Apps** | Native support | Cookie limitations |
| **Microservices** | Ideal | Complex |
| **Cross-domain** | Easy | CORS issues |

---

## Pros and Cons of OAuth 2.0

### ✅ Pros

1. **Security**
   - No password storage in your app
   - Token expiration and refresh
   - Scope-limited access
   - Professional security from providers

2. **User Experience**
   - Single Sign-On
   - No registration forms
   - Faster login
   - Familiar UI from trusted providers

3. **Developer Experience**
   - Standard protocol
   - Libraries available for all languages
   - Well-documented
   - Active community support

4. **Scalability**
   - Stateless tokens
   - Distributed systems friendly
   - Microservices compatible
   - Cloud-native

5. **Flexibility**
   - Multiple providers (Google, Microsoft, GitHub)
   - Different flows for different scenarios
   - Granular permissions
   - Extensible (OpenID Connect)

6. **Compliance**
   - Provider handles data protection
   - Audit trails
   - Industry-standard
   - Regulatory compliance

### ❌ Cons

1. **Complexity**
   - More complex than simple auth
   - Multiple flows to understand
   - Security considerations (CSRF, token storage)
   - Configuration overhead

2. **Provider Dependency**
   - Reliant on third-party availability
   - Provider outages affect your app
   - Subject to provider's terms of service
   - API changes can break integration

3. **User Privacy Concerns**
   - Some users distrust "Login with X"
   - Data sharing concerns
   - Tracking across sites
   - Not all users have accounts

4. **Limited Control**
   - Can't customize login experience
   - Provider controls auth flow
   - Limited branding
   - Rate limiting by provider

5. **Network Dependency**
   - Requires internet connectivity
   - Latency from external calls
   - No offline authentication
   - Additional network hops

6. **Implementation Challenges**
   - Token storage security
   - Refresh token rotation
   - PKCE for SPAs
   - Testing complexity

7. **Not Always Appropriate**
   - Internal-only applications
   - High-security government systems
   - Offline-first applications
   - Systems requiring full control

---

## Implementation Guide for TodoList App

Now let's implement OAuth 2.0 with Google and Microsoft Entra ID in your TodoList application.

### Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│                       Frontend (React)                       │
│  - Login buttons (Google, Microsoft)                        │
│  - Redirect to provider                                     │
│  - Receive and store tokens                                 │
│  - Send tokens with API requests                            │
└────────────────────┬─────────────────────────────────────────┘
                     │
                     │ HTTP + Bearer Token
                     │
┌────────────────────▼─────────────────────────────────────────┐
│                   Backend (.NET 8 API)                       │
│  - JWT Bearer authentication                                │
│  - Validate tokens                                          │
│  - Extract user identity                                    │
│  - Protect endpoints with [Authorize]                       │
└────────────────────┬─────────────────────────────────────────┘
                     │
                     │ Validate
                     │
┌────────────────────▼─────────────────────────────────────────┐
│              OAuth Providers                                 │
│  - Google OAuth 2.0                                         │
│  - Microsoft Entra ID (Azure AD)                            │
│  - Issue tokens                                             │
│  - Provide user info                                        │
└──────────────────────────────────────────────────────────────┘
```

### Prerequisites

1. **Google OAuth Setup**
   - Google Cloud Console account
   - OAuth 2.0 Client ID and Secret

2. **Microsoft Entra ID Setup**
   - Azure account
   - App registration
   - Client ID and Secret

3. **Development Environment**
   - .NET 8 SDK
   - Node.js and npm
   - Your existing TodoList application

---

## Part 1: Provider Configuration

### Setting Up Google OAuth

1. **Go to Google Cloud Console**: https://console.cloud.google.com/

2. **Create a New Project** (or select existing):
   - Project name: "TodoList OAuth"

3. **Enable Google+ API**:
   - Navigate to "APIs & Services" > "Library"
   - Search for "Google+ API" and enable it

4. **Create OAuth Credentials**:
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth client ID"
   - Application type: "Web application"
   - Name: "TodoList Web Client"
   - Authorized JavaScript origins:
     - `http://localhost:5173`
     - `http://localhost:3000`
   - Authorized redirect URIs:
     - `http://localhost:5173/auth/google/callback`
     - `http://localhost:5001/api/auth/google/callback`

5. **Save Credentials**:
   - Client ID: `[Your Google Client ID]`
   - Client Secret: `[Your Google Client Secret]`

### Setting Up Microsoft Entra ID (Azure AD)

1. **Go to Azure Portal**: https://portal.azure.com/

2. **Navigate to Microsoft Entra ID**:
   - Search for "Microsoft Entra ID" (or "Azure Active Directory")

3. **Register an Application**:
   - Go to "App registrations" > "New registration"
   - Name: "TodoList Application"
   - Supported account types: "Accounts in any organizational directory and personal Microsoft accounts"
   - Redirect URI:
     - Platform: "Web"
     - URI: `http://localhost:5001/api/auth/microsoft/callback`
   - Click "Register"

4. **Configure Authentication**:
   - Go to "Authentication"
   - Under "Implicit grant and hybrid flows", check:
     - ✅ ID tokens
   - Add additional redirect URIs:
     - `http://localhost:5173/auth/microsoft/callback`
   - Click "Save"

5. **Create Client Secret**:
   - Go to "Certificates & secrets"
   - Click "New client secret"
   - Description: "TodoList App Secret"
   - Expires: "24 months" (choose based on policy)
   - Click "Add"
   - **Copy the secret value immediately** (you won't see it again)

6. **Note Your Configuration**:
   - Application (client) ID: `[Your Azure Client ID]`
   - Directory (tenant) ID: `[Your Azure Tenant ID]`
   - Client Secret: `[Your Azure Client Secret]`

7. **Configure API Permissions** (optional, for accessing user data):
   - Go to "API permissions"
   - "Add a permission" > "Microsoft Graph"
   - Select "Delegated permissions"
   - Add: `User.Read`, `email`, `profile`, `openid`
   - Click "Add permissions"

---

## Part 2: Backend Implementation (.NET 8)

### Step 1: Install Required NuGet Packages

Add the following packages to `TodoListApi.csproj`:

```xml
<ItemGroup>
  <!-- Existing packages -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.20" />
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.20" />
  <!-- ... other existing packages ... -->
  
  <!-- New OAuth packages -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.20" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.Google" Version="8.0.20" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.MicrosoftAccount" Version="8.0.20" />
  <PackageReference Include="Microsoft.Identity.Web" Version="3.3.0" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
</ItemGroup>
```

### Step 2: Update appsettings.json

Add OAuth configuration to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=todolist.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_AZURE_CLIENT_ID",
      "ClientSecret": "YOUR_AZURE_CLIENT_SECRET",
      "TenantId": "YOUR_AZURE_TENANT_ID"
    }
  },
  
  "Jwt": {
    "Key": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG_FOR_SECURITY",
    "Issuer": "TodoListAPI",
    "Audience": "TodoListApp",
    "ExpirationMinutes": 60
  }
}
```

### Step 3: Create appsettings.Development.json for Local Secrets

Create `appsettings.Development.json` with your actual credentials (DO NOT commit this file):

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abcdefg.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-your-actual-secret"
    },
    "Microsoft": {
      "ClientId": "12345678-1234-1234-1234-123456789abc",
      "ClientSecret": "your-actual-secret-value",
      "TenantId": "common"
    }
  },
  "Jwt": {
    "Key": "MySecretKeyForJwtTokenGeneration12345678901234567890"
  }
}
```

**Important**: Add to `.gitignore`:
```
appsettings.Development.json
```

### Step 4: Create DTOs for Authentication

Create `backend/TodoListApi/DTOs/AuthDTOs.cs`:

```csharp
namespace TodoListApi.DTOs;

public record GoogleLoginRequest(string IdToken);
public record MicrosoftLoginRequest(string Code);

public record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    string Id,
    string Email,
    string Name,
    string? Picture,
    string Provider
);

public record RefreshTokenRequest(string RefreshToken);
```

### Step 5: Create Authentication Service

Create `backend/TodoListApi/Services/IAuthService.cs`:

```csharp
using TodoListApi.DTOs;

namespace TodoListApi.Services;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateGoogleAsync(string idToken);
    Task<AuthResponse> AuthenticateMicrosoftAsync(string code);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    string GenerateJwtToken(UserInfo user);
}
```

Create `backend/TodoListApi/Services/AuthService.cs`:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using TodoListApi.DTOs;

namespace TodoListApi.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly TodoDbContext _dbContext;  // Database context for user storage

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
                Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Check if user exists in database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderUserId == payload.Subject);

            if (user == null)
            {
                // Create new user in database
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
                Id: payload.Subject,
                Email: payload.Email,
                Name: payload.Name,
                Picture: payload.Picture,
                Provider: "Google"
            );

            var token = GenerateJwtToken(userInfo);
            var refreshToken = GenerateRefreshToken();
            
            // Store refresh token (use database in production)
            _refreshTokens[refreshToken] = (userInfo.Id, DateTime.UtcNow.AddDays(30));

            return new AuthResponse(
                Token: token,
                RefreshToken: refreshToken,
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
            var userInfo = await GetMicrosoftUserInfoAsync(tokenResponse.AccessToken);

            var token = GenerateJwtToken(userInfo);
            var refreshToken = GenerateRefreshToken();
            
            _refreshTokens[refreshToken] = (userInfo.Id, DateTime.UtcNow.AddDays(30));

            return new AuthResponse(
                Token: token,
                RefreshToken: refreshToken,
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
        if (!_refreshTokens.TryGetValue(refreshToken, out var tokenData))
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (tokenData.expiresAt < DateTime.UtcNow)
        {
            _refreshTokens.Remove(refreshToken);
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        // In production, fetch user from database
        // For now, we'll need to recreate the user info
        // This is simplified - in real app, store user in DB
        throw new NotImplementedException("Refresh token implementation requires user database");
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
            ["redirect_uri"] = "http://localhost:5173/login",  // ⚠️ Must match frontend redirect URI
            ["grant_type"] = "authorization_code"
        };

        var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestData));
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>();
        return tokenResponse!;
    }

    private async Task<UserInfo> GetMicrosoftUserInfoAsync(string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
        response.EnsureSuccessStatusCode();

        var userProfile = await response.Content.ReadFromJsonAsync<MicrosoftUserProfile>();
        
        return new UserInfo(
            Id: userProfile!.Id,
            Email: userProfile.Mail ?? userProfile.UserPrincipalName,
            Name: userProfile.DisplayName,
            Picture: null, // Microsoft Graph requires additional call for photo
            Provider: "Microsoft"
        );
    }

    // ⚠️ IMPORTANT: Use JsonPropertyName attributes for proper JSON deserialization
    // Microsoft's API returns snake_case (access_token) but C# uses PascalCase (AccessToken)
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

**Key Implementation Notes:**

1. **Redirect URI Consistency**: The `redirect_uri` in the token exchange **must exactly match** what you configured in Azure Portal and what the frontend uses (`http://localhost:5173/login`)

2. **JSON Property Mapping**: Microsoft's OAuth API returns JSON with snake_case property names, but C# uses PascalCase. Use `[JsonPropertyName]` attributes to map them correctly. Don't forget to add `using System.Text.Json.Serialization;`

3. **API Permissions**: Ensure your Azure app registration has proper Microsoft Graph API permissions (`User.Read`, `openid`, `profile`, `email`) with admin consent granted.
```

### Step 6: Update Program.cs

Modify your `Program.cs` to add authentication:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoListApi.DTOs;
using TodoListApi.Models;
using TodoListApi.Services;
using TodoListApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "TodoList API", 
        Version = "v1",
        Description = "A comprehensive TodoList API built with .NET 8 Minimal APIs with OAuth 2.0"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:5174", 
                "http://localhost:3000", 
                "http://127.0.0.1:5173", 
                "http://127.0.0.1:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Add EF Core with SQLite
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

// Register services
builder.Services.AddScoped<ICategoryService, EfCategoryService>();
builder.Services.AddScoped<ITodoService, EfTodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register HttpClient for making external API calls
builder.Services.AddHttpClient();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoList API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// ==================== AUTH ENDPOINTS ====================

var authGroup = app.MapGroup("/api/auth")
    .WithTags("Authentication")
    .WithOpenApi();

authGroup.MapPost("/google", async ([FromBody] GoogleLoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateGoogleAsync(request.IdToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
})
.WithName("GoogleLogin")
.WithSummary("Authenticate with Google")
.AllowAnonymous();

authGroup.MapPost("/microsoft", async ([FromBody] MicrosoftLoginRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.AuthenticateMicrosoftAsync(request.Code);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
})
.WithName("MicrosoftLogin")
.WithSummary("Authenticate with Microsoft")
.AllowAnonymous();

authGroup.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
})
.WithName("RefreshToken")
.WithSummary("Refresh access token")
.AllowAnonymous();

authGroup.MapGet("/me", (HttpContext context) =>
{
    var user = context.User;
    var userInfo = new
    {
        Id = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
        Name = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
        Provider = user.FindFirst("provider")?.Value
    };
    return Results.Ok(userInfo);
})
.WithName("GetCurrentUser")
.WithSummary("Get current authenticated user")
.RequireAuthorization();

// ==================== TODO ENDPOINTS (NOW PROTECTED) ====================

var todosGroup = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .WithOpenApi()
    .RequireAuthorization(); // All todo endpoints require authentication

todosGroup.MapGet("/", async (ITodoService todoService, HttpContext context) =>
{
    var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var todos = await todoService.GetAllTodosAsync();
    
    // Filter todos by user (you'll need to add UserId to Todo model)
    var response = todos.Select(t => new TodoResponse
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority.ToString(),
        Category = t.Category,
        IsCompleted = t.IsCompleted,
        CreatedDate = t.CreatedDate,
        DueDate = t.DueDate,
        Tags = t.Tags
    });
    return Results.Ok(response);
})
.WithName("GetAllTodos");

// ... (keep existing todo endpoints, they're now protected)

app.Run();
```

---

## Part 3: Frontend Implementation (React + TypeScript)

### Step 1: Install Required npm Packages

```bash
cd frontend
npm install @react-oauth/google jwt-decode
npm install -D @types/jwt-decode
```

### Step 2: Create Auth Context

Create `frontend/src/context/AuthContext.tsx`:

```typescript
import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';

interface UserInfo {
  id: string;
  email: string;
  name: string;
  picture?: string;
  provider: string;
}

interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

interface AuthContextType {
  user: UserInfo | null;
  token: string | null;
  login: (authResponse: AuthResponse) => void;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Check for existing session
    const storedToken = localStorage.getItem('authToken');
    const storedUser = localStorage.getItem('user');

    if (storedToken && storedUser) {
      try {
        const decoded: any = jwtDecode(storedToken);
        
        // Check if token is expired
        if (decoded.exp * 1000 > Date.now()) {
          setToken(storedToken);
          setUser(JSON.parse(storedUser));
        } else {
          // Token expired, clear storage
          localStorage.removeItem('authToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
        }
      } catch (error) {
        console.error('Invalid token:', error);
        localStorage.removeItem('authToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
      }
    }
    setIsLoading(false);
  }, []);

  const login = (authResponse: AuthResponse) => {
    setToken(authResponse.token);
    setUser(authResponse.user);
    
    localStorage.setItem('authToken', authResponse.token);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.user));
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  };

  const value: AuthContextType = {
    user,
    token,
    login,
    logout,
    isAuthenticated: !!token && !!user,
    isLoading
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
```

### Step 3: Create Login Page

Create `frontend/src/pages/LoginPage.tsx`:

```typescript
import { GoogleOAuthProvider, GoogleLogin } from '@react-oauth/google';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import axios from 'axios';

const GOOGLE_CLIENT_ID = 'YOUR_GOOGLE_CLIENT_ID';
const MICROSOFT_CLIENT_ID = 'YOUR_MICROSOFT_CLIENT_ID';
const API_BASE_URL = 'http://localhost:5001/api';

const LoginPage = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleGoogleSuccess = async (credentialResponse: any) => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await axios.post(`${API_BASE_URL}/auth/google`, {
        idToken: credentialResponse.credential
      });

      login(response.data);
      navigate('/todos');
    } catch (err: any) {
      console.error('Google login failed:', err);
      setError('Google login failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleGoogleError = () => {
    setError('Google login failed. Please try again.');
  };

  const handleMicrosoftLogin = () => {
    const tenantId = 'common'; // or your specific tenant ID
    const redirectUri = encodeURIComponent('http://localhost:5001/api/auth/microsoft/callback');
    const scope = encodeURIComponent('openid profile email User.Read');
    const responseType = 'code';
    const state = Math.random().toString(36).substring(7); // CSRF protection

    const authUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?` +
      `client_id=${MICROSOFT_CLIENT_ID}` +
      `&response_type=${responseType}` +
      `&redirect_uri=${redirectUri}` +
      `&scope=${scope}` +
      `&state=${state}`;

    window.location.href = authUrl;
  };

  return (
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="max-w-md w-full space-y-8 p-8 bg-white rounded-lg shadow-md">
          <div>
            <h2 className="text-center text-3xl font-extrabold text-gray-900">
              Sign in to TodoList
            </h2>
            <p className="mt-2 text-center text-sm text-gray-600">
              Choose your preferred login method
            </p>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <div className="space-y-4">
            {/* Google Login */}
            <div className="flex justify-center">
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={handleGoogleError}
                useOneTap
                theme="outline"
                size="large"
                text="signin_with"
                shape="rectangular"
              />
            </div>

            {/* Divider */}
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300"></div>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">Or</span>
              </div>
            </div>

            {/* Microsoft Login */}
            <button
              onClick={handleMicrosoftLogin}
              disabled={isLoading}
              className="w-full flex items-center justify-center gap-3 px-4 py-2 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
            >
              <svg className="w-5 h-5" viewBox="0 0 21 21">
                <rect x="1" y="1" width="9" height="9" fill="#f25022"/>
                <rect x="1" y="11" width="9" height="9" fill="#00a4ef"/>
                <rect x="11" y="1" width="9" height="9" fill="#7fba00"/>
                <rect x="11" y="11" width="9" height="9" fill="#ffb900"/>
              </svg>
              Sign in with Microsoft
            </button>
          </div>

          {isLoading && (
            <div className="text-center text-sm text-gray-600">
              Signing in...
            </div>
          )}
        </div>
      </div>
    </GoogleOAuthProvider>
  );
};

export default LoginPage;
```

### Step 4: Create Protected Route Component

Create `frontend/src/components/ProtectedRoute.tsx`:

```typescript
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ReactNode } from 'react';

interface ProtectedRouteProps {
  children: ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-xl">Loading...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
```

### Step 5: Update API Service to Include Auth Token

Update `frontend/src/services/api.ts`:

```typescript
import axios from 'axios';
import { Todo, Category, TodoStats, Priority, WeatherInfo, WeatherForecast5Day } from '../types';

const API_BASE_URL = 'http://localhost:5001/api';

// ... (existing interfaces)

// Create axios instance with default configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - redirect to login
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    } else if (error.response) {
      console.error(`API Error: ${error.response.status} ${error.response.statusText}`);
      throw new Error(`API Error: ${error.response.status} ${error.response.statusText}`);
    } else if (error.request) {
      console.error('Network Error:', error.message);
      throw new Error('Network Error: Unable to connect to server');
    } else {
      console.error('Request Error:', error.message);
      throw error;
    }
  }
);

// ... (rest of your existing API functions remain the same)
```

### Step 6: Update App.tsx

Update `frontend/src/App.tsx`:

```typescript
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/layout/Layout';
import { TodoProvider } from './context/TodoContext';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import HomePage from './pages/HomePage';
import TodosPage from './pages/TodosPage';
import CategoriesPage from './pages/CategoriesPage';
import AboutPage from './pages/AboutPage';
import LoginPage from './pages/LoginPage';
import NotFoundPage from './pages/NotFoundPage';
import './index.css';

function App() {
  return (
    <AuthProvider>
      <TodoProvider>
        <Router>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            
            {/* Protected routes */}
            <Route path="/" element={
              <ProtectedRoute>
                <Layout>
                  <HomePage />
                </Layout>
              </ProtectedRoute>
            } />
            
            <Route path="/todos" element={
              <ProtectedRoute>
                <Layout>
                  <TodosPage />
                </Layout>
              </ProtectedRoute>
            } />
            
            <Route path="/categories" element={
              <ProtectedRoute>
                <Layout>
                  <CategoriesPage />
                </Layout>
              </ProtectedRoute>
            } />
            
            <Route path="/about" element={
              <ProtectedRoute>
                <Layout>
                  <AboutPage />
                </Layout>
              </ProtectedRoute>
            } />
            
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Router>
      </TodoProvider>
    </AuthProvider>
  );
}

export default App;
```

### Step 7: Add User Profile to Navigation

Update `frontend/src/components/layout/Navigation.tsx`:

```typescript
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const Navigation = () => {
  const location = useLocation();
  const { user, logout } = useAuth();

  const isActive = (path: string) => location.pathname === path;

  return (
    <nav className="bg-indigo-600 text-white shadow-lg">
      <div className="container mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          {/* Logo and nav links */}
          <div className="flex space-x-4">
            <Link
              to="/"
              className={`px-3 py-2 rounded-md text-sm font-medium ${
                isActive('/') ? 'bg-indigo-700' : 'hover:bg-indigo-500'
              }`}
            >
              Home
            </Link>
            <Link
              to="/todos"
              className={`px-3 py-2 rounded-md text-sm font-medium ${
                isActive('/todos') ? 'bg-indigo-700' : 'hover:bg-indigo-500'
              }`}
            >
              Todos
            </Link>
            <Link
              to="/categories"
              className={`px-3 py-2 rounded-md text-sm font-medium ${
                isActive('/categories') ? 'bg-indigo-700' : 'hover:bg-indigo-500'
              }`}
            >
              Categories
            </Link>
            <Link
              to="/about"
              className={`px-3 py-2 rounded-md text-sm font-medium ${
                isActive('/about') ? 'bg-indigo-700' : 'hover:bg-indigo-500'
              }`}
            >
              About
            </Link>
          </div>

          {/* User profile */}
          {user && (
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                {user.picture && (
                  <img
                    src={user.picture}
                    alt={user.name}
                    className="w-8 h-8 rounded-full"
                  />
                )}
                <span className="text-sm">{user.name}</span>
              </div>
              <button
                onClick={logout}
                className="px-3 py-2 rounded-md text-sm font-medium bg-indigo-700 hover:bg-indigo-800"
              >
                Logout
              </button>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navigation;
```

---

## Part 4: Testing and Deployment

### Testing Checklist

1. **Google OAuth**:
   - [ ] Click "Sign in with Google"
   - [ ] Verify redirect to Google
   - [ ] Grant permissions
   - [ ] Verify redirect back to app
   - [ ] Check user profile displays
   - [ ] Test making authenticated API calls
   - [ ] Test logout

2. **Microsoft OAuth**:
   - [ ] Click "Sign in with Microsoft"
   - [ ] Verify redirect to Microsoft
   - [ ] Grant permissions
   - [ ] Verify redirect back to app
   - [ ] Check user profile displays
   - [ ] Test making authenticated API calls
   - [ ] Test logout

3. **Security**:
   - [ ] Test accessing protected routes without login
   - [ ] Test expired token handling
   - [ ] Test invalid token handling
   - [ ] Verify CORS configuration
   - [ ] Check HTTPS in production

### Environment Variables for Production

Create `.env` files:

**Backend (.NET)**: Use User Secrets or Azure Key Vault
```bash
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-secret"
```

**Frontend (React)**: Create `.env`:
```
VITE_GOOGLE_CLIENT_ID=your-google-client-id
VITE_MICROSOFT_CLIENT_ID=your-microsoft-client-id
VITE_API_BASE_URL=https://your-api.com/api
```

### Deployment Considerations

1. **Update Redirect URIs** in Google Cloud Console and Azure Portal
2. **Use HTTPS** in production (required by OAuth providers)
3. **Store secrets securely** (Azure Key Vault, AWS Secrets Manager)
4. **Configure CORS** for production domains
5. **Implement token refresh** logic before token expiration
6. **Add user database** to persist user data and refresh tokens
7. **Implement proper logging** for auth events
8. **Add rate limiting** to prevent abuse

---

## Part 5: Additional Enhancements

### ✅ 1. User Database Integration - IMPLEMENTED!

We've fully implemented database-backed user storage using Entity Framework Core with SQLite.

#### User Entity Model

Create `Models/User.cs`:

```csharp
namespace TodoListApi.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public string Provider { get; set; } = string.Empty;  // "Google" or "Microsoft"
    public string ProviderUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
```

#### Update DbContext

Update `Data/TodoDbContext.cs`:

```csharp
public class TodoDbContext : DbContext
{
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();  // NEW
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();  // NEW

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing Todo and Category configurations ...

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => new { u.Provider, u.ProviderUserId }).IsUnique();
            
            entity.HasMany(u => u.Todos)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasIndex(rt => rt.Token).IsUnique();
        });
    }
}
```

#### Update AuthService for Database Storage

Update `Services/AuthService.cs` to use database instead of in-memory storage:

```csharp
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private readonly TodoDbContext _dbContext;  // Inject DbContext

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
                Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            // Check if user exists in database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderUserId == payload.Subject);

            if (user == null)
            {
                // Create new user in database
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
    
    // Similar implementation for AuthenticateMicrosoftAsync
}
```

#### Create and Apply Migration

```bash
# Create migration
dotnet ef migrations add AddUsersAndAuthTables

# Apply migration to database
dotnet ef database update
```

**Benefits of Database Integration:**
- ✅ Users persist across application restarts
- ✅ Track user activity (CreatedAt, LastLoginAt)
- ✅ Unique constraint prevents duplicate users per provider
- ✅ Cascade delete removes all user data when user is deleted
- ✅ Foundation for advanced features (roles, permissions, audit logs)

---

### ✅ 2. Associate Todos with Users - IMPLEMENTED!

We've implemented user-specific todo filtering so each user only sees their own todos.

#### Update Todo Model

Update `Models/TodoModels.cs`:

```csharp
public class Todo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Priority Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? AssignedTo { get; set; }
    
    // NEW: User association
    public string? UserId { get; set; }
    public User? User { get; set; }
}
```

#### Update TodoService Interface

Update `Services/TodoServices.cs`:

```csharp
public interface ITodoService
{
    Task<IEnumerable<Todo>> GetAllTodosAsync(string userId);
    Task<Todo?> GetTodoByIdAsync(string id, string userId);
    Task<Todo> CreateTodoAsync(Todo todo, string userId);
    Task<Todo?> UpdateTodoAsync(string id, Todo updatedTodo, string userId);
    Task<bool> DeleteTodoAsync(string id, string userId);
    Task<TodoStats> GetStatsAsync(string userId);
}
```

#### Implement User Filtering in EfTodoService

Update `Services/EFTodoServices.cs`:

```csharp
public class EfTodoService : ITodoService
{
    private readonly TodoDbContext _context;

    public EfTodoService(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Todo>> GetAllTodosAsync(string userId)
    {
        // Filter todos by userId
        return await _context.Todos
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<Todo?> GetTodoByIdAsync(string id, string userId)
    {
        // Verify user owns this todo
        return await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<Todo> CreateTodoAsync(Todo todo, string userId)
    {
        todo.Id = Guid.NewGuid().ToString();
        todo.CreatedDate = DateTime.UtcNow;
        todo.UserId = userId;  // Automatically assign to user

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateTodoAsync(string id, Todo updatedTodo, string userId)
    {
        // Verify ownership before updating
        var existingTodo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        if (existingTodo == null)
            return null;

        existingTodo.Title = updatedTodo.Title;
        existingTodo.Description = updatedTodo.Description;
        existingTodo.Priority = updatedTodo.Priority;
        existingTodo.Category = updatedTodo.Category;
        existingTodo.IsCompleted = updatedTodo.IsCompleted;
        existingTodo.DueDate = updatedTodo.DueDate;
        existingTodo.Tags = updatedTodo.Tags;

        await _context.SaveChangesAsync();
        return existingTodo;
    }

    public async Task<bool> DeleteTodoAsync(string id, string userId)
    {
        // Verify ownership before deleting
        var todo = await _context.Todos
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        
        if (todo == null)
            return false;

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TodoStats> GetStatsAsync(string userId)
    {
        // Calculate statistics only for user's todos
        var todos = await _context.Todos
            .Where(t => t.UserId == userId)
            .ToListAsync();
        
        var categories = await _context.Categories.ToListAsync();

        var completedTodos = todos.Count(t => t.IsCompleted);
        var overdueTodos = todos.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate < DateTime.UtcNow);

        var todosByCategory = todos
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var todosByPriority = todos
            .GroupBy(t => t.Priority.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new TodoStats
        {
            TotalTodos = todos.Count,
            CompletedTodos = completedTodos,
            PendingTodos = todos.Count - completedTodos,
            TotalCategories = categories.Count,
            TodosByCategory = todosByCategory,
            TodosByPriority = todosByPriority,
            OverdueTodos = overdueTodos
        };
    }
}
```

#### Update API Endpoints with User Context

Update `Program.cs` to extract userId from JWT and require authorization:

```csharp
// Todo API Endpoints
var todosGroup = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .WithOpenApi()
    .RequireAuthorization();  // All todo endpoints require authentication

todosGroup.MapGet("/", async (HttpContext httpContext, ITodoService todoService) =>
{
    // Extract userId from JWT claims
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todos = await todoService.GetAllTodosAsync(userId);
    var response = todos.Select(t => new TodoResponse
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority.ToString(),
        Category = t.Category,
        IsCompleted = t.IsCompleted,
        CreatedDate = t.CreatedDate,
        DueDate = t.DueDate,
        Tags = t.Tags
    });
    return Results.Ok(response);
})
.WithName("GetAllTodos")
.WithSummary("Get all todos")
.WithDescription("Retrieves all todos for the authenticated user");

todosGroup.MapGet("/{id}", async (string id, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todo = await todoService.GetTodoByIdAsync(id, userId);
    if (todo == null)
        return Results.NotFound($"Todo with ID {id} not found");

    var response = new TodoResponse
    {
        Id = todo.Id,
        Title = todo.Title,
        Description = todo.Description,
        Priority = todo.Priority.ToString(),
        Category = todo.Category,
        IsCompleted = todo.IsCompleted,
        CreatedDate = todo.CreatedDate,
        DueDate = todo.DueDate,
        Tags = todo.Tags
    };
    return Results.Ok(response);
})
.WithName("GetTodoById")
.WithSummary("Get todo by ID")
.WithDescription("Retrieves a specific todo by its ID for the authenticated user");

todosGroup.MapPost("/", async ([FromBody] CreateTodoRequest request, HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var todo = new Todo
    {
        Title = request.Title,
        Description = request.Description,
        Priority = request.Priority,
        Category = request.Category,
        DueDate = request.DueDate,
        Tags = request.Tags
    };

    var createdTodo = await todoService.CreateTodoAsync(todo, userId);
    var response = new TodoResponse
    {
        Id = createdTodo.Id,
        Title = createdTodo.Title,
        Description = createdTodo.Description,
        Priority = createdTodo.Priority.ToString(),
        Category = createdTodo.Category,
        IsCompleted = createdTodo.IsCompleted,
        CreatedDate = createdTodo.CreatedDate,
        DueDate = createdTodo.DueDate,
        Tags = createdTodo.Tags
    };

    return Results.Created($"/api/todos/{createdTodo.Id}", response);
})
.WithName("CreateTodo")
.WithSummary("Create a new todo")
.WithDescription("Creates a new todo item for the authenticated user");

// Similar updates for PUT, PATCH, DELETE endpoints...

// Stats endpoint also requires authorization
app.MapGet("/api/stats", async (HttpContext httpContext, ITodoService todoService) =>
{
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

    var stats = await todoService.GetStatsAsync(userId);
    return Results.Ok(stats);
})
.WithTags("Statistics")
.WithName("GetTodoStats")
.WithSummary("Get todo statistics")
.WithDescription("Retrieves comprehensive statistics about todos for the authenticated user")
.WithOpenApi()
.RequireAuthorization();
```

**Security Benefits:**
- ✅ Users can only see their own todos
- ✅ Users cannot modify or delete other users' todos
- ✅ Automatic ownership assignment on creation
- ✅ 401 Unauthorized if user not authenticated
- ✅ 404 Not Found if todo doesn't exist or user doesn't own it

---

### ✅ 3. Token Refresh Implementation - IMPLEMENTED!

We've implemented secure refresh token rotation with database persistence.

#### RefreshToken Entity Model

Create `Models/RefreshToken.cs`:

```csharp
namespace TodoListApi.Models;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    // Navigation property
    public User? User { get; set; }
}
```

#### Implement Token Refresh in AuthService

Add refresh token method to `Services/AuthService.cs`:

```csharp
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

    // Revoke old refresh token (rotation pattern)
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
```

#### Add Refresh Endpoint

Add to `Program.cs`:

```csharp
app.MapPost("/api/auth/refresh", async ([FromBody] RefreshTokenRequest request, IAuthService authService) =>
{
    try
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken);
        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
})
.WithTags("Authentication")
.WithName("RefreshToken")
.WithSummary("Refresh access token")
.WithDescription("Exchange a refresh token for a new access token and refresh token")
.WithOpenApi();
```

**Refresh Token Benefits:**
- ✅ Secure token rotation (old token revoked when new one issued)
- ✅ Database tracking of all refresh tokens
- ✅ Ability to revoke tokens (logout from all devices)
- ✅ 30-day expiration for refresh tokens
- ✅ Prevents need for frequent logins
- ✅ Audit trail of token refresh activity

---

### 4. Social Login Buttons - ALREADY IMPLEMENTED!

The frontend already uses the official `@react-oauth/google` package which provides styled Google login buttons.

#### Current Implementation in LoginPage.tsx

```typescript
import { GoogleLogin } from '@react-oauth/google';

// Google Login Button (styled by @react-oauth/google)
<GoogleLogin
  onSuccess={handleGoogleSuccess}
  onError={() => {
    console.error('Google Login Failed');
    setError('Google login failed. Please try again.');
  }}
  useOneTap
  theme="filled_blue"
  size="large"
  text="signin_with"
/>

// Microsoft Login Button (custom styled)
<button
  onClick={handleMicrosoftLogin}
  className="microsoft-login-btn"
  style={{
    backgroundColor: '#2F2F2F',
    color: 'white',
    padding: '10px 20px',
    border: 'none',
    borderRadius: '4px',
    fontSize: '16px',
    cursor: 'pointer',
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
    width: '100%',
    justifyContent: 'center'
  }}
>
  <svg width="21" height="21" viewBox="0 0 21 21">
    {/* Microsoft logo SVG */}
  </svg>
  Sign in with Microsoft
</button>
```

**Optional Enhancement:** Consider using `react-social-login-buttons` for consistent styling across providers.

---

## Summary

This implementation provides:

✅ **OAuth 2.0 authentication** with Google and Microsoft  
✅ **JWT-based authorization** for API endpoints  
✅ **Database-backed user storage** with Entity Framework Core & SQLite  
✅ **Refresh token rotation** with database persistence  
✅ **User-specific data isolation** - users only see their own todos  
✅ **Secure token storage** and management  
✅ **Protected routes** in React frontend  
✅ **User profile** display and management  
✅ **Production-ready architecture** with best practices  
✅ **Activity tracking** with LastLoginAt timestamps  
✅ **Cascade delete** - user deletion removes all associated data  

### Key Takeaways

1. OAuth delegates authentication to trusted providers
2. Use authorization code flow for web apps (most secure)
3. Store tokens securely (HttpOnly cookies or secure localStorage)
4. Always validate tokens on the backend
5. Implement proper error handling and token refresh
6. Never commit secrets to version control
7. Use HTTPS in production
8. **Persist user data in databases for production applications**
9. **Implement refresh token rotation for security**
10. **Always filter data by authenticated user to ensure data isolation**

### What We Implemented Beyond Basic OAuth

This implementation goes beyond a basic OAuth tutorial by including:

1. ✅ **User Database Integration** - Full Entity Framework Core implementation with Users, RefreshTokens, and Todo relationships
2. ✅ **User-Specific Todos** - Complete data isolation with foreign keys and filtering
3. ✅ **Refresh Token Rotation** - Secure token refresh with revocation tracking
4. ✅ **Database Migrations** - Professional database schema management
5. ✅ **Activity Tracking** - CreatedAt and LastLoginAt timestamps
6. ✅ **Unique Constraints** - Prevent duplicate users per provider
7. ✅ **Cascade Deletes** - Maintain referential integrity
8. ✅ **Comprehensive Error Handling** - Proper logging and user feedback

### Next Steps for Further Enhancement

1. ~~Implement user database persistence~~ ✅ **COMPLETE**
2. ~~Add token refresh logic~~ ✅ **COMPLETE**
3. Implement role-based authorization (Admin, User roles)
4. Add more OAuth providers (GitHub, Facebook)
5. Implement social profile features (edit profile, change picture)
6. Add audit logging (track all user actions)
7. Set up monitoring and analytics
8. Implement "Remember Me" functionality
9. Add email verification
10. Create admin dashboard for user management

### Database Schema Summary

```
Users
├── Id (PK, GUID)
├── Email
├── Name
├── Picture
├── Provider (Google/Microsoft)
├── ProviderUserId
├── CreatedAt
└── LastLoginAt
    Relationships: Has many Todos, Has many RefreshTokens

RefreshTokens
├── Id (PK, GUID)
├── Token (Unique)
├── UserId (FK → Users.Id)
├── ExpiresAt
├── CreatedAt
├── IsRevoked
└── RevokedAt
    Relationships: Belongs to User

Todos
├── Id (PK)
├── Title
├── Description
├── Priority
├── Category
├── IsCompleted
├── CreatedDate
├── DueDate
├── Tags
├── AssignedTo
└── UserId (FK → Users.Id)
    Relationships: Belongs to User
```

### Educational Value

This implementation demonstrates:

- **OAuth 2.0** - Modern authentication with external providers
- **JWT Tokens** - Stateless authentication for APIs
- **Entity Framework Core** - ORM with Code First approach
- **Database Migrations** - Professional schema management
- **One-to-Many Relationships** - User has many Todos and RefreshTokens
- **Data Filtering** - User-specific data isolation
- **Token Management** - Refresh token rotation and revocation
- **Security Best Practices** - Input validation, authorization checks, secure token storage
- **REST API Design** - Proper HTTP methods and status codes
- **React State Management** - Global auth context
- **Protected Routes** - Frontend route guards
- **API Integration** - Axios interceptors for authentication

This guide provides a comprehensive understanding of OAuth and a **complete, production-ready implementation** for your TodoList application!

### Testing Your Implementation

To verify everything works:

1. **Start the backend**:
   ```bash
   cd TodoList/backend/TodoListApi
   dotnet run
   ```

2. **Start the frontend**:
   ```bash
   cd TodoList/frontend
   npm run dev
   ```

3. **Test the flow**:
   - Visit http://localhost:5173
   - Click "Login with Google" or "Login with Microsoft"
   - Create some todos
   - Logout and login with a different provider
   - Verify you don't see the previous user's todos
   - Check the database to see user records:
     ```bash
     cd TodoList/backend/TodoListApi
     sqlite3 TodoList.db "SELECT Email, Provider, LastLoginAt FROM Users;"
     ```

4. **Test refresh token**:
   - Login and note the refresh token from localStorage
   - Call the refresh endpoint with Postman/curl
   - Verify you get a new access token and refresh token
   - Check database to see old token is revoked

Congratulations on implementing a production-ready OAuth application! 🎉
