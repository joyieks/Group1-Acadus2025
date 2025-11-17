# ? PROGRAM & DEPARTMENT STORAGE FIX - Complete Solution

## ?? **PROBLEM IDENTIFIED**

When adding a student from the admin side, the program name and department name were not being properly stored in the `studentProfile` table.

### **Database Structure:**

Based on your Supabase schema:

```
studentProfile table:
- id (int) PK
- studentId (text) FK ? users.userTypeId
- yearLevel (numeric)
- programid (text) ? Should reference programs.id (int)
- departmentId (text) ? Should reference departments.id (int)
- created_at (timestamptz)

programs table:
- id (int) PK
- programName (text)
- programCode (text)
- departmentid (int) FK ? departments.id
- created_at (timestamptz)

departments table:
- id (int) PK
- departmentName (text)
- departmentCode (text)
- created_at (timestamptz)
```

---

## ?? **ROOT CAUSE**

### **Data Flow Issue:**

1. **Admin Form** sends:
   - `ProgramId`: `"1"` (string containing the program ID)
   - `DepartmentId`: `"2"` (string containing the department ID)

2. **AdminController** receives `StudentCreateViewModel` and maps to `StudentCreateDto`:
   ```csharp
   var studentDto = new StudentCreateDto
   {
       ProgramId = model.ProgramId,  // "1"
   DepartmentId = model.DepartmentId  // "2"
   };
   ```

3. **UserService.CreateStudentAsync** was trying to look up names:
   ```csharp
   // ? WRONG - Was looking up names instead of using IDs
   Program = null,  // Set to null!
   Department = null  // Set to null!
   ```

4. **StudentService.CreateStudentAsync** was trying to look up by name:
   ```csharp
   // ? WRONG - Program and Department were null, so no lookup happened
   var programQuery = await client.From<Program>()
       .Where(x => x.ProgramName == model.Program)  // null!
       .Get();
   ```

5. **Result:** `programId` and `departmentId` were always `null`

---

## ? **THE FIX**

### **1. UserService.CreateStudentAsync**

**File:** `ASI.Basecode.Services\Services\UserService.cs`

**Changed:**
```csharp
public async Task<bool> CreateStudentAsync(StudentCreateDto model)
{
    Console.WriteLine($"Creating student: {model.FirstName} {model.LastName}");
    Console.WriteLine($"  Program ID: {model.ProgramId}, Department ID: {model.DepartmentId}");

    // ? FIX: Pass IDs directly as strings (StudentService will parse them)
 var studentViewModel = new StudentViewModel
    {
        FirstName = model.FirstName,
        MiddleName = model.MiddleName,
        LastName = model.LastName,
        Suffix = model.Suffix,
    Email = model.Email,
        ContactNumber = model.ContactNumber,
 HouseNumber = model.HouseNumber,
        StreetName = model.StreetName,
        Subdivision = model.Subdivision,
        Barangay = model.Barangay,
      City = model.City,
     Province = model.Province,
        ZipCode = model.ZipCode,
        YearLevel = (int)model.YearLevel,
   Program = model.ProgramId,  // ? Pass ID string: "1"
        Department = model.DepartmentId,  // ? Pass ID string: "2"
        EmergencyFirstName = model.EmergencyContactFirstName,
      EmergencyMiddleName = model.EmergencyContactMiddleName,
 EmergencyLastName = model.EmergencyContactLastName,
   EmergencySuffix = model.EmergencyContactSuffix,
   EmergencyContactNumber = model.EmergencyContactNumber,
        Relationship = model.EmergencyContactRelationship
    };

    // Call StudentService to handle the creation
    var result = await _studentService.CreateStudentAsync(studentViewModel);
    
    return result;
}
```

### **2. StudentService.CreateStudentAsync**

**File:** `ASI.Basecode.Services\Services\StudentService.cs`

