# OAuth 2.0 Quick Reference Card

## 🔑 What is OAuth 2.0?

**OAuth 2.0** is an authorization framework that lets users grant third-party applications access to their resources without sharing passwords.

### Key Concepts

| Term | Definition |
|------|------------|
| **Resource Owner** | The user who owns the data |
| **Client** | Your application (TodoList app) |
| **Resource Server** | API server with protected resources (your .NET backend) |
| **Authorization Server** | Issues access tokens (Google, Microsoft) |
| **Access Token** | Credential to access protected resources |
| **Refresh Token** | Used to obtain new access tokens |

---

## 🔄 Authorization Code Flow (Simplified)

```
1. User clicks "Login with Google"
   ↓
2. Redirect to Google login page
   ↓
3. User logs in and grants permission
   ↓
4. Google redirects back with authorization code
   ↓
5. Backend exchanges code for access token
   ↓
6. Backend generates JWT for your app
   ↓
7. Frontend stores JWT
   ↓
8. All API requests include JWT in Authorization header
```

---

## 📁 File Structure

### Backend Files Created/Modified

```
backend/TodoListApi/
├── DTOs/
│   └── AuthDTOs.cs                    # Authentication request/response models
├── Models/
│   ├── User.cs                        # ✨ NEW: User entity for database
│   ├── RefreshToken.cs                # ✨ NEW: Refresh token entity
│   └── TodoModels.cs                  # ✏️ MODIFIED: Added UserId foreign key
├── Data/
│   └── TodoDbContext.cs               # ✏️ MODIFIED: Added Users & RefreshTokens DbSets
├── Services/
│   ├── IAuthService.cs                # Auth service interface
│   ├── AuthService.cs                 # ✏️ MODIFIED: Database-backed auth
│   ├── TodoServices.cs                # ✏️ MODIFIED: Added userId parameters
│   └── EFTodoServices.cs              # ✏️ MODIFIED: Filter todos by user
├── Migrations/
│   └── AddUsersAndAuthTables.cs       # ✨ NEW: Database migration
├── Program.cs                         # ✏️ MODIFIED: Auth endpoints & user filtering
├── TodoListApi.csproj                 # ✏️ MODIFIED: Added OAuth NuGet packages
├── appsettings.json                   # ✏️ MODIFIED: Added OAuth config template
└── appsettings.Development.json       # ✏️ MODIFIED: Your actual credentials (DO NOT COMMIT!)
```

### Frontend Files Created/Modified

```
frontend/src/
├── context/
│   └── AuthContext.tsx                # Authentication state management
├── components/
│   └── ProtectedRoute.tsx             # Route protection wrapper
├── pages/
│   └── LoginPage.tsx                  # Login UI with OAuth buttons
├── components/layout/
│   └── Navigation.tsx                 # ✏️ MODIFIED: Added user profile & logout
├── services/
│   └── api.ts                         # ✏️ MODIFIED: Added auth interceptors
├── App.tsx                            # ✏️ MODIFIED: Wrapped with AuthProvider
├── index.css                          # ✏️ MODIFIED: Added user profile styles
└── package.json                       # ✏️ MODIFIED: Added OAuth packages
```

---

## 🔧 Configuration Checklist

### Google OAuth Setup

- [ ] Create project in Google Cloud Console
- [ ] Enable Google+ API
- [ ] Create OAuth Client ID (Web application)
- [ ] Add authorized JavaScript origins: `http://localhost:5173`
- [ ] Add redirect URIs: `http://localhost:5173`
- [ ] Copy Client ID
- [ ] Copy Client Secret
- [ ] Add to `appsettings.Development.json`
- [ ] Add Client ID to `LoginPage.tsx`

### Microsoft OAuth Setup (Optional)

- [ ] Create app registration in Azure Portal
- [ ] Configure authentication (check "ID tokens")
- [ ] Add redirect URI: `http://localhost:5173`
- [ ] Create client secret
- [ ] Copy Application (client) ID
- [ ] Copy Client Secret
- [ ] Copy Directory (tenant) ID
- [ ] Add to `appsettings.Development.json`
- [ ] Add Client ID to `LoginPage.tsx`

### Application Setup

- [ ] Run `dotnet restore` in backend
- [ ] Run `npm install` in frontend
- [ ] Configure OAuth credentials
- [ ] Start backend: `dotnet run`
- [ ] Start frontend: `npm run dev`
- [ ] Test login flow

---

## 🎯 API Endpoints

### Authentication Endpoints (No Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/google` | Login with Google ID token |
| POST | `/api/auth/microsoft` | Login with Microsoft auth code |
| POST | `/api/auth/refresh` | Refresh access token |

### Protected Endpoints (Auth Required)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/auth/me` | Get current user info |
| GET | `/api/todos` | Get user's todos (filtered) |
| POST | `/api/todos` | Create new todo for user |
| PUT | `/api/todos/{id}` | Update user's todo |
| DELETE | `/api/todos/{id}` | Delete user's todo |
| GET | `/api/stats` | Get user's todo statistics |
| GET | `/api/categories` | Get all categories |
| ... | ... | All other todo/category endpoints |

---

## 💻 Code Snippets

### Backend: AuthService Key Methods

