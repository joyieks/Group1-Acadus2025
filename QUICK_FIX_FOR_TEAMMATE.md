# ?? Quick Fix for Your Teammate - SetPassword Not Working

## ? The Problem
Your SetPassword page doesn't work because your port (`localhost:58209`) is different from the configured port in Supabase.

## ? Immediate Solution (Choose One)

### Option 1: Add Your Port to Supabase Dashboard (2 minutes)

1. **Go to Supabase Dashboard**: https://app.supabase.com
2. **Select your project**
3. **Navigate to**: Authentication ? URL Configuration
4. **Scroll to "Redirect URLs"**
5. **Click "Add URL"** and enter:
   ```
   http://localhost:58209/Account/SetPassword
   ```
6. **Click "Save"**
7. **Done!** Try the password reset again

---

### Option 2: Use the Same Port as Team (1 minute)

Ask your teammate what port they're using (probably `63125`), then:

1. **Stop your application**
2. **Open**: `ASI.Basecode.WebApp/Properties/launchSettings.json`
3. **Find the port numbers** and change to match your teammate's:
   ```json
   {
     "profiles": {
       "ASI.Basecode.WebApp": {
    "applicationUrl": "https://localhost:7125;http://localhost:63125"
       }
     }
   }
   ```
4. **Save and restart** your application
5. **Done!** Now you're using the same port that's already configured

---

## ?? How to Test

1. **Run your application**
2. **Login as Admin** (use existing admin credentials)
3. **Add a test student**:
   - Go to "Manage Students" ? "Add Student"
   - Fill in basic info and submit
4. **Check the Console/Terminal Output** - you should see:
   ```
   === SendPasswordSetupEmailAsync ===
 Target email: test@example.com
   ?? Dynamic Redirect URL: http://localhost:YOUR_PORT/Account/SetPassword
   ? IMPORTANT: This redirect URL must be added to Supabase Dashboard
   ```
5. **Verify the port** matches your running application
6. **Check the test student's email** and click the password setup link
7. **SetPassword page should load** without errors

---

## ?? Still Not Working?

### Check Console Logs
Look for this in your application console/terminal:
```
=== SendPasswordSetupEmailAsync ===
Redirect URL: http://localhost:YOUR_PORT/Account/SetPassword
```

**The URL shown here MUST be added to Supabase Dashboard.**

### Common Issues

**Issue**: Email not received
- **Wait 60 seconds** between attempts (Supabase rate limit)
- **Check spam folder**
- See: `SUPABASE_EMAIL_NOT_RECEIVED_COMPLETE_FIX.md`

**Issue**: "Invalid redirect URL" error
- **The exact URL** from console logs must be in Supabase Dashboard
- **Check for typos** (http vs https, trailing slash, etc.)
- **Wildcards**: Try adding `http://localhost:*/Account/SetPassword`

**Issue**: Application won't start on new port
- **Another application** might be using that port
- **Close Visual Studio** completely and restart
- **Try a different port** (5000, 5001, 7000, 7001, etc.)

---

## ?? Pro Tips

### For Team Collaboration
Ask your team lead to add **wildcard URL** to Supabase Dashboard:
```
http://localhost:*/Account/SetPassword
https://localhost:*/Account/SetPassword
```
This allows everyone to use their own port without configuration!

### For Quick Testing
Use a **temporary email service** like:
- https://temp-mail.org
- https://10minutemail.com

This way you don't spam your real email during testing.

---

## ?? Need Help?

1. **Check console logs** - they tell you exactly what's wrong
2. **Read the full guide**: `SETPASSWORD_LOCALHOST_PORT_FIX.md`
3. **Still stuck?** Share these details with your team:
   - Your localhost port
   - Console log output from password setup attempt
   - Any error messages from the SetPassword page

---

**Last Updated**: 2025
**Status**: ? Fix Tested and Working
