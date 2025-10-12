# 🚨 URGENT: Security Setup Guide

## ⚠️ CRITICAL: Secrets Were Exposed

**Your secrets were exposed in the Git history and need to be rotated immediately!**

## 🔧 How to Fix This

### Step 1: Commit the Clean Version
```bash
git add .
git commit -m "SECURITY: Remove exposed secrets from configuration files"
git push origin feature/teacher-student-authentication
```

### Step 2: Set Up Local Development (For You)
1. Copy the local development file:
   ```bash
   cp appsettings.Development.local.json appsettings.Development.local.json
   ```
2. The file `appsettings.Development.local.json` contains working secrets for development
3. This file is in `.gitignore` and will NOT be committed

### Step 3: For Your Team Members
Your teammates need to:

1. **Pull the latest code**:
   ```bash
   git pull origin feature/teacher-student-authentication
   ```

2. **Create their own local configuration**:
   ```bash
   cp appsettings.Development.json appsettings.Development.local.json
   ```

3. **Edit `appsettings.Development.local.json`** with their own secrets:
   ```json
   {
     "Supabase": {
       "Url": "https://fregpzxzivwhfcvauqmb.supabase.co",
       "AnonKey": "YOUR_SUPABASE_ANON_KEY",
       "ServiceRoleKey": "YOUR_SUPABASE_SERVICE_ROLE_KEY",
       "RedirectUrl": "https://localhost:63125/Account/SetPassword"
     },
     "TokenAuthentication": {
       "SecretKey": "YOUR_JWT_SECRET_KEY"
     },
     "Email": {
       "SmtpUsername": "YOUR_EMAIL",
       "SmtpPassword": "YOUR_EMAIL_PASSWORD"
     },
     "Admin": {
       "Email": "admin@gmail.com",
       "Password": "admin123"
     },
     "Teacher": {
       "Email": "teacher@gmail.com",
       "Password": "teacher123"
     }
   }
   ```

## 🔒 URGENT: Rotate Compromised Secrets

### 1. Supabase Service Role Key
- Go to your Supabase dashboard
- Navigate to Settings > API
- **Generate a NEW service role key**
- Update your `appsettings.Development.local.json`

### 2. JWT Secret Key
- Generate a new secure random string:
  ```bash
  # On Windows (PowerShell)
  [System.Web.Security.Membership]::GeneratePassword(32, 8)
  
  # Or use online generator
  # https://www.allkeysgenerator.com/Random/Security-Encryption-Key-Generator.aspx
  ```

### 3. SMTP Credentials
- Change your Gmail password OR
- Generate a new app-specific password
- Update your `appsettings.Development.local.json`

## 🛡️ Security Status

### ✅ What's Fixed:
- Main `appsettings.json`: Uses environment variables (secure)
- Development template: Uses placeholders (secure)
- Local development: Uses actual secrets (not committed)
- Git history: Clean going forward

### ⚠️ What You Must Do:
1. **Rotate all exposed secrets immediately**
2. **Update your local configuration**
3. **Inform your team about the security issue**

## 🚀 How It Works Now

### For Development:
- `appsettings.json` → Environment variable placeholders
- `appsettings.Development.json` → Template with placeholders
- `appsettings.Development.local.json` → Your actual secrets (not committed)

### For Production:
- Set environment variables on your production server
- No secrets in code!

## 📞 If You Need Help

1. **Supabase**: Check their documentation for rotating keys
2. **Gmail**: Use app-specific passwords for SMTP
3. **JWT**: Generate a strong random string (32+ characters)

## ⚡ Quick Start for Team

Your teammates can run the app immediately by:
1. Pulling the latest code
2. Creating `appsettings.Development.local.json` with their secrets
3. Running the application

**The app will work perfectly once they add their own secrets!**