```csharp
// Validate Google token and generate JWT with database storage
public async Task<AuthResponse> AuthenticateGoogleAsync(string idToken)
{
    // 1. Validate Google ID token
    var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    
    // 2. Check if user exists in database
    var user = await _dbContext.Users
        .FirstOrDefaultAsync(u => u.Provider == "Google" && u.ProviderUserId == payload.Subject);
    
    if (user == null)
    {
        // 3. Create new user in database
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
    }
    else
    {
        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
    
    // 4. Generate JWT for our app
    var userInfo = new UserInfo(user.Id, user.Email, user.Name, user.Picture, user.Provider);
    var token = GenerateJwtToken(userInfo);
    
    // 5. Store refresh token in database
    var refreshToken = new RefreshToken
    {
        Token = GenerateRefreshToken(),
        UserId = user.Id,
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        CreatedAt = DateTime.UtcNow
    };
    _dbContext.RefreshTokens.Add(refreshToken);
    await _dbContext.SaveChangesAsync();
    
    // 6. Return auth response
    return new AuthResponse(token, refreshToken.Token, expiresAt, userInfo);
}

// Generate JWT token
public string GenerateJwtToken(UserInfo user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Name, user.Name),
        new Claim("provider", user.Provider)
    };
    
    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials
    );
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Frontend: AuthContext Key Methods

```typescript
// Login function - stores auth response
const login = (authResponse: AuthResponse) => {
    setToken(authResponse.token);
    setUser(authResponse.user);
    
    localStorage.setItem('authToken', authResponse.token);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.user));
};

// Logout function - clears storage
const logout = () => {
    setToken(null);
    setUser(null);
    
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
};
```

### Frontend: API Interceptor

```typescript
// Add auth token to all requests
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('authToken');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    }
);

// Handle 401 unauthorized - redirect to login
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('authToken');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);
```

---

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| "Invalid Google token" | Check Client ID matches in both backend and frontend |
| "401 Unauthorized" | Check token is being sent in Authorization header |
| Module not found errors | Run `npm install` or `dotnet restore` |
| Backend won't start | Check port 5001 is available |
| Can't access /todos | Make sure you're logged in |
| Logout doesn't work | Check localStorage is being cleared |
| **Microsoft "code has expired"** | React Strict Mode runs effects twice. Add sessionStorage check in LoginPage to prevent duplicate code exchanges |
| **Microsoft redirects to login** | Redirect URI must be `http://localhost:5173/login` (not root) in Azure Portal, backend redirect_uri, and Azure platform must be "Web" not "SPA" |
| **Google One Tap 403 errors** | Disable One Tap in LoginPage if it interferes with manual button login |
| **Infinite refresh loop** | TodoProvider should only wrap protected routes, not entire app including LoginPage |

---

## 📊 Token Flow Diagram

```
┌──────────┐
│  User    │
└────┬─────┘
     │ 1. Click "Login with Google"
     ▼
┌──────────────┐
│ LoginPage    │
└────┬─────────┘
     │ 2. Redirect to Google
     ▼
┌──────────────┐
│   Google     │ User authenticates
└────┬─────────┘
     │ 3. Return ID Token
     ▼
┌──────────────┐
│ AuthService  │ Validate token
│  (Backend)   │ Generate JWT
└────┬─────────┘
     │ 4. Return JWT + User Info
     ▼
┌──────────────┐
│ AuthContext  │ Store in localStorage
│  (Frontend)  │
└────┬─────────┘
     │ 5. Include in all API requests
     ▼
┌──────────────┐
│ API Calls    │ Authorization: Bearer <JWT>
└──────────────┘
```

---

## 🎓 Learning Outcomes

After completing this implementation, you should understand:

- ✅ How OAuth 2.0 authorization code flow works
- ✅ Difference between OAuth tokens and JWT tokens
- ✅ How to validate OAuth tokens from providers
- ✅ How to generate and validate JWTs
- ✅ How to protect API endpoints with authentication
- ✅ How to manage authentication state in React
- ✅ How to protect frontend routes
- ✅ How to add authentication headers to API requests
- ✅ How to persist user data in databases with Entity Framework Core
- ✅ How to implement refresh token rotation
- ✅ How to filter data by authenticated user

---

## 🚀 Extension Ideas

1. ~~**Store refresh tokens in database**~~ - ✅ **IMPLEMENTED!** Persistent refresh token storage with rotation
2. **Add user profile page** - Display and edit user information
3. ~~**Implement token refresh**~~ - ✅ **IMPLEMENTED!** Auto-refresh with rotation before expiration
4. ~~**Add user-specific todos**~~ - ✅ **IMPLEMENTED!** Todos filtered by authenticated user
5. **Add GitHub OAuth** - Implement a third provider
6. **Implement logout all devices** - Invalidate all user tokens (partially done with revocation)
7. ~~**Add activity log**~~ - ✅ **PARTIALLY IMPLEMENTED!** Track user login history (LastLoginAt field)

---

## 📚 Key Takeaways

### Security Best Practices

✅ **DO:**
- Validate all tokens on the server
- Use HTTPS in production
- Store secrets securely
- Set appropriate token expiration
- Implement CSRF protection

❌ **DON'T:**
- Trust client-side token validation
- Commit secrets to git
- Store passwords
- Use weak JWT secrets
- Skip token validation

### OAuth vs Other Methods

| Method | Pros | Cons |
|--------|------|------|
| **OAuth 2.0** | No password storage, trusted providers, SSO | Dependency on providers, complexity |
| **Username/Password** | Full control, offline support | Password storage risk, no SSO |
| **API Keys** | Simple for services | Not user-specific, hard to revoke |

---

## 🔗 Useful Links

- [OAuth 2.0 Specification](https://oauth.net/2/)
- [JWT.io](https://jwt.io/)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Microsoft Identity Platform](https://learn.microsoft.com/en-us/azure/active-directory/develop/)

---

**Remember:** Authentication is about **who you are**, Authorization is about **what you can do**!

Good luck! 🎉