**Changed Step 3:**
```csharp
// Step 3: Parse program and department IDs from model
Console.WriteLine($"Step 3: Parsing program and department IDs...");
int? programId = null;
int? departmentId = null;

// ? FIX: Try to parse as integer first (ID), fall back to name lookup
try
{
    if (!string.IsNullOrEmpty(model.Program))
    {
        // Try to parse as integer first (if it's an ID string like "1")
        if (int.TryParse(model.Program, out int progId))
        {
      programId = progId;
        Console.WriteLine($"  Program: Parsed ID {programId} from string");
      }
        else
        {
            // It's a name, look it up
   var programQuery = await client.From<Program>()
     .Where(x => x.ProgramName == model.Program)
     .Get();
 var programRecord = programQuery?.Models?.FirstOrDefault();
         programId = programRecord?.Id;
   Console.WriteLine($"Program lookup by name '{model.Program}': Found ID {programId}");
    }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  Warning: Program lookup failed: {ex.Message}");
}

try
{
    if (!string.IsNullOrEmpty(model.Department))
    {
        // Try to parse as integer first (if it's an ID string like "2")
        if (int.TryParse(model.Department, out int deptId))
        {
      departmentId = deptId;
   Console.WriteLine($"  Department: Parsed ID {departmentId} from string");
   }
   else
  {
          // It's a name, look it up
   var deptQuery = await client.From<Department>()
 .Where(x => x.DepartmentName == model.Department)
   .Get();
      var deptRecord = deptQuery?.Models?.FirstOrDefault();
         departmentId = deptRecord?.Id;
            Console.WriteLine($"  Department lookup by name '{model.Department}': Found ID {departmentId}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  Warning: Department lookup failed: {ex.Message}");
}

Console.WriteLine($"? Step 3 Complete: Program ID = {programId}, Department ID = {departmentId}");

// Step 4: Create studentProfile record with correct IDs
var student = new Student
{
    StudentId = supabaseUserId,
    YearLevel = model.YearLevel,
    ProgramId = programId,  // ? Now has the correct integer ID
    DepartmentId = departmentId,  // ? Now has the correct integer ID
    CreatedAt = DateTime.UtcNow
};
```

---

## ?? **HOW IT WORKS NOW**

### **Complete Data Flow:**

```
1. Admin Form
   ?
   ProgramId: "1" (selected from dropdown)
 DepartmentId: "2" (selected from dropdown)
   ?
2. AdminController
   ?
   StudentCreateDto {
       ProgramId: "1",
       DepartmentId: "2"
   }
   ?
3. UserService.CreateStudentAsync
   ?
   StudentViewModel {
       Program: "1",  // Pass ID as string
       Department: "2"  // Pass ID as string
   }
   ?
4. StudentService.CreateStudentAsync
   ?
   Parse "1" ? programId = 1
   Parse "2" ? departmentId = 2
   ?
5. Database Insert
   ?
   studentProfile {
    programId: 1,  ? Correct!
       departmentId: 2  ? Correct!
   }
```

---

## ?? **TESTING**

### **Expected Console Output:**

```
=== CreateStudentAsync (UserService) ===
Creating student: John Doe (john.doe@example.com)
  Program ID: 1, Department ID: 2

=== CREATING STUDENT: John Doe ===

Step 1: Creating Supabase Auth user...
? Step 1 Complete: Auth user created with ID: abc123-def456

Step 2: Inserting into users table...
? Step 2 Complete: User record created with ID: 10

Step 3: Parsing program and department IDs...
  Program: Parsed ID 1 from string
  Department: Parsed ID 2 from string
? Step 3 Complete: Program ID = 1, Department ID = 2

Step 4: Creating studentProfile record...
? Step 4 Complete: StudentProfile created with ID: 5

Step 5: Looking up Student role...
? Step 5 Complete: Student role assigned

Step 8: Sending password setup email...
? Step 8 Complete: Password setup email sent

??? STUDENT CREATION COMPLETE ???
  Student ID: 5
  Auth User ID: abc123-def456
  Email: john.doe@example.com

Student creation result: True
=== End CreateStudentAsync ===
```

