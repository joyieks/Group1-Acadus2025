# ? CUSTOM STUDENT/TEACHER ID IMPLEMENTATION GUIDE

## ?? **OBJECTIVE**

Implement a professional ID system like real-world schools:
- **Students:** `STU-202511001` (Year 2025, November, 1st student)
- **Teachers:** `FAC-202511001` (Year 2025, November, 1st faculty)
- **Admins:** `ADM-202511001` (Year 2025, November, 1st admin)

**While maintaining** the existing UUID-based authentication system.

---

## ?? **CURRENT vs NEW SYSTEM**

### **Current System:**
```
users.userTypeId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID)
studentProfile.studentId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID)
user_roles.userId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID)
```

### **New System:**
```
users.userTypeId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID - for auth)
users.studentNumber = "STU-202511001" (Display ID - for humans)

studentProfile.studentId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID - for FK)
studentProfile.studentNumber = "STU-202511001" (Display ID - for display)

user_roles.userId = "3fc54222-9115-498a-917e-63ac80ce9f78" (UUID - for FK)
```

**Key Point:** UUIDs stay for **authentication & relationships**, Display IDs are added for **human readability**.

---

## ??? **DATABASE CHANGES**

### **Step 1: Add studentNumber Column to tables**

Run this SQL in Supabase:

```sql
-- ============================================
-- ADD STUDENT/TEACHER NUMBER COLUMNS
-- ============================================

-- Add to users table
ALTER TABLE users 
ADD COLUMN IF NOT EXISTS "studentNumber" text UNIQUE;

CREATE INDEX IF NOT EXISTS idx_users_student_number 
ON users("studentNumber");

-- Add to studentProfile table (optional redundancy)
ALTER TABLE "studentProfile"
ADD COLUMN IF NOT EXISTS "studentNumber" text UNIQUE;

CREATE INDEX IF NOT EXISTS idx_student_profile_student_number 
ON "studentProfile"("studentNumber");

-- Add to teacherProfile table
ALTER TABLE "teacherProfile"
ADD COLUMN IF NOT EXISTS "teacherNumber" text UNIQUE;

CREATE INDEX IF NOT EXISTS idx_teacher_profile_teacher_number 
ON "teacherProfile"("teacherNumber");

-- ============================================
-- CREATE SEQUENCE TRACKING TABLE (OPTIONAL)
-- ============================================

CREATE TABLE IF NOT EXISTS id_sequences (
    id serial PRIMARY KEY,
    prefix text NOT NULL UNIQUE, -- STU, FAC, ADM
    year_month text NOT NULL, -- 202511
 last_index integer NOT NULL DEFAULT 0,
    created_at timestamptz DEFAULT NOW(),
    updated_at timestamptz DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_id_sequences_prefix_year_month 
ON id_sequences(prefix, year_month);

-- ============================================
-- VERIFY CHANGES
-- ============================================

-- Check users table
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'users' 
AND column_name = 'studentNumber';

-- Check studentProfile table
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'studentProfile' 
AND column_name = 'studentNumber';
```

---

## ?? **CODE IMPLEMENTATION**

### **Files Created:**

1. ? `IdGeneratorService.cs` - Service to generate unique IDs

### **Files to Update:**

2. ? `SupabaseUserNew.cs` - Add `StudentNumber` property
3. ? `Student.cs` - Add `StudentNumber` property
4. ? `Teacher.cs` - Add `TeacherNumber` property
5. ? `StudentService.cs` - Generate student numbers on creation
6. ? `TeacherService.cs` - Generate teacher numbers on creation
7. ? `Startup.DI.cs` - Register `IdGeneratorService`

---

## ?? **IMPLEMENTATION STEPS**

### **Step 1: Run Database Migration**

```sql
-- Run the SQL script above in Supabase SQL Editor
```

### **Step 2: Update StudentService.cs**

Add IdGeneratorService to constructor and use it:

```csharp
public class StudentService : IStudentService
{
    private readonly ISupabaseAuthService _supabaseAuthService;
    private readonly IConfiguration _configuration;
private readonly IdGeneratorService _idGenerator; // ? NEW

    public StudentService(
        ISupabaseAuthService supabaseAuthService, 
        IConfiguration configuration,
        IdGeneratorService idGenerator) // ? NEW
    {
        _supabaseAuthService = supabaseAuthService;
        _configuration = configuration;
        _idGenerator = idGenerator; // ? NEW
    }

public async Task<bool> CreateStudentAsync(StudentViewModel model)
    {
// ... existing code ...

// Step 1: Create Supabase Auth user (gets UUID)
   var supabaseUserId = await _supabaseAuthService.CreateUserAsync(...);

   // ? NEW: Step 1.5: Generate student number
        var studentNumber = await _idGenerator.GenerateStudentIdAsync();
        Console.WriteLine($"Generated student number: {studentNumber}");

        // Step 2: Create user record with student number
        var userRecord = new SupabaseUserNew
        {
            UserTypeId = supabaseUserId,  // UUID for auth
         StudentNumber = studentNumber,  // ? NEW: Display ID
    FirstName = model.FirstName,
 LastName = model.LastName,
            Email = model.Email,
    // ... other fields
        };
        
        var insertedUser = await client.From<SupabaseUserNew>().Insert(userRecord);

     // Step 4: Create studentProfile with student number
        var student = new Student
        {
            StudentId = supabaseUserId,  // UUID for FK
     StudentNumber = studentNumber,  // ? NEW: Display ID
     YearLevel = model.YearLevel,
   ProgramId = programId,
            DepartmentId = departmentId,
    CreatedAt = DateTime.UtcNow
        };

        await client.From<Student>().Insert(student);

        // ... rest of code
    }
}
```

### **Step 3: Update Views to Display Student Number**

**In Admin Dashboard:**

```html
<!-- Display student number instead of UUID -->
<td>@student.StudentNumber</td>  <!-- STU-202511001 -->
<td>@student.FirstName @student.LastName</td>
```

**In Student Profile:**

```html
<div>
    <label>Student ID:</label>
    <span>@Model.StudentNumber</span>  <!-- STU-202511001 -->
</div>
```

---

## ?? **TESTING**

### **Test Scenario 1: Create First Student in November 2025**

1. Run application
2. Login as admin
3. Add student (John Doe)
4. Expected Student Number: `STU-202511001`
5. Verify in database:
 ```sql
   SELECT "studentNumber", "firstName", "lastName", email
   FROM users
   WHERE "studentNumber" LIKE 'STU-%'
   ORDER BY "studentNumber" DESC
   LIMIT 5;
   ```

### **Test Scenario 2: Create Second Student Same Month**

1. Add another student (Jane Smith)
2. Expected Student Number: `STU-202511002` (index incremented)

### **Test Scenario 3: Create Student in December 2025**

1. Wait until December (or change system date for testing)
2. Add student
3. Expected Student Number: `STU-202512001` (new month, index resets)

### **Test Scenario 4: Verify UUID Still Works**

1. Student logs in with email/password
2. Verify authentication uses UUID (not student number)
3. Verify database relationships use UUID

---

## ?? **VERIFICATION QUERIES**

### **Check Student Numbers Generated:**

```sql
SELECT 
    u."studentNumber",
 u."firstName",
    u."lastName",
    u.email,
    sp."yearLevel",
    p."programName"
FROM users u
LEFT JOIN "studentProfile" sp ON u."userTypeId" = sp."studentId"
LEFT JOIN programs p ON sp.programid = p.id
WHERE u."studentNumber" LIKE 'STU-%'
ORDER BY u."studentNumber" DESC
LIMIT 10;
```

**Expected Output:**
```
studentNumber   | firstName | lastName | email    | yearLevel | programName
STU-202511003   | Alice     | Johnson  | alice.j@uni.edu | 1      | BSIT
STU-202511002   | Jane      | Smith    | jane.s@uni.edu   | 2   | BSCS
STU-202511001   | John      | Doe      | john.d@uni.edu       | 1         | BSIT
```

### **Verify UUIDs Still Exist:**

```sql
SELECT 
    u.id,
u."userTypeId",  -- Should be UUID
    u."studentNumber",  -- Should be STU-YYYYMMNNN
    u.email
FROM users u
WHERE u."studentNumber" = 'STU-202511001';
```

**Expected Output:**
```
id | userTypeId       | studentNumber   | email
10 | 3fc54222-9115-498a-917e-63ac80ce9f78 | STU-202511001   | john.d@uni.edu
```

---

## ?? **ID FORMAT SPECIFICATION**

### **Student ID:**
- **Format:** `STU-YYYYMMNNN`
- **Example:** `STU-202511001`
- **Parts:**
  - `STU` - Prefix for students
  - `2025` - Year
  - `11` - Month (November)
  - `001` - Sequential index (zero-padded to 3 digits)

### **Teacher ID:**
- **Format:** `FAC-YYYYMMNNN`
- **Example:** `FAC-202511001`

### **Admin ID:**
- **Format:** `ADM-YYYYMMNNN`
- **Example:** `ADM-202511001`

### **Index Behavior:**
- Resets each month
- `001-999` for up to 999 students per month
- Automatically expands if needed (`1000`, `1001`, etc.)

---

## ?? **UI/UX IMPROVEMENTS**

### **Before:**
```
Student ID: 3fc54222-9115-498a-917e-63ac80ce9f78
```

