# OAuth 2.0 Implementation Setup Guide

This guide will help you set up and run the TodoList application with OAuth 2.0 authentication using Google and Microsoft Entra ID.

## 🎯 What You'll Learn

- How OAuth 2.0 works in a real application
- Setting up OAuth providers (Google and Microsoft)
- Implementing JWT-based authentication in .NET 8
- Managing authentication state in React
- Protecting API endpoints and frontend routes

## 📋 Prerequisites

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js (v18+)** - [Download](https://nodejs.org/)
3. **Google Account** - For Google OAuth setup
4. **Microsoft Account** - For Azure/Entra ID setup (optional)
5. **Code Editor** - VS Code recommended

## 🚀 Quick Start

### Step 1: Install Dependencies

#### Backend (.NET)
```powershell
cd TodoList/backend/TodoListApi
dotnet restore
```

#### Frontend (React)
```powershell
cd TodoList/frontend
npm install
```

### Step 2: Configure OAuth Providers

You need to set up OAuth credentials with Google and/or Microsoft. Follow the detailed setup instructions below.

---

## 🔐 OAuth Provider Setup

### Option 1: Google OAuth Setup

1. **Go to Google Cloud Console**
   - Visit: https://console.cloud.google.com/

2. **Create a New Project**
   - Click "Select a project" → "New Project"
   - Name: `TodoList OAuth`
   - Click "Create"

3. **Enable Required APIs**
   - Navigate to "APIs & Services" → "Library"
   - Search for "Google+ API" and click "Enable"

4. **Create OAuth 2.0 Credentials**
   - Go to "APIs & Services" → "Credentials"
   - Click "Create Credentials" → "OAuth client ID"
   - If prompted, configure the OAuth consent screen:
     - User Type: External
     - App name: TodoList Application
     - User support email: Your email
     - Developer contact: Your email
     - Click "Save and Continue"
   - Application type: **Web application**
   - Name: `TodoList Web Client`
   
   **Authorized JavaScript origins:**
   ```
   http://localhost:5173
   http://localhost:3000
   ```
   
   **Authorized redirect URIs:**
   ```
   http://localhost:5173
   http://localhost:5173/auth/google/callback
   ```

5. **Save Your Credentials**
   - Copy the **Client ID** (format: `xxxxx.apps.googleusercontent.com`)
   - Copy the **Client Secret**
   - Keep these safe!

### Option 2: Microsoft Entra ID (Azure AD) Setup

1. **Go to Azure Portal**
   - Visit: https://portal.azure.com/

2. **Navigate to Microsoft Entra ID**
   - Search for "Microsoft Entra ID" in the top search bar
   - Click on it

3. **Register Your Application**
   - Click "App registrations" in the left menu
   - Click "New registration"
   - Name: `TodoList Application`
   - Supported account types: "Accounts in any organizational directory and personal Microsoft accounts"
   - Redirect URI:
     - Platform: **Web** (NOT "Single-page application")
     - URI: `http://localhost:5173/login`
   - Click "Register"

4. **Configure Authentication**
   - In your app registration, click "Authentication"
   - Under "Platform configurations" → "Web", verify redirect URIs:
     - `http://localhost:5173/login` (primary)
     - `http://localhost:5173` (optional fallback)
   - Under "Implicit grant and hybrid flows":
     - ✅ Check "ID tokens"
     - ✅ Check "Access tokens"
   - Click "Save"

5. **Configure API Permissions** ⚠️ **CRITICAL STEP**
   - Click "API permissions" in the left menu
   - You should see "Microsoft Graph" with "User.Read" permission
   - Click "Add a permission" → "Microsoft Graph" → "Delegated permissions"
   - Ensure these permissions are added:
     - ✅ `User.Read` (Read user profile)
     - ✅ `openid` (Sign users in)
     - ✅ `profile` (View users' basic profile)
     - ✅ `email` (View users' email address)
   - Click "Add permissions"
   - **Important:** Click "Grant admin consent for [Your Org]" button
   - Verify all permissions show "Granted" status in green

6. **Create Client Secret**
   - Click "Certificates & secrets" in the left menu
   - Click "New client secret"
   - Description: `TodoList App Secret`
   - Expires: Choose duration (e.g., 24 months)
   - Click "Add"
   - **⚠️ IMPORTANT**: Copy the secret **VALUE** immediately (you won't see it again!)

7. **Save Your Configuration**
   - Application (client) ID: Found on "Overview" page
   - Directory (tenant) ID: Found on "Overview" page (use "common" for multi-tenant)
   - Client Secret: The value you just copied

---

## ⚙️ Application Configuration

### Backend Configuration

Edit `TodoList/backend/TodoListApi/appsettings.Development.json`:

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
  "Urls": "http://localhost:5001",
  
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_AZURE_CLIENT_ID",
      "ClientSecret": "YOUR_AZURE_CLIENT_SECRET",
      "TenantId": "common"
    }
  },
  
  "Jwt": {
    "Key": "MySecretKeyForJwtTokenGeneration12345678901234567890",
    "Issuer": "TodoListAPI",
    "Audience": "TodoListApp",
    "ExpirationMinutes": 60
  }
}
```

**Replace:**
- `YOUR_GOOGLE_CLIENT_ID` with your actual Google Client ID
- `YOUR_GOOGLE_CLIENT_SECRET` with your actual Google Client Secret
- `YOUR_AZURE_CLIENT_ID` with your actual Microsoft Application (client) ID
- `YOUR_AZURE_CLIENT_SECRET` with your actual Microsoft Client Secret

**⚠️ Security Note:** 
- Never commit `appsettings.Development.json` to version control
- The `.gitignore` file should already exclude it
- For production, use environment variables or Azure Key Vault

### Frontend Configuration

Edit `TodoList/frontend/src/pages/LoginPage.tsx`:

Find these lines near the top:
```typescript
const GOOGLE_CLIENT_ID = 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com';
const MICROSOFT_CLIENT_ID = 'YOUR_MICROSOFT_CLIENT_ID';
```

Replace with your actual Client IDs:
```typescript
const GOOGLE_CLIENT_ID = '123456789-abc123.apps.googleusercontent.com';
const MICROSOFT_CLIENT_ID = '12345678-1234-1234-1234-123456789abc';
```

---

## 🏃 Running the Application

### Terminal 1: Start the Backend

```powershell
cd TodoList/backend/TodoListApi
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5001
```

### Terminal 2: Start the Frontend

```powershell
cd TodoList/frontend
npm run dev
```

You should see:
```
  VITE ready in XXX ms

  ➜  Local:   http://localhost:5173/
```

### Access the Application

1. Open your browser to: http://localhost:5173
2. You'll be redirected to the login page
3. Choose "Sign in with Google" or "Sign in with Microsoft"
4. Complete the OAuth flow
5. You'll be redirected back and logged in!

---

## 🧪 Testing the Implementation

### Test Google OAuth

1. Click "Sign in with Google"
2. Select your Google account
3. Grant permissions
4. Verify you're redirected to the app
5. Check that your name and profile picture appear in the navigation
6. Try accessing different pages
7. Test logout functionality

### Test Microsoft OAuth

1. Click "Sign in with Microsoft"
2. Enter your Microsoft credentials
3. Grant permissions
4. Verify you're redirected to the app
5. Check that your name appears in the navigation
6. Try accessing different pages
7. Test logout functionality

### Test Protected Routes

1. Log out of the application
2. Try accessing: http://localhost:5173/todos
3. You should be redirected to /login
4. Log in again
5. You should be able to access all routes

### Test API Authentication

1. Open browser DevTools (F12)
2. Go to Network tab
3. Create a new todo item
4. Find the request in Network tab
5. Check the Headers section
6. Verify there's an `Authorization: Bearer <token>` header

---

## 🔍 Understanding the Implementation

### Backend Architecture

```
┌─────────────────────────────────────────┐
│         OAuth Provider                  │
│    (Google / Microsoft)                 │
└─────────────┬───────────────────────────┘
              │
              │ 1. Validate Token
              │ 2. Return User Info
              │
┌─────────────▼───────────────────────────┐
│      AuthService.cs                     │
│  - AuthenticateGoogleAsync()            │
│  - AuthenticateMicrosoftAsync()         │
│  - GenerateJwtToken()                   │
└─────────────┬───────────────────────────┘
              │
              │ 3. Generate JWT
              │
┌─────────────▼───────────────────────────┐
│      Program.cs                         │
│  - /api/auth/google                     │
│  - /api/auth/microsoft                  │
│  - /api/auth/me                         │
│  - Protected /api/todos endpoints       │
└──────────────────────────────────────────┘
```

### Frontend Architecture

```
┌─────────────────────────────────────────┐
│         LoginPage.tsx                   │
│  - GoogleLogin component                │
│  - Microsoft login redirect             │
└─────────────┬───────────────────────────┘
              │
              │ 1. User clicks login
              │ 2. OAuth flow starts
              │
┌─────────────▼───────────────────────────┐
│      AuthContext.tsx                    │
│  - login(authResponse)                  │
│  - logout()                             │
│  - isAuthenticated                      │
│  - Store in localStorage                │
└─────────────┬───────────────────────────┘
              │
              │ 3. Token stored
              │
┌─────────────▼───────────────────────────┐
│      api.ts (Axios Interceptor)         │
│  - Add Bearer token to requests         │
│  - Handle 401 unauthorized              │
└─────────────┬───────────────────────────┘
              │
              │ 4. Authenticated requests
              │
┌─────────────▼───────────────────────────┐
│      Protected Components               │
│  - ProtectedRoute wrapper               │
│  - TodosPage, CategoriesPage, etc.      │
└──────────────────────────────────────────┘
```

### Key Files

**Backend:**
- `Services/AuthService.cs` - OAuth validation and JWT generation
- `DTOs/AuthDTOs.cs` - Request/response models
- `Program.cs` - Authentication middleware and endpoints

**Frontend:**
- `context/AuthContext.tsx` - Authentication state management
- `pages/LoginPage.tsx` - Login UI with OAuth buttons
- `components/ProtectedRoute.tsx` - Route protection
- `services/api.ts` - API client with auth interceptors

---

## 🐛 Troubleshooting

### "Invalid Google token"

**Cause:** Google Client ID mismatch or expired token

**Solution:**
1. Verify Client ID in `appsettings.Development.json` matches Google Cloud Console
2. Verify Client ID in `LoginPage.tsx` matches Google Cloud Console
3. Check that redirect URIs are correctly configured

### "Microsoft authentication failed"

**Cause:** Misconfigured Azure app registration

**Solution:**

1. Verify redirect URIs in Azure Portal match `http://localhost:5173/login` (note the `/login` path)
2. Ensure "ID tokens" AND "Access tokens" are checked in Authentication settings
3. Verify Client ID and Secret are correct
4. Check that TenantId is set to "common"
5. **Verify API Permissions are granted:**
   - Go to Azure Portal → Your App → API permissions
   - Ensure `User.Read`, `openid`, `profile`, and `email` are present
   - Click "Grant admin consent" if status shows "Not granted"

### "Microsoft Graph API 401 Unauthorized"

**Cause:** Missing or not consented API permissions

**Solution:**

1. Go to Azure Portal → Your App Registration → API permissions
2. Verify these Microsoft Graph Delegated permissions exist:
   - `User.Read`
   - `openid`
   - `profile`
   - `email`
3. Click "Grant admin consent for [Your Organization]"
4. Wait a few minutes for changes to propagate
5. Try logging in again with Microsoft

### "The code has expired" error from Microsoft

**Cause:** Authorization code being used multiple times (React Strict Mode issue)

**Solution:**

This is already handled in the code with sessionStorage checks. If you see this error:

1. Clear browser storage:
   ```javascript
   // In browser console (F12):
   sessionStorage.clear()
   localStorage.clear()
   ```
2. Refresh the page (Ctrl+F5)
3. Try logging in again

The code uses `sessionStorage` to prevent duplicate code exchanges during React's development mode double-rendering.

### "NullReferenceException: access_token is null"

**Cause:** JSON deserialization mismatch between C# properties and Microsoft's JSON response

**Solution:**

This is already fixed in the codebase. The `MicrosoftTokenResponse` record uses `[JsonPropertyName]` attributes to map snake_case JSON (`access_token`) to PascalCase C# properties (`AccessToken`).

If you're implementing your own OAuth provider, ensure proper JSON property mapping:

```csharp
private record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn
);
```

### "Cannot find module 'react'" errors

**Cause:** npm packages not installed

**Solution:**
```powershell
cd TodoList/frontend
npm install
```

### "401 Unauthorized" on API requests

**Cause:** Token not being sent or invalid

**Solution:**
1. Check browser DevTools → Application → Local Storage
2. Verify `authToken` exists
3. Try logging out and logging in again
4. Check Network tab for Authorization header

### Backend won't start

**Cause:** Port 5001 already in use or missing packages

**Solution:**
```powershell
# Kill process on port 5001
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Restore packages
dotnet restore
```

---

## 📚 Learning Resources

### OAuth 2.0 Concepts
- [OAuth 2.0 Simplified](https://www.oauth.com/)
- [Understanding OAuth 2.0](https://auth0.com/intro-to-iam/what-is-oauth-2)

### Google OAuth
- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Google Sign-In for Web](https://developers.google.com/identity/gsi/web)

### Microsoft Entra ID
- [Microsoft identity platform documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [OAuth 2.0 and OpenID Connect on Microsoft identity platform](https://learn.microsoft.com/en-us/azure/active-directory/develop/v2-protocols)

### JWT Tokens
- [JWT.io](https://jwt.io/)
- [Introduction to JSON Web Tokens](https://jwt.io/introduction)

---

## 🎓 Assignment Ideas

1. **Add GitHub OAuth** - Implement a third provider
2. ~~**User Database**~~ - ✅ **IMPLEMENTED!** Users stored in SQLite with Entity Framework Core
3. ~~**Associate Todos with Users**~~ - ✅ **IMPLEMENTED!** Todos filtered by logged-in user with UserId foreign key
4. ~~**Refresh Token Implementation**~~ - ✅ **IMPLEMENTED!** Automatic token refresh with rotation and database storage
5. **Role-Based Authorization** - Add admin vs user roles
6. **Social Features** - Share todos with other users
7. **Activity Log** - Track login history and user actions (partially implemented with LastLoginAt)

---

## 🔒 Security Best Practices

✅ **DO:**
- Store secrets in environment variables or Azure Key Vault
- Use HTTPS in production
- Validate tokens on every request
- Set appropriate token expiration times
- Implement CSRF protection
- Use secure, HttpOnly cookies for production

❌ **DON'T:**
- Commit secrets to version control
- Store tokens in plain localStorage without consideration
- Skip token validation
- Use weak JWT secrets
- Expose client secrets in frontend code
- Ignore CORS policies

---

## 📝 Next Steps

1. **Review the Implementation Guide** - Read `OAUTH_IMPLEMENTATION_GUIDE.md` for detailed explanations
2. **Experiment with the Code** - Try modifying authentication flows
3. **Add Features** - Implement user-specific todos
4. **Deploy to Production** - Learn about production OAuth setup
5. **Explore Extensions** - Add role-based access control, MFA, etc.

---

## 💡 Common Questions

**Q: Why use OAuth instead of username/password?**
A: OAuth delegates authentication to trusted providers, eliminating password storage, reducing security risks, and improving user experience.

**Q: What's the difference between OAuth and JWT?**
A: OAuth is an authorization framework for delegated access. JWT is a token format. We use OAuth to authenticate users, then issue JWTs for API access.

**Q: Is it safe to store tokens in localStorage?**
A: For educational purposes, yes. In production, consider HttpOnly cookies or more secure storage with proper XSS protection.

**Q: Can I use only Google or only Microsoft?**
A: Yes! You can implement just one provider. The code supports both but you can choose either.

**Q: How do I add more OAuth providers?**
A: Follow the same pattern: create credentials with the provider, add configuration, implement authentication method in AuthService.

---

## 🤝 Support

If you encounter issues:
1. Check the Troubleshooting section above
2. Review the detailed implementation guide
3. Check browser console and network tab for errors
4. Verify all configuration values are correct
5. Ask your instructor or TA for help

---

Happy Coding! 🚀
