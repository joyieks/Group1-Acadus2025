# ?? Visual Guide: Configuring Supabase Redirect URLs

## Step-by-Step with Screenshots

### Step 1: Access Supabase Dashboard

1. **Open your browser** and go to: https://app.supabase.com
2. **Login** with your Supabase account
3. **Select your project** from the projects list

---

### Step 2: Navigate to URL Configuration

1. **Click on "Authentication"** in the left sidebar (shield icon)
2. **Click on "URL Configuration"** tab
3. You should see a page like this:

```
???????????????????????????????????????????????????????????????
? URL Configuration         ?
? Configure site URL and redirect URLs for authentication   ?
???????????????????????????????????????????????????????????????
?             ?
? Site URL   ?
? ??????????????????????????????????????????????????????????? ?
? ? https://localhost:63125/# ? ?
? ??????????????????????????????????????????????????????????? ?
?     ?
? Redirect URLs           ?
? URLs that auth providers are permitted to redirect to       ?
? post authentication. Wildcards are allowed.                 ?
?    ?
? ??????????????????????????????????????????????????????????? ?
? ? http://localhost:63125/Account/SetPassword      ? ?
? ? http://localhost:58209/Account/SetPassword               ? ?
? ?    ? ?
? ? [+ Add URL]       ? ?
? ??????????????????????????????????????????????????????????? ?
?    ?
? Total URLs: 2      ?
?              ?
? [Save Changes] ?
???????????????????????????????????????????????????????????????
```

---

### Step 3: Add Your Redirect URL

#### Option A: Add Specific Port

1. **Click "[+ Add URL]"** button
2. **Enter your URL exactly**:
   ```
   http://localhost:YOUR_PORT/Account/SetPassword
   ```
   Replace `YOUR_PORT` with your actual port number (e.g., `58209`)

3. **Press Enter** or click outside the input box
4. **Click "Save Changes"** button at the bottom

#### Option B: Add Wildcard (Recommended)

1. **Click "[+ Add URL]"** button
2. **Enter wildcard URL**:
   ```
   http://localhost:*/Account/SetPassword
   ```
3. **Press Enter** or click outside the input box
4. **Repeat for HTTPS** (if you use https):
   ```
   https://localhost:*/Account/SetPassword
   ```
5. **Click "Save Changes"** button at the bottom

---

### Step 4: Verify Configuration

After saving, your configuration should look like this:

#### If Using Specific Ports:
```
Redirect URLs:
  ? http://localhost:63125/Account/SetPassword
  ? http://localhost:58209/Account/SetPassword
  ? http://localhost:5000/Account/SetPassword
  
Total URLs: 3
```

#### If Using Wildcards (Better):
```
Redirect URLs:
  ? http://localhost:*/Account/SetPassword
  ? https://localhost:*/Account/SetPassword
  
Total URLs: 2
```

---

## ?? What Each URL Does

### Site URL
```
https://localhost:63125/#
```
- This is the **default redirect** when no specific redirect is provided
- Used as a template variable in email templates
- Can be your production URL in production environment

### Redirect URLs
```
http://localhost:*/Account/SetPassword
```
- These are **allowed destinations** for authentication redirects
- **Wildcard (`*`)** means any port is allowed
- More specific URLs take precedence over wildcards

---

## ? Validation

### Test Your Configuration

1. **Go back to your application**
2. **Login as Admin**
3. **Add a new student/teacher**
4. **Check console output**:
   ```
   === SendPasswordSetupEmailAsync ===
   Target email: newstudent@example.com
   ?? Dynamic Redirect URL: http://localhost:58209/Account/SetPassword
   ? Password setup email sent successfully
   ```

5. **Check the email** (in the student/teacher's inbox)
6. **Click the "Set Your Password" link**
7. **Verify you land on**: `http://localhost:YOUR_PORT/Account/SetPassword`
8. **No error messages should appear**

### Expected Result
? SetPassword page loads successfully
? URL in browser shows: `http://localhost:PORT/Account/SetPassword#access_token=...`
? Form fields are visible and working
? No "Invalid redirect URL" or "Redirect URL not allowed" errors

---

## ?? Common Mistakes

### ? Wrong Path
```
http://localhost:5000/SetPassword      ? WRONG (missing /Account)
http://localhost:5000/Account/SetPassword? CORRECT
```

### ? Trailing Slash
```
http://localhost:5000/Account/SetPassword/  ? WRONG (has trailing slash)
http://localhost:5000/Account/SetPassword   ? CORRECT
```

### ? Wrong Protocol
```
https://localhost:5000/Account/SetPassword  ? Check your actual protocol
http://localhost:5000/Account/SetPassword   ? Most development uses http
```

### ? Forgot to Save
```
After adding URLs, you MUST click "Save Changes" button!
```

---

## ?? Troubleshooting Dashboard Issues

### Can't Find URL Configuration?

**Check you're in the right place**:
1. Left sidebar ? Authentication (shield icon)
2. Top tabs ? URL Configuration
3. Should be between "Providers" and "Email Templates"

### Can't Add Wildcard URLs?

**Some Supabase versions don't support wildcards.**

**Workaround**: Add each port individually:
```
http://localhost:5000/Account/SetPassword
http://localhost:5001/Account/SetPassword
http://localhost:7000/Account/SetPassword
http://localhost:7001/Account/SetPassword
http://localhost:63125/Account/SetPassword
http://localhost:58209/Account/SetPassword
```

### Changes Not Taking Effect?

**Wait a moment** - Supabase sometimes takes 10-30 seconds to propagate changes.

**Then**:
1. Clear browser cache
2. Restart your application
3. Try sending a new password setup email

---

## ?? Production Configuration

### For Production/Staging Environments

**Add your production URL**:
```
https://acadus.edu/Account/SetPassword
https://www.acadus.edu/Account/SetPassword
https://staging.acadus.edu/Account/SetPassword
```

**Set Site URL to production**:
```
Site URL: https://acadus.edu
```

**Remove localhost URLs** (or keep them for testing):
```
Remove:
  http://localhost:*/Account/SetPassword
  https://localhost:*/Account/SetPassword
```

---

## ?? Need Help?

### If URLs Still Don't Work

1. **Check exact URL** from console logs
2. **Copy it exactly** (including http/https, port, path)
3. **Paste into Supabase Dashboard**
4. **Save changes**
5. **Wait 30 seconds**
6. **Try again**

### If Wildcards Don't Work

**Alternative**: Have all developers use the same port
- Edit `launchSettings.json`
- Set same port for everyone (e.g., `5000`)
- Only add that one port to Supabase Dashboard

---

**Last Updated**: 2025
**Related Docs**:
- `SETPASSWORD_LOCALHOST_PORT_FIX.md` - Technical details
- `QUICK_FIX_FOR_TEAMMATE.md` - Quick reference
