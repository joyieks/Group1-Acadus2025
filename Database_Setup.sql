-- =====================================================
-- Supabase Database Setup for User Roles
-- =====================================================
-- Run this SQL in your Supabase SQL Editor
-- =====================================================

-- Step 1: Insert the three main roles (if they don't exist)
-- The roleProfile column is what's referenced by user_roles.roleId
INSERT INTO public.roles (id, "roleName", "roleProfile", created_at)
VALUES 
    (1, 'Student', 'Student', NOW()),
    (2, 'Teacher', 'Teacher', NOW()),
    (3, 'Admin', 'Admin', NOW())
ON CONFLICT (id) DO NOTHING;

-- Step 2: Verify roles were created
SELECT * FROM public.roles ORDER BY id;

-- Step 3: Disable RLS on user_roles table (if enabled)
ALTER TABLE public.user_roles DISABLE ROW LEVEL SECURITY;

-- Step 4: Disable RLS on users table (if enabled)
ALTER TABLE public.users DISABLE ROW LEVEL SECURITY;

-- Step 5: Disable RLS on roles table (if enabled)
ALTER TABLE public.roles DISABLE ROW LEVEL SECURITY;

-- Step 6: Create RLS policies for service role access (better than disabling)
-- Uncomment these if you want to keep RLS enabled:

/*
-- Enable RLS
ALTER TABLE public.user_roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.roles ENABLE ROW LEVEL SECURITY;

-- Policy for service role to access user_roles
CREATE POLICY "Service role can access user_roles"
ON public.user_roles
FOR ALL
TO service_role
USING (true)
WITH CHECK (true);

-- Policy for service role to access users
CREATE POLICY "Service role can access users"
ON public.users
FOR ALL
TO service_role
USING (true)
WITH CHECK (true);

-- Policy for service role to access roles
CREATE POLICY "Service role can access roles"
ON public.roles
FOR ALL
TO service_role
USING (true)
WITH CHECK (true);
*/

-- Step 7: Verify your admin user exists in user_roles
-- Replace 'YOUR_ADMIN_UUID' with your actual admin user's UUID from auth.users
-- Example: '24d6f7ac-a9fb-4c9d-ac27-1e1a2146ca7e'
/*
INSERT INTO public.user_roles ("userId", "roleId", created_at)
VALUES ('YOUR_ADMIN_UUID', 'Admin', NOW())
ON CONFLICT ("userId") DO UPDATE SET "roleId" = 'Admin';
*/

-- Step 8: Verify the setup
SELECT 
    ur.id,
    ur."userId",
    ur."roleId",
    r."roleName",
    u."firstName",
    u."lastName",
    u.email
FROM public.user_roles ur
LEFT JOIN public.roles r ON ur."roleId" = r."roleProfile"
LEFT JOIN public.users u ON ur."userId" = u."userTypeId";

-- Step 9: Check if your admin user exists in the users table
-- If not, you'll need to add them manually
SELECT * FROM public.users WHERE "userTypeId" = '24d6f7ac-a9fb-4c9d-ac27-1e1a2146ca7e';

-- =====================================================
-- Expected Output:
-- =====================================================
-- You should see:
-- 1. Three roles: Student (id=1), Teacher (id=2), Admin (id=3)
-- 2. At least one user_role entry for your admin
-- 3. The admin user in the users table
-- =====================================================
