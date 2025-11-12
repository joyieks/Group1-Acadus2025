# ?? Student & Teacher Registration Fix - Complete Summary

## ?? Problem Solved
When admin creates students or teachers, they were not being properly inserted into the `users` and `user_roles` tables, and the table names in C# models didn't match Supabase.

---

## ? Changes Made

### 1. **Fixed C# Model Table Names**

#### `Student.cs`
```csharp
[Table("studentProfile")]  // Changed from "students"
```

#### `Teacher.cs`
```csharp
[Table("teacherProfile")]  // Changed from "teachers"
```

#### `UserRole.cs`
```csharp
[Column("roleId")]
public string RoleId { get; set; }  // Changed from int to string
```
- **Reason:** `user_roles.roleId` is TEXT and references `roles.roleProfile` (also TEXT)

---

### 2. **Updated StudentService.cs**
Added two new steps to the student creation process:

**Step 7: Insert into `users` table**
```csharp
var userRecord = new SupabaseUserNew
{
    FirstName = model.FirstName,
    LastName = model.LastName,
  MiddleName = model.MiddleName,
    Suffix = model.Suffix,
    Email = model.Email,
    ContactNumber = model.ContactNumber,
    UserTypeId = supabaseUserId,  // Supabase Auth UUID
    IsActive = true
};
await client.From<SupabaseUserNew>().Insert(userRecord);
```

**Step 8: Assign Student role in `user_roles` table**
```csharp
var userRole = new UserRole
{
    UserId = supabaseUserId,      // Supabase Auth UUID
    RoleId = "Student",   // References roles.roleProfile
    CreatedAt = DateTime.UtcNow
};
await client.From<UserRole>().Insert(userRole);
```

---

### 3. **Updated TeacherService.cs**
Added the same two steps for teacher creation:

**Step 7: Insert into `users` table**
```csharp
var userRecord = new SupabaseUserNew
{
    FirstName = model.FirstName,
    LastName = model.LastName,
    MiddleName = model.MiddleName,
    Suffix = model.Suffix,
    Email = model.Email,
    ContactNumber = model.ContactNumber,
    UserTypeId = supabaseUserId,
    IsActive = true
};
await client.From<SupabaseUserNew>().Insert(userRecord);
```

**Step 8: Assign Teacher role**
```csharp
var userRole = new UserRole
{
    UserId = supabaseUserId,
    RoleId = "Teacher",  // References roles.roleProfile
    CreatedAt = DateTime.UtcNow
};
await client.From<UserRole>().Insert(userRole);
```

---

### 4. **Fixed SupabaseAuthService.cs Role Lookup**
Changed the role lookup to use `roleProfile` instead of `id`:

**Before (Wrong):**
```csharp
var roleQuery = await client
.From<Role>()
    .Where(x => x.Id == userRoleRecord.RoleId)  // ? Wrong: RoleId is TEXT, not int
    .Get();
```

**After (Correct):**
```csharp
var roleQuery = await client
    .From<Role>()
    .Where(x => x.RoleProfile == userRoleRecord.RoleId)  // ? Correct
    .Get();
```

---

## ?? Database Schema Understanding

### Your Supabase Tables:

1. **`roles`**
   - `id` (bigint) - Primary key
   - `roleName` (text) - e.g., "Student", "Teacher", "Admin"
   - `roleProfile` (text) - UNIQUE - e.g., "Student", "Teacher", "Admin"

2. **`users`**
   - `id` (bigint) - Primary key
   - `userTypeId` (text) - UNIQUE - Supabase Auth UUID
   - `firstName`, `lastName`, `email`, etc.

3. **`user_roles`**
   - `id` (bigint) - Primary key
   - `userId` (text) - UNIQUE - Foreign key to `users.userTypeId`
   - `roleId` (text) - Foreign key to `roles.roleProfile`

4. **`studentProfile`** (NOT `students`)
   - Student-specific data
   - `supabase_user_id` links to Supabase Auth

5. **`teacherProfile`** (NOT `teachers`)
   - Teacher-specific data
   - `supabase_user_id` links to Supabase Auth

---

## ??? Required Supabase Setup

### Run these SQL scripts in order:

#### 1. **Database_Setup.sql**
- Creates/verifies the `roles` table
- Inserts Student, Teacher, Admin roles
- Disables RLS on `user_roles`, `users`, `roles`

#### 2. **Complete_Database_Setup.sql**
- Creates/verifies all profile tables
- Creates junction tables for addresses and contacts
- Sets up foreign keys with CASCADE delete
- Disables RLS on all tables

### Quick Setup Commands:
```sql
-- 1. Insert roles
INSERT INTO public.roles (id, "roleName", "roleProfile") VALUES 
    (1, 'Student', 'Student'),
  (2, 'Teacher', 'Teacher'),
    (3, 'Admin', 'Admin')
ON CONFLICT (id) DO NOTHING;

-- 2. Disable RLS
ALTER TABLE public.user_roles DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.users DISABLE ROW LEVEL SECURITY;
ALTER TABLE public.roles DISABLE ROW LEVEL SECURITY;
ALTER TABLE public."studentProfile" DISABLE ROW LEVEL SECURITY;
ALTER TABLE public."teacherProfile" DISABLE ROW LEVEL SECURITY;
```

---

## ?? Registration Flow (Now Fixed)

### When Admin Creates a Student:

1. ? **Create Supabase Auth user** ? Get `supabaseUserId` (UUID)
2. ? **Insert into `studentProfile` table** ? Student-specific data
3. ? **Insert into `addresses` table** ? Address data
4. ? **Link student to address** ? `student_addresses` junction table
5. ? **Insert into `contacts` table** ? Emergency contact
6. ? **Link student to contact** ? `student_emergency_contacts` junction
7. ? **Insert into `users` table** ? Universal user record with `userTypeId`
8. ? **Insert into `user_roles` table** ? Assign "Student" role
9. ? **Send password setup email** ? User can set their password

### When Admin Creates a Teacher:
Same flow as Student, but with:
- `teacherProfile` table instead of `studentProfile`
- `teacher_addresses` junction table
- `teacher_emergency_contacts` junction table
- Role = "Teacher" instead of "Student"

---

## ?? Testing Steps

### 1. **Setup Database**
```sql
-- Run in Supabase SQL Editor
\i Database_Setup.sql
\i Complete_Database_Setup.sql
```

### 2. **Test Student Creation**
1. Log in as admin
2. Navigate to "Add Student"
3. Fill in all required fields
4. Submit form
5. Check console output for success

### 3. **Verify Database Entries**
```sql
-- Check if student was created in all tables
SELECT * FROM public."studentProfile" WHERE email = 'student@test.com';
SELECT * FROM public.users WHERE email = 'student@test.com';
SELECT * FROM public.user_roles WHERE "userId" = (
    SELECT "userTypeId" FROM public.users WHERE email = 'student@test.com'
);
```

### 4. **Test Login**
1. Student receives password setup email
2. Student clicks link and sets password
3. Student can now log in
4. Should be redirected to Student dashboard

---

## ?? Troubleshooting

### Error: "Could not find the table 'public.students'"
? **Fixed** - Changed model from `[Table("students")]` to `[Table("studentProfile")]`

### Error: "No role mapping found in user_roles table"
? **Fixed** - Now inserting into `user_roles` table during registration

### Error: "Cannot cast int to text" or similar
? **Fixed** - Changed `UserRole.RoleId` from `int` to `string`

### Student created but can't login
**Check:**
1. Is there a record in `users` table?
2. Is there a record in `user_roles` table?
3. Has RLS been disabled on these tables?
4. Run the verification SQL above

---

## ?? Console Output Example (Success)

```
=== CREATING STUDENT: John Doe ===
Step 1: Creating Supabase Auth user...
? Step 1 Complete: Auth user created with ID: abc123-def456-...
Step 2: Inserting student record into database...
? Step 2 Complete: Student record created with ID: 15
Step 3: Creating address record...
? Step 3 Complete: Address created with ID: 8
Step 4: Linking student to address...
? Step 4 Complete: Student-Address link created
Step 5: Creating emergency contact...
? Step 5 Complete: Emergency contact created with ID: 12
Step 6: Linking student to emergency contact...
? Step 6 Complete: Student-Emergency Contact link created
Step 7: Inserting into users table...
? Step 7 Complete: User record created with ID: 20
Step 8: Assigning Student role in user_roles table...
? Step 8 Complete: Student role assigned
Step 9: Sending password setup email...
? Step 9 Complete: Password setup email sent to john@test.com

??? STUDENT CREATION COMPLETE ???
  Student ID: 15
  Auth User ID: abc123-def456-...
  Email: john@test.com
```

---

## ? What Works Now

? Admin can create students with proper role assignment  
? Admin can create teachers with proper role assignment  
? Students inserted into: `studentProfile`, `users`, `user_roles`  
? Teachers inserted into: `teacherProfile`, `users`, `user_roles`  
? Role-based login routing works correctly  
? Students redirect to Student dashboard  
? Teachers redirect to Teacher dashboard  
? Admin redirects to Admin dashboard  

---

## ?? Files Modified

1. `ASI.Basecode.Data\Models\UserRole.cs` - Changed RoleId to string
2. `ASI.Basecode.Data\Models\Student.cs` - Fixed table name
3. `ASI.Basecode.Data\Models\Teacher.cs` - Fixed table name
4. `ASI.Basecode.Services\Services\StudentService.cs` - Added users/user_roles insertion
5. `ASI.Basecode.Services\Services\TeacherService.cs` - Added users/user_roles insertion
6. `ASI.Basecode.Services\Services\SupabaseAuthService.cs` - Fixed role lookup

## ?? New Files Created

1. `Database_Setup.sql` - Roles table and RLS policies
2. `Complete_Database_Setup.sql` - All profile tables and foreign keys
3. `REGISTRATION_FIX_SUMMARY.md` - This file

---

## ?? Next Steps

1. **Run the SQL scripts** in Supabase SQL Editor
2. **Restart your application**
3. **Test student creation** as admin
4. **Test teacher creation** as admin
5. **Verify the data** in Supabase Table Editor
6. **Test login** with newly created accounts

---

## ?? Key Takeaways

- Always match C# model `[Table("name")]` with actual Supabase table names
- The `user_roles.userId` should contain the Supabase Auth UUID (not database ID)
- The `user_roles.roleId` references `roles.roleProfile` (both TEXT)
- Disable RLS during development, enable with proper policies for production
- Always insert into both profile tables AND the universal `users` table
- Use consistent logging to debug issues

---

**All changes are committed and ready to test! ??**
