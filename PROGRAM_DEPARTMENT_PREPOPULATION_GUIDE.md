# ? PROGRAM & DEPARTMENT SETUP - Pre-populate Reference Tables

## ?? **ISSUE EXPLANATION**

The `programs` and `departments` tables are **reference/lookup tables** that should be pre-populated with data. They are NOT automatically created when adding students.

### **How It Works:**

```
1. Admin pre-populates programs and departments (one-time setup)
   ?
2. Dropdown menus show available programs/departments
   ?
3. Admin selects program "BSIT" (id=1) and department "CCS" (id=1)
   ?
4. Student is created with programId=1 and departmentId=1
   ?
5. studentProfile.programId ? references programs.id
6. studentProfile.departmentId ? references departments.id
```

---

## ?? **SQL SCRIPT TO PRE-POPULATE TABLES**

Run this in your Supabase SQL Editor:

```sql
-- ============================================
-- PRE-POPULATE DEPARTMENTS TABLE
-- ============================================

-- Delete existing data (optional - only if you want to start fresh)
-- DELETE FROM departments;
-- DELETE FROM programs;

-- Insert Departments
INSERT INTO departments (id, "departmentName", "departmentCode", created_at) VALUES
(1, 'College of Computer Studies', 'CCS', NOW()),
(2, 'College of Engineering', 'COE', NOW()),
(3, 'College of Business Administration', 'CBA', NOW()),
(4, 'College of Arts and Sciences', 'CAS', NOW()),
(5, 'College of Education', 'COED', NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- PRE-POPULATE PROGRAMS TABLE
-- ============================================

-- Programs under College of Computer Studies (departmentid = 1)
INSERT INTO programs (id, "programName", "programCode", departmentid, created_at) VALUES
(1, 'Bachelor of Science in Information Technology', 'BSIT', 1, NOW()),
(2, 'Bachelor of Science in Computer Science', 'BSCS', 1, NOW()),
(3, 'Bachelor of Science in Information Systems', 'BSIS', 1, NOW())
ON CONFLICT (id) DO NOTHING;

-- Programs under College of Engineering (departmentid = 2)
INSERT INTO programs (id, "programName", "programCode", departmentid, created_at) VALUES
(4, 'Bachelor of Science in Civil Engineering', 'BSCE', 2, NOW()),
(5, 'Bachelor of Science in Mechanical Engineering', 'BSME', 2, NOW()),
(6, 'Bachelor of Science in Electrical Engineering', 'BSEE', 2, NOW())
ON CONFLICT (id) DO NOTHING;

-- Programs under College of Business Administration (departmentid = 3)
INSERT INTO programs (id, "programName", "programCode", departmentid, created_at) VALUES
(7, 'Bachelor of Science in Business Administration', 'BSBA', 3, NOW()),
(8, 'Bachelor of Science in Accountancy', 'BSA', 3, NOW()),
(9, 'Bachelor of Science in Entrepreneurship', 'BSE', 3, NOW())
ON CONFLICT (id) DO NOTHING;

-- Programs under College of Arts and Sciences (departmentid = 4)
INSERT INTO programs (id, "programName", "programCode", departmentid, created_at) VALUES
(10, 'Bachelor of Arts in Communication', 'BA Comm', 4, NOW()),
(11, 'Bachelor of Science in Psychology', 'BS Psych', 4, NOW()),
(12, 'Bachelor of Science in Mathematics', 'BS Math', 4, NOW())
ON CONFLICT (id) DO NOTHING;

-- Programs under College of Education (departmentid = 5)
INSERT INTO programs (id, "programName", "programCode", departmentid, created_at) VALUES
(13, 'Bachelor of Elementary Education', 'BEEd', 5, NOW()),
(14, 'Bachelor of Secondary Education Major in English', 'BSEd English', 5, NOW()),
(15, 'Bachelor of Secondary Education Major in Mathematics', 'BSEd Math', 5, NOW())
ON CONFLICT (id) DO NOTHING;

-- ============================================
-- VERIFY DATA
-- ============================================

-- Check departments
SELECT * FROM departments ORDER BY id;

-- Check programs with their departments
SELECT 
    p.id,
    p."programName",
    p."programCode",
 d."departmentName"
FROM programs p
LEFT JOIN departments d ON p.departmentid = d.id
ORDER BY p.id;

-- ============================================
-- RESET SEQUENCES (if using serial/auto-increment)
-- ============================================

-- Reset department sequence to start from next ID
SELECT setval('departments_id_seq', (SELECT MAX(id) FROM departments));

-- Reset program sequence to start from next ID
SELECT setval('programs_id_seq', (SELECT MAX(id) FROM programs));
```

---

## ?? **DATABASE SCHEMA FIXES**

Your `studentProfile` table has incorrect column types:

### **Current Schema (WRONG):**

```sql
CREATE TABLE "studentProfile" (
    id integer PRIMARY KEY,
    "studentId" text,
    "yearLevel" numeric,
    programid text,  -- ? WRONG: Should be integer
    "departmentId" text,  -- ? WRONG: Should be integer
    created_at timestamptz
);
```

### **Corrected Schema:**

```sql
-- Fix programid column type
ALTER TABLE "studentProfile" 
ALTER COLUMN programid TYPE integer USING programid::integer;

-- Fix departmentId column type
ALTER TABLE "studentProfile" 
ALTER COLUMN "departmentId" TYPE integer USING "departmentId"::integer;

-- Add foreign key constraints
ALTER TABLE "studentProfile"
ADD CONSTRAINT fk_student_program 
FOREIGN KEY (programid) REFERENCES programs(id);

ALTER TABLE "studentProfile"
ADD CONSTRAINT fk_student_department 
FOREIGN KEY ("departmentId") REFERENCES departments(id);
```