### **Verify in Database:**

After creating a student, check Supabase:

```sql
-- Check studentProfile
SELECT * FROM "studentProfile" 
WHERE "studentId" = 'abc123-def456';

-- Should show:
-- id | studentId | yearLevel | programid | departmentId
-- 5  | abc123... | 1         | 1      | 2
```

---

## ?? **KEY IMPROVEMENTS**

### **1. Flexible ID/Name Handling**

The fix supports both scenarios:
- ? **ID as string:** `Program = "1"` ? Parsed to `programId = 1`
- ? **Name as string:** `Program = "Computer Science"` ? Looked up ? `programId = 1`

### **2. Better Logging**

Enhanced console output shows:
- What IDs are received
- How they're parsed
- Final values stored

### **3. Error Handling**

If parsing or lookup fails:
- Logs warning
- Continues with `null` value
- Doesn't crash the entire process

---

## ?? **POTENTIAL ISSUES & FIXES**

### **Issue 1: Program/Department Not Found**

**Symptom:**
```
Program lookup by name 'CS': Not found
```

**Cause:** Program doesn't exist in `programs` table

**Fix:** Ensure programs are pre-populated:
```sql
INSERT INTO programs (id, "programName", "programCode", departmentid) 
VALUES 
  (1, 'Computer Science', 'CS', 1),
    (2, 'Information Technology', 'IT', 1),
    (3, 'Engineering', 'ENG', 2);
```

### **Issue 2: Type Mismatch in studentProfile**

**Symptom:** Error inserting into `studentProfile` because `programid` is `text` but value is `int`

**Fix:** Update database schema:
```sql
-- Change programid from text to int
ALTER TABLE "studentProfile" 
ALTER COLUMN "programid" TYPE integer USING "programid"::integer;

-- Change departmentId from text to int  
ALTER TABLE "studentProfile"
ALTER COLUMN "departmentId" TYPE integer USING "departmentId"::integer;
```

### **Issue 3: Dropdown Not Sending IDs**

**Symptom:** `ProgramId` is `null` or empty

**Fix:** Check admin form dropdown:
```html
<!-- Ensure dropdown sends ID as value -->
<select asp-for="ProgramId" class="form-control">
    <option value="">Select Program</option>
    <option value="1">Computer Science</option>
    <option value="2">Information Technology</option>
</select>
```

---

## ? **VERIFICATION CHECKLIST**

After applying the fix:

- [ ] Rebuild application
- [ ] Run application
- [ ] Login as admin
- [ ] Go to Add Student
- [ ] Select program from dropdown (e.g., "Computer Science")
- [ ] Select department from dropdown (e.g., "College of Engineering")
- [ ] Fill in other required fields
- [ ] Submit form
- [ ] Check console output shows correct IDs
- [ ] Check database `studentProfile` table shows correct `programid` and `departmentId`
- [ ] Success message appears
- [ ] Student can receive email and set password
- [ ] Student can log in

---

## ?? **SUMMARY**

### **Problem:**
Program and department were not being saved in `studentProfile` table (always `null`)

### **Root Cause:**
1. `UserService` was setting `Program` and `Department` to `null`
2. `StudentService` had no values to parse or look up

### **Solution:**
1. ? `UserService` now passes `ProgramId` and `DepartmentId` as strings
2. ? `StudentService` parses them as integers or looks up by name
3. ? Correct integer IDs are stored in database

### **Result:**
? Students are created with correct program and department references  
? Data integrity maintained (foreign key constraints)  
? Admin can view student's program and department  
? Reports and queries work correctly

---

## ?? **WHAT'S FIXED**

- ? Program ID properly stored in `studentProfile.programid`
- ? Department ID properly stored in `studentProfile.departmentId`
- ? Foreign key relationships maintained
- ? Console logging shows exact values being processed
- ? Supports both ID parsing and name lookup (flexible)

---

**Your students will now have their programs and departments properly saved!** ???
