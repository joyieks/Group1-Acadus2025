# ?? JWT Token Usage in Your Application

## Executive Summary

**Yes, your application uses JWT tokens from Supabase**, but in a specific way:

1. **Supabase generates JWT tokens** automatically during authentication
2. **You store minimal session data** (User ID, Email) in ASP.NET Session
3. **You have custom JWT infrastructure** for your own token generation (currently unused with Supabase)

---

## ?? Where JWT Tokens Are Used

### 1. **Supabase Auth JWT Token Generation**

**File:** `SupabaseAuthService.cs` ? Lines 183-234

When a user logs in or signs up, Supabase automatically generates a JWT token:

```csharp
// Login/Signup creates a session with JWT
var session = await gotrueClient.SignUp(email, password, new SignUpOptions
{
    Data = new System.Collections.Generic.Dictionary<string, object>
    {
        { "first_name", firstName },
 { "last_name", lastName },
        { "full_name", $"{firstName} {lastName}" }
    }
});

// The session contains:
// - session.AccessToken  ? JWT token (used for API calls)
// - session.RefreshToken ? Refresh token (to get new JWT)
// - session.User.Id      ? User UUID
```

**What's in the JWT token:**
- User ID (UUID)
- Email
- User metadata (firstName, lastName, etc.)
- Token expiration time
- Issuer (Supabase)

---

### 2. **Login Process Using JWT**

**File:** `AuthController.cs` ? Lines 44-80

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginModel model)
{
    var gotrueClient = await _supabaseAuthService.GetSupabaseClientForAuthAsync();
    
    // Step 1: Authenticate with Supabase (generates JWT internally)
    var session = await gotrueClient.Auth.SignInWithPassword(
        model.Email, 
        model.Password
    );
    
    // session.AccessToken contains the JWT token
    // session.RefreshToken contains the refresh token
    
    if (session?.User != null)
    {
        // Step 2: Store minimal data in ASP.NET Session (NOT the JWT!)
        HttpContext.Session.SetString("UserEmail", session.User.Email);
        HttpContext.Session.SetString("SupabaseUserId", session.User.Id);
        
      // Step 3: Determine role from database
        var userRole = await _supabaseAuthService.GetUserRoleAsync(session.User.Id);
        
        // Step 4: Redirect based on role
        return RedirectToAction("Dashboard", userRole);
    }
}
```

**Important:** The JWT token is used internally by the Supabase client but **NOT stored in your ASP.NET session**. Instead, you store:
- User Email
- Supabase User ID (UUID)

---

### 3. **Where Supabase Uses the JWT Token**

**File:** `SupabaseAuthService.cs` ? Lines 84-145

The JWT token is automatically included in requests to Supabase:

```csharp
private async Task<Supabase.Client> GetSupabaseClientAsync()
{
    var options = new SupabaseOptions
  {
        AutoConnectRealtime = false,
        AutoRefreshToken = true,  // ? JWT auto-refreshed!
   Headers = new System.Collections.Generic.Dictionary<string, string>
   {
  { "X-Client-Info", "supabase-csharp/1.1.1" }
     }
    };
 
    _supabaseClient = new Supabase.Client(url, serviceRoleKey, options);
    
    // The client automatically includes JWT token in Authorization header:
 // Authorization: Bearer <JWT_TOKEN>
}
```

**Every API call to Supabase includes:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### 4. **Custom JWT Infrastructure (Not Currently Used for Supabase)**

Your application has its own JWT infrastructure that's **separate from Supabase**:

#### **Files Involved:**

| File | Purpose | Used? |
|------|---------|-------|
| `CustomJwtDataFormat.cs` | Validates and generates JWT tokens | ? Not used for Supabase auth |
| `TokenProvider.cs` | Creates JWT tokens | ? Not used for Supabase auth |
| `TokenProviderMiddleware.cs` | Middleware for token generation | ? Not used for Supabase auth |
| `SignInManager.cs` | Manages sign-in with custom claims | ? Not used for Supabase auth |

**These files are legacy code** that would be used if you were implementing your own JWT system instead of using Supabase Auth.

---

## ?? JWT Token Flow Diagram

```
???????????????????????????????????????????????????????????????
?       USER LOGIN FLOW WITH JWT       ?
???????????????????????????????????????????????????????????????

1. User enters email/password
   ?
2. AuthController calls SignInWithPassword()
   ?
3. Supabase Auth API
   ?? Verifies credentials in auth.users
   ?? Generates JWT token (AccessToken)
?? Generates RefreshToken
   ?? Returns session object
   ?
4. Session object contains:
   {
     "access_token": "eyJhbGciOiJIUzI1NiIs...",  ? JWT!
     "refresh_token": "abc123...",
     "user": {
     "id": "uuid-here",
  "email": "user@example.com"
     }
   }
   ?
5. Application stores (in ASP.NET Session):
   ?? UserEmail: "user@example.com"
   ?? SupabaseUserId: "uuid-here"
   (Note: JWT NOT stored in session!)
   ?
6. Application queries public.users to get role
   ?
7. Redirect to appropriate dashboard
   ?
