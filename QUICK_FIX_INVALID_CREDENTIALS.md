# ?? QUICK ACTION: Fix "Invalid Credentials" Error

## ? **IMMEDIATE STEPS TO FIX**

Your authentication is working correctly, but the user `parillakenrumi@gmail.com` doesn't exist in Supabase Auth or has the wrong password.

---

## ?? **SOLUTION 1: Create User in Supabase Dashboard (5 minutes)**

### **Step 1: Go to Supabase Dashboard**
```
https://supabase.com/dashboard/project/fregpzxzivwhfcvauqmb/auth/users
```

### **Step 2: Create User**
1. Click **"Add User"** button (top right)
2. Fill in:
   - **Email:** `parillakenrumi@gmail.com`
   - **Password:** Choose a secure password (e.g., `SecurePass123!`)
   - ? Check **"Auto Confirm User"**
3. Click **"Create User"**
4. **Copy the User ID** (UUID format)

### **Step 3: Add to Your Database**

Go to **SQL Editor** in Supabase and run:

```sql
-- Replace <USER_ID> with the UUID you copied
-- Example: '12345678-1234-1234-1234-123456789012'

-- Add to users table
INSERT INTO users (
    id,
    email,
    first_name,
    last_name,
    user_type_id,
    is_active,
created_at
) VALUES (
    '<USER_ID>',  -- Paste the UUID here
    'parillakenrumi@gmail.com',
    'Ken',
  'Parilla',
    '1',  -- 1 = Student, 2 = Teacher, 3 = Admin
    true,
    NOW()
);

-- Assign role (1 = Student, 2 = Teacher, 3 = Admin)
INSERT INTO user_roles (user_id, role_id)
VALUES ('<USER_ID>', 1);  -- Change 1 to appropriate role

-- Verify
SELECT 
    u.email,
    u.first_name,
    u.last_name,
    r.role_name
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
WHERE u.email = 'parillakenrumi@gmail.com';
```

### **Step 4: Try Logging In**
- Email: `parillakenrumi@gmail.com`
- Password: (the password you set in Step 2)

---

## ?? **SOLUTION 2: Create Test User (Quick Test)**

If you want to test immediately with a new account:

### **Create Test User:**

1. Go to Supabase Dashboard ? **Add User**
2. Fill in:
   - Email: `test@acadus.com`
   - Password: `Test1234!`
   - ? Auto Confirm User
3. Click "Create User" and copy the UUID

### **Add to Database:**

```sql
-- Replace <TEST_USER_ID> with the UUID from Step 3

INSERT INTO users (id, email, first_name, last_name, user_type_id, is_active, created_at)
VALUES ('<TEST_USER_ID>', 'test@acadus.com', 'Test', 'User', '1', true, NOW());

INSERT INTO user_roles (user_id, role_id)
VALUES ('<TEST_USER_ID>', 1);
```

### **Login:**
- Email: `test@acadus.com`
- Password: `Test1234!`

---

## ?? **SOLUTION 3: Reset Existing User Password**

If the user already exists but you forgot the password:

1. Go to Supabase Dashboard ? Authentication ? Users
2. Search for: `parillakenrumi@gmail.com`
3. Click on the user
4. Click **"Reset Password"**
5. Enter new password: `NewPassword123!`
6. Click **"Update Password"**
7. Try logging in with the new password

---

## ?? **WHAT YOU'LL SEE AFTER FIX**

### **Console Output (Success):**
```
=== SIGN IN ATTEMPT ===
Email: parillakenrumi@gmail.com
Password Length: 18 characters
Using Anon Key for authentication: eyJhbGci...
? Auth client initialized with Anon Key
Attempting to sign in with Supabase Auth...
? Sign in SUCCESS!  ? THIS IS WHAT YOU WANT
  - User ID: 12345-67890-abcdef
  - Email: parillakenrumi@gmail.com
- Email Confirmed: True
```

### **Browser:**
- Redirected to dashboard
- No error message

---

## ?? **COMMON MISTAKES**

1. **Forgot to check "Auto Confirm User"**
   - User gets created but email not confirmed
   - Solution: Manually confirm in Dashboard

2. **Forgot to add user to `users` table**
   - User exists in auth but not in your database
   - Solution: Run the INSERT SQL above

3. **Forgot to assign role**
   - User has no role assigned
   - Solution: Run the user_roles INSERT SQL

4. **UUID mismatch**
   - Used wrong UUID in INSERT statements
   - Solution: Copy UUID exactly from auth.users

---

## ? **CHECKLIST**

- [ ] User exists in Supabase Dashboard ? Authentication ? Users
- [ ] Email is confirmed (has timestamp or Auto Confirm checked)
- [ ] User added to `users` table (SQL INSERT completed)
- [ ] Role assigned in `user_roles` table
- [ ] Password is correct and remembered
- [ ] Try logging in

---

## ?? **EXPECTED TIMELINE**

- **Create user in Dashboard:** 2 minutes
- **Run SQL to add to database:** 1 minute
- **Test login:** 30 seconds
- **Total:** ~5 minutes

---

## ?? **IF STILL NOT WORKING**

After completing all steps above, if login still fails:

1. **Check console output** - Copy the full error message
2. **Verify SQL ran successfully** - Check the SELECT query results
3. **Check email spelling** - Must match exactly (case-insensitive)
4. **Try with test@acadus.com** - Eliminate variables

---

**Follow Solution 1 and you should be able to login in 5 minutes!** ?
