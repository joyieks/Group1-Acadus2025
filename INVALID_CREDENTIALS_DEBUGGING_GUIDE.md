# ?? DEBUGGING INVALID CREDENTIALS - Step-by-Step Guide

## ? **Good News: Authentication is Working Correctly!**

Your console shows:
```
? Auth client initialized with Anon Key  ? CORRECT!
Attempting to sign in with Supabase Auth...
? Invalid login credentials  ? The issue
```

This means:
- ? The fix worked (using AnonKey)
- ? Connection to Supabase is successful
- ? The credentials don't match what's in the database

---

## ?? **DIAGNOSIS: Why "Invalid Credentials"?**

### **Possible Causes:**

1. **User doesn't exist in Supabase Auth** (most common)
2. **Password is incorrect** (typo, wrong password)
3. **Email format mismatch** (case sensitivity, spaces)
4. **User was deleted from auth.users** but exists in your users table

---

## ??? **STEP-BY-STEP FIX**

### **Step 1: Check if User Exists in Supabase Auth**

1. Go to your Supabase Dashboard: https://supabase.com/dashboard
2. Select project: `fregpzxzivwhfcvauqmb`
3. Go to **Authentication** ? **Users**
4. Search for: `parillakenrumi@gmail.com`

#### **Scenario A: User NOT Found**

If you don't see the user, they need to be created:

**Option 1: Manual Creation (Quick Test)**
1. Click "Add User" button
2. Email: `parillakenrumi@gmail.com`
3. Password: `YourSecurePassword123!`
4. ? Check "Auto Confirm User"
5. Click "Create User"
6. Try logging in again

**Option 2: Use Registration System**
- Go to your registration page
- Register with the email and password
- Complete the registration process

#### **Scenario B: User Found**

If the user exists, check:
- **Email Confirmed?** - Must have a timestamp in "Email Confirmed" column
- If not confirmed:
  1. Click on the user
  2. Click "Send confirmation email"
  3. OR manually check the "Email Confirmed" checkbox
  4. Click "Save"

---

### **Step 2: Verify Password**

The password you're using might not match what's stored.

**To Reset Password:**

1. In Supabase Dashboard ? Authentication ? Users
2. Click on the user `parillakenrumi@gmail.com`
3. Click "Send recovery email" OR
4. Manually set new password:
   - Click "Reset Password"
   - Enter new password: `NewPassword123!`
   - Click "Update Password"
5. Try logging in with the new password

---

### **Step 3: Check Email Format**

Your code normalizes email to lowercase:
```csharp
var normalizedEmail = model.Email.Trim().ToLowerInvariant();
```

**Verify in Supabase:**
1. Go to Authentication ? Users
2. Check if email is stored as: `parillakenrumi@gmail.com` (all lowercase)
3. Or: `Parillakenrumi@gmail.com` (mixed case)

If it's mixed case, either:
- **Option A:** Change it to lowercase in Supabase
- **Option B:** Try logging in with exact case

---

### **Step 4: SQL Verification**

Run these queries in Supabase SQL Editor:

#### **Check if user exists in auth:**
```sql
SELECT 
    id,
    email,
    email_confirmed_at,
    created_at,
    last_sign_in_at
FROM auth.users
WHERE email ILIKE 'parillakenrumi@gmail.com';
```

#### **Check if user exists in your users table:**
```sql
SELECT 
    id,
    email,
    first_name,
  last_name,
    user_type_id,
    is_active
FROM users
WHERE email ILIKE 'parillakenrumi@gmail.com';
```

#### **Check user's role:**
```sql
SELECT 
    u.email,
    u.first_name,
    u.last_name,
    r.role_name
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
WHERE u.email ILIKE 'parillakenrumi@gmail.com';
```

---

## ?? **QUICK TEST: Create a Fresh Test User**

To eliminate all variables, create a brand new test user:

### **1. Create User in Supabase Dashboard:**

1. Authentication ? Users ? "Add User"
2. **Email:** `test@acadus.com`
3. **Password:** `Test1234!`
4. ? **Auto Confirm User:** Checked
5. Click "Create User"

### **2. Get the User ID:**

After creating, copy the **User ID** (UUID format: `abc123-def456-...`)

### **3. Create Records in Your Database:**

Run this SQL (replace `<USER_ID>` with the actual UUID):

```sql
-- Step 1: Add user to users table
INSERT INTO users (
    id,
    email,
    first_name,
 last_name,
    user_type_id,
    is_active,
  created_at
) VALUES (
    '<USER_ID>',  -- Replace with actual UUID from auth.users
    'test@acadus.com',
    'Test',
    'User',
    '1',  -- 1 = Student
    true,
    NOW()
);

-- Step 2: Assign Student role
INSERT INTO user_roles (user_id, role_id)
VALUES ('<USER_ID>', 1);  -- 1 = Student role

-- Step 3: Verify
SELECT 
    u.email,
    u.first_name,
    u.last_name,
    r.role_name
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
WHERE u.id = '<USER_ID>';
```

### **4. Test Login:**

- Email: `test@acadus.com`
- Password: `Test1234!`

**Expected Result:**
```
? Sign in SUCCESS!
  - User ID: <USER_ID>
  - Email: test@acadus.com
  - Email Confirmed: True
```