8. All subsequent Supabase API calls use JWT:
   ?? Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
   ?? Supabase validates JWT
 ?? API request succeeds
```

---

## ?? Detailed JWT Usage Breakdown

### **During Registration (Admin Creates Student)**

**File:** `StudentService.cs` ? Line 90

```csharp
// Step 1: Create auth user (Supabase generates JWT internally)
var supabaseUserId = await _supabaseAuthService.CreateUserAsync(
    model.Email,
    password,
    model.FirstName,
    model.LastName
);

// Supabase internally:
// 1. Creates user in auth.users
// 2. Generates JWT for that user
// 3. Returns user UUID

// Step 2: Insert into public.users (uses Service Role Key, not JWT)
var client = await GetSupabaseClientAsync();
var userRecord = new SupabaseUserNew
{
    UserTypeId = supabaseUserId,  // Links to auth.users
    // ...
};
await client.From<SupabaseUserNew>().Insert(userRecord);
```

**Two types of authentication:**
1. **JWT Token** - For end users (students, teachers)
2. **Service Role Key** - For server-side operations (admin creating users)

---

### **During API Calls**

**File:** `SupabaseAuthService.cs` ? Line 620+

```csharp
public async Task<string> GetUserRoleAsync(string supabaseUserId)
{
    var client = await GetSupabaseClientAsync();
    
    // This query automatically includes JWT in headers:
    // Authorization: Bearer <JWT_TOKEN>
    var userQuery = await client
        .From<SupabaseUserNew>()
        .Where(x => x.UserTypeId == supabaseUserId)
 .Get();
}
```

**Behind the scenes:**
```http
GET /rest/v1/users?userTypeId=eq.abc123
Host: your-project.supabase.co
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
apikey: your-anon-key
```

---

## ?? JWT Token Contents

### **What's Inside a Supabase JWT?**

When decoded, the JWT looks like this:

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "aud": "authenticated",
    "exp": 1705334400,
    "sub": "abc123-def456-789-...",  // User UUID
    "email": "john.doe@example.com",
    "phone": "",
    "app_metadata": {
      "provider": "email",
      "providers": ["email"]
    },
    "user_metadata": {
   "first_name": "John",
      "last_name": "Doe",
      "full_name": "John Doe",
      "needs_password_setup": true
    },
    "role": "authenticated",
    "aal": "aal1",
    "amr": [
      {
"method": "password",
        "timestamp": 1705248000
  }
    ],
    "session_id": "xyz789-...",
    "is_anonymous": false
  },
  "signature": "..."
}
```

---

## ?? Key Configuration

### **File:** `appsettings.json`

```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "ServiceRoleKey": "eyJhbGciOiJIUzI1NiIsInR...",  // For server operations
    "AnonKey": "eyJhbGciOiJIUzI1NiIsInR5cCI...",    // For client operations
    "RedirectUrl": "https://localhost:7296/Account/SetPassword"
  },
  "TokenAuthentication": {
    "SecretKey": "your-custom-secret-key",  // For custom JWT (not Supabase)
    "Issuer": "ASI.Basecode",
    "Audience": "ASI.Basecode.WebApp"
  }
}
```

**Two different JWT systems:**
1. **Supabase JWT** - Uses Supabase's secret key (managed by Supabase)
2. **Custom JWT** - Uses your SecretKey (legacy, not currently used)

---

## ?? Summary for Your PM

### **Does the app use JWT?**
? **Yes**, but specifically **Supabase-generated JWT tokens**

### **Where are JWT tokens used?**
1. **Authentication** - When users log in
2. **Authorization** - In every API call to Supabase
3. **Auto-refresh** - Tokens refresh automatically

### **Where are JWT tokens stored?**
- **NOT in ASP.NET Session** - Only User ID and Email stored there
- **In Supabase client memory** - Managed by Supabase C# SDK
- **NOT in browser cookies** - Supabase handles this internally

### **Custom JWT infrastructure?**
? **Not used** - Your app has custom JWT code (`CustomJwtDataFormat`, `TokenProvider`, etc.) but these are **not used** for Supabase authentication. They're legacy code.

### **Security**
? **Secure** - JWT tokens are:
- Generated by Supabase (industry standard)
- Auto-refreshed before expiration
- Validated on every API call
- Never exposed to client-side JavaScript

---

## ?? Files That Use JWT

| File | JWT Usage | Type |
|------|-----------|------|
| `SupabaseAuthService.cs` | Uses Supabase JWT | ? Active |
| `AuthController.cs` | Receives JWT from Supabase | ? Active |
| `StudentService.cs` | API calls with JWT | ? Active |
| `TeacherService.cs` | API calls with JWT | ? Active |
| `CustomJwtDataFormat.cs` | Custom JWT validation | ? Unused |
| `TokenProvider.cs` | Custom JWT generation | ? Unused |
| `SignInManager.cs` | Custom auth manager | ? Unused |

---

**Conclusion:** Your application uses Supabase's JWT token system for authentication and authorization. The JWT is automatically handled by the Supabase C# SDK and included in all API requests.