---

## ?? **COMPLETE SETUP STEPS**

### **Step 1: Run Pre-population SQL**

1. Go to Supabase Dashboard
2. Click **SQL Editor**
3. Paste the pre-population SQL above
4. Click **Run**
5. Verify data appears in tables

### **Step 2: Fix Column Types (if needed)**

1. In SQL Editor, run:
```sql
   -- Check current column types
   SELECT column_name, data_type 
   FROM information_schema.columns 
   WHERE table_name = 'studentProfile' 
   AND column_name IN ('programid', 'departmentId');
   ```

2. If they're `text`, fix them:
   ```sql
   ALTER TABLE "studentProfile" 
   ALTER COLUMN programid TYPE integer USING programid::integer;
   
   ALTER TABLE "studentProfile" 
   ALTER COLUMN "departmentId" TYPE integer USING "departmentId"::integer;
   ```

### **Step 3: Test Adding a Student**

1. Run your application
2. Login as admin
3. Go to Add Student
4. Fill in details
5. **Select Program:** BSIT (id=1)
6. **Select Department:** College of Computer Studies (id=1)
7. Submit

### **Step 4: Verify Data**

```sql
-- Check student was created with correct IDs
SELECT 
    sp.id,
    u."firstName",
    u."lastName",
    sp."yearLevel",
    sp.programid,
    p."programName",
    sp."departmentId",
    d."departmentName"
FROM "studentProfile" sp
JOIN users u ON sp."studentId" = u."user_type_id"
LEFT JOIN programs p ON sp.programid = p.id
LEFT JOIN departments d ON sp."departmentId" = d.id
WHERE sp.id = (SELECT MAX(id) FROM "studentProfile");
```

**Expected Result:**
```
id | firstName | lastName | yearLevel | programid | programName | departmentId | departmentName
1  | John      | Doe      | 1    | 1 | BSIT        | 1            | CCS
```

---

## ?? **CONSOLE OUTPUT TO WATCH FOR**

When creating a student, you should see:

```
=== CreateStudentAsync (UserService) ===
Creating student: John Doe (john.doe@example.com)
  Program ID: 1, Department ID: 1

=== CREATING STUDENT: John Doe ===

Step 3: Parsing program and department IDs...
  Program: Parsed ID 1 from string
  Department: Parsed ID 1 from string
? Step 3 Complete: Program ID = 1, Department ID = 1

Step 4: Creating studentProfile record...
? Step 4 Complete: StudentProfile created with ID: 5
```

**If you see:**
```
Program: Parsed ID 1 from string  ? Good
Department: Parsed ID 1 from string  ? Good
```

**Then the data IS being saved!**

---

## ? **COMMON MISCONCEPTIONS**

### **WRONG: Creating Programs/Departments When Adding Students**

```
? Admin adds student with program "BSIT"
? System creates a new program record with name "BSIT"
? Result: Duplicate programs, inconsistent data
```

### **CORRECT: Pre-populated Reference Tables**

```
? Programs and departments pre-populated (one-time)
? Admin selects from existing programs/departments
? Student is linked to existing program by ID
? Result: Consistent, normalized data
```

---

## ?? **TROUBLESHOOTING**

### **Issue: Dropdown Shows No Options**

**Cause:** Programs/departments tables are empty

**Solution:** Run the pre-population SQL script

### **Issue: Console Shows "Program: Not found"**

**Cause:** Program ID doesn't exist in `programs` table

**Check:**
```sql
SELECT * FROM programs WHERE id = 1;
```

**Fix:** Run pre-population script

### **Issue: Foreign Key Constraint Error**

**Error:**
```
insert or update on table "studentProfile" violates foreign key constraint
```

**Cause:** `programId` or `departmentId` doesn't exist in reference tables

**Fix:**
1. Verify programs/departments exist
2. Use correct IDs when creating student

### **Issue: studentProfile Has NULL programid**

**Cause:** Column type mismatch or parsing failure

**Check Console:**
```
Program: Parsed ID 1 from string  ? Should see this
```

**If you see:**
```
Program: Not provided (null)  ? Problem!
```

**Fix:** Check that `UserService` is passing `ProgramId` correctly

---

## ?? **VERIFICATION QUERIES**

### **Check Programs and Departments Exist:**

```sql
-- Should return at least 5 departments
SELECT COUNT(*) as department_count FROM departments;

-- Should return at least 15 programs
SELECT COUNT(*) as program_count FROM programs;
```

### **Check Student Has Correct IDs:**

```sql
SELECT 
    sp.id,
    sp."studentId",
    sp.programid,
    sp."departmentId",
    p."programName",
    d."departmentName"
FROM "studentProfile" sp
LEFT JOIN programs p ON sp.programid = p.id
LEFT JOIN departments d ON sp."departmentId" = d.id
ORDER BY sp.id DESC
LIMIT 5;
```

### **Expected Output:**

```
id | studentId | programid | departmentId | programName | departmentName
5  | abc123... | 1         | 1     | BSIT        | CCS
```

**If you see NULL for programName or departmentName:**
- IDs don't match
- Programs/departments not pre-populated

---

## ?? **SUMMARY**

### **The Problem:**
You expected programs/departments to be created when adding students, but they're reference tables that must be pre-populated.

### **The Solution:**
1. ? Run SQL script to pre-populate `programs` and `departments` tables
2. ? Fix `studentProfile` column types (text ? integer)
3. ? Add foreign key constraints
4. ? Test adding a student
5. ? Verify data with SQL queries

### **Result:**
? Programs and departments are pre-populated  
? Students are linked to existing programs/departments by ID  
? Data integrity is maintained  
? Admin can select from existing options

---

**Run the SQL script now and your program/department data will be properly set up!** ???