---

## ?? **DEBUGGING ENHANCEMENT**

Let me add more detailed logging to help diagnose:

### **Add this to SupabaseAuthService.cs:**

```csharp
public async Task<Supabase.Gotrue.Session> SignInAsync(string email, string password)
{
    try
    {
        Console.WriteLine($"=== SIGN IN ATTEMPT ===");
        Console.WriteLine($"Email: {email}");
 Console.WriteLine($"Password Length: {password?.Length ?? 0} characters");
     
        // ? ADDED: Try to check if user exists first
        var url = _configuration["Supabase:Url"];
        var anonKey = _configuration["Supabase:AnonKey"];

     Console.WriteLine($"Using Anon Key for authentication: {anonKey?.Substring(0, Math.Min(20, anonKey?.Length ?? 0))}...");

        // Create auth client
        var authOptions = new SupabaseOptions
        {
            AutoConnectRealtime = false,
      AutoRefreshToken = true
        };

        var authClient = new Supabase.Client(url, anonKey, authOptions);
        await authClient.InitializeAsync();

        Console.WriteLine("? Auth client initialized with Anon Key");
     Console.WriteLine($"Attempting to sign in with Supabase Auth...");
     
        // ? ADDED: More detailed error context
    try
   {
      var session = await authClient.Auth.SignIn(email, password);

            if (session != null)
            {
         Console.WriteLine($"? Sign in SUCCESS!");
       Console.WriteLine($"  - User ID: {session.User?.Id}");
   Console.WriteLine($"  - Email: {session.User?.Email}");
     Console.WriteLine($"  - Email Confirmed: {session.User?.EmailConfirmedAt.HasValue}");
         Console.WriteLine($"  - Email Confirmed At: {session.User?.EmailConfirmedAt}");
 }
            else
        {
     Console.WriteLine($"? Sign in returned NULL session");
            }

          return session;
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException gex)
        {
  Console.WriteLine($"? GOTRUE EXCEPTION during sign in:");
 Console.WriteLine($"  - Message: {gex.Message}");
          Console.WriteLine($"  - Status Code: {gex.StatusCode}");
      Console.WriteLine($"  - Content: {gex.Content}");
            
      // ? ADDED: More specific error analysis
       if (gex.Message.Contains("Invalid login credentials") || gex.Message.Contains("invalid_credentials"))
        {
  Console.WriteLine($"  ? Invalid email or password");
                Console.WriteLine($"  ? POSSIBLE CAUSES:");
     Console.WriteLine($"     1. User doesn't exist in auth.users table");
         Console.WriteLine($"     2. Password is incorrect");
    Console.WriteLine($"     3. Email case mismatch");
                Console.WriteLine($"  ? ACTION: Check Supabase Dashboard ? Authentication ? Users");
   }
            else if (gex.Message.Contains("Email not confirmed"))
            {
     Console.WriteLine($"  ? User email not verified");
          Console.WriteLine($"  ? ACTION: Confirm email in Supabase Dashboard");
    }
            
         throw;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? UNEXPECTED EXCEPTION during sign in:");
    Console.WriteLine($"  - Type: {ex.GetType().Name}");
        Console.WriteLine($"  - Message: {ex.Message}");
        Console.WriteLine($"  - Stack Trace: {ex.StackTrace}");
        throw;
    }
}
```

---

## ?? **COMMON SCENARIOS & SOLUTIONS**

| Symptom | Cause | Solution |
|---------|-------|----------|
| "Invalid credentials" | User doesn't exist | Create user in Supabase Dashboard |
| "Invalid credentials" | Wrong password | Reset password in Dashboard or use Forgot Password |
| "Invalid credentials" | Case mismatch | Ensure email is lowercase in both systems |
| "Email not confirmed" | Unverified email | Manually confirm in Dashboard or resend email |
| NULL session | Supabase connection issue | Check URL and AnonKey |

---

## ? **VERIFICATION CHECKLIST**

After creating/fixing the user:

- [ ] User exists in Supabase Dashboard ? Authentication ? Users
- [ ] Email is confirmed (has timestamp)
- [ ] User exists in your `users` table (SQL query)
- [ ] User has a role assigned in `user_roles` table
- [ ] Email format matches exactly (case-insensitive)
- [ ] Password is known and correct
- [ ] Try logging in again

---

## ?? **MOST LIKELY SOLUTION**

Based on your error, **the user probably doesn't exist in Supabase Auth (`auth.users` table)**.

**Quick Fix:**
1. Go to Supabase Dashboard ? Authentication ? Users
2. Click "Add User"
3. Email: `parillakenrumi@gmail.com`
4. Set password
5. Check "Auto Confirm User"
6. Click "Create User"
7. Run SQL to add to your `users` table (see Step 3 above)
8. Try logging in

---

## ?? **NEXT STEPS**

1. **Check Supabase Dashboard** - Is the user there?
2. **If NO:** Create the user (see Quick Test above)
3. **If YES:** Reset the password
4. **Verify email is confirmed**
5. **Try logging in again**
6. **Copy the console output here if still failing**

---

**The authentication system is working correctly - you just need to ensure the user exists with the correct credentials!** ?