### **After:**
```
Student Number: STU-202511001
Student Name: John Doe
Email: john.doe@university.edu
```

### **Admin Dashboard Table:**

| Student Number | Name | Email | Program | Year Level |
|----------------|------|-------|---------|------------|
| STU-202511001  | John Doe | john.d@uni.edu | BSIT | 1 |
| STU-202511002  | Jane Smith | jane.s@uni.edu | BSCS | 2 |

---

## ?? **SECURITY CONSIDERATIONS**

### **? UUIDs Still Used For:**
- Authentication (Supabase Auth)
- Database foreign keys
- Internal relationships
- API tokens

### **? Student Numbers Used For:**
- Display purposes
- Reports
- Student cards
- Printed materials
- Human communication

### **?? Important:**
- Student numbers are **NOT** used for authentication
- Student numbers are **public** (safe to display)
- UUIDs remain **private** (internal use only)

---

## ?? **MIGRATION STRATEGY**

### **For Existing Students:**

If you already have students in the database without student numbers:

```sql
-- Generate student numbers for existing students
-- WARNING: This will assign numbers based on creation date

WITH numbered_users AS (
    SELECT 
    id,
        "userTypeId",
        ROW_NUMBER() OVER (
ORDER BY 
     EXTRACT(YEAR FROM created_at),
       EXTRACT(MONTH FROM created_at),
     id
        ) as row_num,
        EXTRACT(YEAR FROM created_at) as year,
        LPAD(EXTRACT(MONTH FROM created_at)::text, 2, '0') as month
    FROM users
    WHERE "studentNumber" IS NULL
    AND "userTypeId" IN (
        SELECT "userId" FROM user_roles WHERE "roleId" = 1
    )
)
UPDATE users u
SET "studentNumber" = 'STU-' || nu.year || nu.month || LPAD(nu.row_num::text, 3, '0')
FROM numbered_users nu
WHERE u.id = nu.id;

-- Verify
SELECT "studentNumber", "firstName", "lastName", email
FROM users
WHERE "studentNumber" LIKE 'STU-%'
ORDER BY "studentNumber";
```

---

## ?? **BENEFITS**

### **1. Professional Appearance**
- Matches real-world school systems
- Easy to remember and communicate
- Professional on documents

### **2. Better UX**
- Students can identify themselves easily
- No need to copy/paste long UUIDs
- Clear and readable

### **3. Data Integrity**
- UUIDs still ensure uniqueness
- Student numbers provide human-friendly reference
- Best of both worlds

### **4. Reporting**
- Easy to sort by enrollment date
- Can analyze by year/month
- Professional reports

---

## ?? **TROUBLESHOOTING**

### **Issue: Duplicate Student Numbers**

**Cause:** Race condition when creating multiple students simultaneously

**Solution:** Use database sequence or locking:

```sql
-- Option 1: Use id_sequences table with FOR UPDATE lock
BEGIN;

SELECT last_index FROM id_sequences
WHERE prefix = 'STU' AND year_month = '202511'
FOR UPDATE;

UPDATE id_sequences 
SET last_index = last_index + 1
WHERE prefix = 'STU' AND year_month = '202511';

COMMIT;
```

### **Issue: Student Number Not Generated**

**Check:**
1. Is `IdGeneratorService` registered in DI?
2. Is database column created?
3. Check console logs for errors

### **Issue: Old Students Don't Have Numbers**

Run the migration SQL above to generate numbers for existing students.

---

## ?? **CHECKLIST**

- [ ] Run database migration SQL
- [ ] Add `StudentNumber` property to models
- [ ] Update `StudentService.cs` to generate numbers
- [ ] Update `TeacherService.cs` to generate numbers
- [ ] Register `IdGeneratorService` in DI
- [ ] Update admin dashboard to display student numbers
- [ ] Update student profile to display student number
- [ ] Test creating new students
- [ ] Verify student numbers are unique
- [ ] Verify UUIDs still work for auth
- [ ] Generate numbers for existing students (if any)
- [ ] Update reports to use student numbers

---

## ?? **SUMMARY**

### **Solution:**
Add a **separate display ID column** (`studentNumber`) while keeping the **UUID for authentication**.

### **Implementation:**
1. ? Database: Add `studentNumber` column
2. ? Service: `IdGeneratorService` generates IDs
3. ? Models: Add `StudentNumber` property
4. ? Service: Use generator in `StudentService.CreateStudentAsync`
5. ? Views: Display student number instead of UUID

### **Result:**
- ? Professional IDs (STU-202511001)
- ? Secure authentication (UUID)
- ? Best of both worlds!

---

**Your system will now have professional, readable student IDs while maintaining secure UUID-based authentication!** ???
