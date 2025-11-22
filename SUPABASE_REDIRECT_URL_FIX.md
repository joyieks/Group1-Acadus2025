# Fix: Redirected to localhost:5000 Instead of Correct Port

## ?? **Problem**
When clicking the password reset link from Supabase emails (after admin creates a user), you're redirected to `http://localhost:5000` instead of your actual application URL (`https://localhost:63125`).

## ?? **Root Cause**
The **Site URL** and **Redirect URLs** in your Supabase project are configured to use `localhost:5000`, which is not where your application is running.

---

## ? **Solution: Update Supabase URL Configuration**

### **Step 1: Find Your Application URL**

Your application is configured to run on:
- **HTTPS**: `https://localhost:63125` (primary)
- **HTTP**: `http://localhost:63126` (fallback)

This is defined in `ASI.Basecode.WebApp/Properties/launchSettings.json`:

```json
{
  "profiles": {
    "ASI.Basecode.WebApp": {
      "commandName": "Project",
      "launchBrowser": true,
    "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:63125;http://localhost:63126"
    }
  }
}
```

---

### **Step 2: Update Supabase Dashboard Settings**

1. **Go to Supabase Dashboard**
   - Navigate to: https://app.supabase.com
   - Select your project

2. **Navigate to Authentication Settings**
   ```
   Dashboard ? Authentication ? URL Configuration
   ```

3. **Update Site URL**
   ```
   Site URL: https://localhost:63125
   ```
   ? This is the base URL users will be redirected to

4. **Update Redirect URLs**
   Add **BOTH** URLs to support HTTP and HTTPS:
   ```
   https://localhost:63125/**
   http://localhost:63126/**
   https://localhost:63125/Account/SetPassword
   http://localhost:63126/Account/SetPassword
   ```

5. **Click "Save"**

---

### **Step 3: Verify Email Template (Optional)**

Check if your email templates are using the correct variables:

1. Go to: **Authentication** ? **Email Templates**
2. For **"Confirm signup"** or **"Magic Link"** template, ensure it uses:
```html
{{ .ConfirmationURL }}
```
Not a hardcoded URL like `http://localhost:5000/...`

---

## ?? **Testing the Fix**

### **Test 1: Create a New Student**
1. Login as Admin
2. Go to **Users** ? **Add Student**
3. Fill in the form and submit
4. Check the email sent to the student
5. Click the "Set Password" link
6. **Expected**: Should redirect to `https://localhost:63125/Account/SetPassword`

### **Test 2: Password Reset**
1. Go to Login page
2. Click "Forgot Password"
3. Enter email and submit
4. Check email
5. Click "Reset Password" link
6. **Expected**: Should redirect to `https://localhost:63125/Account/SetPassword`

---

## ?? **Alternative: Programmatic Fix (If Needed)**

### **If Using Custom Email URLs**

If you're sending emails programmatically and hardcoding URLs, update them in your code:

**Before:**
```csharp
var resetUrl = $"http://localhost:5000/Account/SetPassword?token={token}";
```

**After:**
```csharp
var resetUrl = $"https://localhost:63125/Account/SetPassword?token={token}";
```

Or better yet, use configuration:

```csharp
// In appsettings.json
{
  "AppSettings": {
    "BaseUrl": "https://localhost:63125"
  }
}

// In code
var baseUrl = _configuration["AppSettings:BaseUrl"];
var resetUrl = $"{baseUrl}/Account/SetPassword?token={token}";
```

---

## ?? **Summary of Changes**

| Setting | Old Value | New Value |
|---------|-----------|-----------|
| **Site URL** | `http://localhost:5000` | `https://localhost:63125` |
| **Redirect URLs** | `http://localhost:5000/**` | `https://localhost:63125/**`<br>`http://localhost:63126/**` |

---

## ?? **Important Notes**

### **For Production Deployment**

When deploying to production, remember to update these URLs again:

```
Site URL: https://yourdomain.com
Redirect URLs:
  - https://yourdomain.com/**
  - https://yourdomain.com/Account/SetPassword
  - https://yourdomain.com/Auth/Login
```

### **Multiple Environments**

If you're working with teammates, everyone should use the same port OR update their individual Supabase project settings to match their local port.

---

## ? **Verification Checklist**

- [ ] Supabase Site URL updated to `https://localhost:63125`
- [ ] Redirect URLs include both HTTP and HTTPS variants
- [ ] Email templates use `{{ .ConfirmationURL }}` instead of hardcoded URLs
- [ ] Tested creating a new student
- [ ] Tested password reset flow
- [ ] Confirmed redirect goes to correct port

---

## ?? **Still Not Working?**

### **Check 1: Clear Browser Cache**
```
Ctrl + Shift + Delete ? Clear cached images and files
```

### **Check 2: Verify Application is Running**
```powershell
# Check if app is running on correct port
netstat -an | findstr "63125"
```

### **Check 3: Check Supabase Logs**
1. Go to Supabase Dashboard
2. Navigate to **Logs** ? **Auth**
3. Look for redirect URL in recent requests

### **Check 4: Inspect Email Link**
1. Open the email in your inbox
2. Right-click the "Set Password" link
3. Select "Copy link address"
4. Paste in notepad to verify URL

Expected format:
```
https://localhost:63125/Account/SetPassword#access_token=...&type=recovery
```

If it shows `localhost:5000`, the Supabase settings weren't saved correctly.

---

## ?? **Related Documentation**

- [Supabase URL Configuration Guide](./SUPABASE_DASHBOARD_URL_CONFIG_GUIDE.md)
- [Set Password Implementation](./PASSWORD_RESET_IMPLEMENTATION_GUIDE.md)
- [Email Configuration](./SUPABASE_EMAIL_NOT_RECEIVED_COMPLETE_FIX.md)

---

**Status**: ? Ready to Fix
**Priority**: ?? High (Blocks user registration)
**Estimated Time**: 2-3 minutes
