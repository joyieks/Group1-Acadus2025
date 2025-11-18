# 🔍 Services vs Repositories: REAL Examples From Your Code

You don't have DTOs yet - you're using **ViewModels as DTOs** (which is fine for now). Let me show you EXACTLY what goes where using your **StudentService** and **StudentCourseService**.

---

## 📊 The Three Levels Explained

### **LEVEL 1: REPOSITORY** = Database Worker
- **File Location:** `ASI.Basecode.Data/Repositories/StudentCourseRepository.cs`
- **Job:** Query Supabase, return Models
- **Returns:** Raw `Models` (CourseModel, ActivityModel, etc.)
- **Queries ALLOWED:** WHERE, SELECT, JOIN, ORDER BY
- **Business Logic:** ❌ NOT ALLOWED

### **LEVEL 2: SERVICE** = Business Logic Brain
- **File Location:** `ASI.Basecode.Services/Services/StudentService.cs` or `StudentCourseService.cs`
- **Job:** Call Repository, apply business rules, transform data
- **Returns:** Models or ViewModels
- **Validation:** ✅ YES
- **Transformation:** ✅ YES
- **Calculations:** ✅ YES
- **Repository Queries:** ✅ YES (call them!)

### **LEVEL 3: CONTROLLER** = Request Handler
- **File Location:** `ASI.Basecode.WebApp/Controllers/StudentController.cs`
- **Job:** Call Service, transform to ViewModel for View, handle HTTP
- **Calls:** ✅ Service (never Repository!)
- **Authentication:** ✅ Read claims here
- **Database Queries:** ❌ NEVER

---

## 🎯 Real Example 1: Getting Student Courses

### **CURRENT CORRECT PATTERN** ✅

#### **Repository: Just Query**
```csharp
// File: StudentCourseRepository.cs (Line 23-45)
public async Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId)
{
    if (string.IsNullOrWhiteSpace(studentId))
        return new List<CourseModel>();

    // DATABASE LEVEL: Query 1 - Get enrollments
    var enrollmentsResponse = await _supabaseClient
        .From<EnrollmentModel>()
        .Where(e => e.StudentId == studentId && e.Status == "Active")  // ← WHERE clause
        .Get();

    var enrolledCourseIds = enrollmentsResponse.Models.Select(e => e.CourseId).ToList();

    if (!enrolledCourseIds.Any())
        return new List<CourseModel>();

    // DATABASE LEVEL: Query 2 - Get all courses
    var allCoursesResponse = await _supabaseClient
        .From<CourseModel>()
        .Get();

    // DATABASE LEVEL: Filter in memory (small dataset)
    var enrolledCourses = allCoursesResponse.Models
        .Where(c => enrolledCourseIds.Contains(c.Id))
        .ToList();

    return enrolledCourses;  // ← Just raw data, no business logic
}

// ACCEPTABLE: Sorting by natural order (no business logic)
public async Task<List<ActivityModel>> GetActivitiesByCourseIdAsync(long courseId)
{
    var res = await _supabaseClient
        .From<ActivityModel>()
        .Where(a => a.CourseId == courseId)
        .Where(a => a.IsVisible == false)  // ← Database filter
        .Get();

    return res.Models
        .OrderBy(a => a.DueDate)  // ← Natural sort order (acceptable)
        .ToList();
}
```

**Repository Job:** 
- ✅ Execute queries at database level
- ✅ Filter WHERE clauses
- ✅ Sort by natural order
- ✅ Return raw Models

---

#### **Service: Add Business Logic**
```csharp
// File: StudentCourseService.cs (Line 24-30)
public async Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId)
{
    // BUSINESS LOGIC 1: Validate
    if (string.IsNullOrWhiteSpace(studentId))
        throw new System.ArgumentException("Invalid Student ID.");

    // CALL REPOSITORY: Get raw data
    var courses = await _studentCourseRepository.GetCoursesByStudentIdAsync(studentId);
    
    // BUSINESS LOGIC 2: Filter by business rules
    // Example: Only show courses from current semester
    var currentSemesterId = GetCurrentSemesterId();
    var filteredCourses = courses.Where(c => c.SemesterId == currentSemesterId).ToList();
    
    // BUSINESS LOGIC 3: Transform or validate
    var validCourses = filteredCourses
        .Where(c => c.IsActive)  // Only active
        .OrderBy(c => c.Name)    // Sort by business logic, not natural order
        .ToList();

    return validCourses ?? new List<CourseModel>();
}
```

**Service Job:**
- ✅ Validate input
- ✅ Call Repository
- ✅ Apply business rules (filtering)
- ✅ Transform results
- ✅ Return processed data

---

#### **Controller: Handle Request & Presentation**
```csharp
// File: StudentController.cs (Line 49-75)
[HttpGet]
public async Task<IActionResult> Courses()
{
    // CONTROLLER JOB 1: Get authenticated user
    var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(supabaseUserId))
        return Unauthorized();

    // CONTROLLER JOB 2: Get student ID from auth
    var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
    if (string.IsNullOrWhiteSpace(studentId))
        return Unauthorized();

    // CONTROLLER JOB 3: ✅ Call SERVICE (NOT Repository!)
    List<CourseModel> enrolledCourses = await _studentCourseService.GetCoursesByStudentAsync(studentId);

    if (enrolledCourses == null || !enrolledCourses.Any())
    {
        ViewData["Message"] = "No enrolled courses found.";
        return View(Array.Empty<CourseCardViewModel>());
    }

    // CONTROLLER JOB 4: Transform to ViewModel for View
    var courseViewModels = enrolledCourses.Select(c => new CourseCardViewModel
    {
        Id = c.Id,
        CourseCode = c.Code ?? "N/A",
        CourseTitle = c.Name ?? "Untitled Course",
        SemesterInfo = c.SemesterId.ToString(),
        CardColor = GetRandomCardColor()  // ← UI-only logic (colors don't exist in database)
    }).ToArray();

    return View(courseViewModels);
}
```

**Controller Job:**
- ✅ Read authentication claims
- ✅ Get user context
- ✅ Call Service
- ✅ Transform to ViewModel (add UI properties like CardColor)
- ✅ Return View

---

## 🎯 Real Example 2: Creating a Student (BETTER Pattern)

### **CURRENT PATTERN IN YOUR CODE** (StudentService.CreateStudentAsync)

This is more complex - shows **validation + multiple queries + transformations**

```csharp
// File: StudentService.cs (Line 148-320)
public async Task<bool> CreateStudentAsync(StudentViewModel model)
{
    try
    {
        var client = await GetSupabaseClientAsync();

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // SERVICE BUSINESS LOGIC: VALIDATION
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        // ✅ BUSINESS LOGIC 1: Verify email unique
        var existingUser = await GetStudentByEmailAsync(model.Email);
        if (existingUser != null)
            return false; // Email already used

        // ✅ BUSINESS LOGIC 2: Validate year level (1-4)
        if (model.YearLevel < 1 || model.YearLevel > 4)
            return false;

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // SERVICE TRANSFORMATION: Parse Program/Department Names → IDs
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        int? programId = null;
        int? departmentId = null;

        // ✅ TRANSFORMATION 1: Look up Program ID from name
        try
        {
            if (!string.IsNullOrEmpty(model.Program))
            {
                // Try to parse as integer first
                if (int.TryParse(model.Program, out int progId))
                {
                    programId = progId;
                    Console.WriteLine($"  Program: Parsed ID {programId} from string");
                }
                else
                {
                    // It's a name, look it up in database
                    var programQuery = await client.From<Program>()
                        .Where(x => x.ProgramName == model.Program)
                        .Get();
                    var programRecord = programQuery?.Models?.FirstOrDefault();
                    programId = programRecord?.Id;
                    Console.WriteLine($"  Program lookup: Found ID {programId}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Program lookup failed: {ex.Message}");
        }

        // ✅ TRANSFORMATION 2: Look up Department ID from name
        try
        {
            if (!string.IsNullOrEmpty(model.Department))
            {
                if (int.TryParse(model.Department, out int deptId))
                {
                    departmentId = deptId;
                }
                else
                {
                    var deptQuery = await client.From<Department>()
                        .Where(x => x.DepartmentName == model.Department)
                        .Get();
                    var deptRecord = deptQuery?.Models?.FirstOrDefault();
                    departmentId = deptRecord?.Id;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Department lookup failed: {ex.Message}");
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // DATABASE QUERIES: Create records
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        // Create auth user first
        var supabaseUserId = await _supabaseAuthService.RegisterAsync(model.Email);

        // Create user record
        var userRecord = new SupabaseUserNew
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            // ... all fields
        };

        var insertedUserResponse = await client.From<SupabaseUserNew>().Insert(userRecord);
        var insertedUser = insertedUserResponse.Model;

        // Create student profile with the IDs we looked up
        var studentProfile = new Student
        {
            StudentId = insertedUser.UserTypeId,
            YearLevel = model.YearLevel,
            ProgramId = programId,       // ← Used the looked-up ID
            DepartmentId = departmentId, // ← Used the looked-up ID
        };

        await client.From<Student>().Insert(studentProfile);

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating student: {ex.Message}");
        return false;
    }
}
```

**What This Service Does:**
1. ✅ **Validation:** Email unique, year level valid
2. ✅ **Transformation:** Program name → ID, Department name → ID
3. ✅ **Multiple Queries:** Auth, users table, student table
4. ✅ **Error Handling:** Try-catch blocks
5. ✅ **Returns:** Success/failure boolean

---

## 🎯 Real Example 3: Where Business Logic Goes vs Database Queries

### **DATABASE LEVEL** = Repository

What goes in Repository:
```csharp
// ✅ YES: Database queries
.Where(a => a.CourseId == courseId)
.Where(a => a.IsVisible == false)

// ✅ YES: Natural sorting
.OrderBy(a => a.DueDate)

// ✅ YES: Joins
.Select(e => e.CourseId)

// ✅ YES: Filtering by database columns
.Filter("activityId", Operator.In, activityIds.ToArray())
```

What does NOT go in Repository:
```csharp
// ❌ NO: Grade calculations
var percentage = (double)s.Score / activity.maxScore * 100;

// ❌ NO: Semester filtering logic
var currentSemester = GetCurrentSemester();

// ❌ NO: Lookups by name
var programRecord = programQuery?.Models?.FirstOrDefault();
programId = programRecord?.Id;

// ❌ NO: Transformations
MidtermGrade = ConvertPercentageToGPA(average)
```

### **SERVICE LEVEL** = Business Brain

Where business logic SHOULD go:

```csharp
// ✅ YES: Filtering by business rules
var filteredCourses = courses.Where(c => c.SemesterId == currentSemesterId).ToList();

// ✅ YES: Calculations
var percentage = (double)s.Score / activity.maxScore * 100;
var average = graded.Any() ? Math.Round(graded.Average(), 1) : 0;

// ✅ YES: Lookups with business meaning
programId = await LookupProgramByNameAsync(model.Program);
departmentId = await LookupDepartmentByNameAsync(model.Department);

// ✅ YES: Transformations
MidtermGrade = ConvertPercentageToGPA(average)

// ✅ YES: Validation logic
if (model.YearLevel < 1 || model.YearLevel > 4)
    return false;

// ✅ YES: Multiple repository calls for complex operations
var courses = await _repo.GetCoursesByStudentIdAsync(studentId);
var enrollments = await _repo.GetEnrollmentsAsync(studentId);
var activities = await _repo.GetActivitiesAsync(courseId);
// Then combine/filter them based on business logic
```

---

## 🔄 Data Flow in Your Actual Code

### **Scenario: Student Views Their Courses**

```
1. BROWSER
   ↓ GET /Student/Courses
   
2. CONTROLLER: StudentController.Courses()
   ├─ Read claims: supabaseUserId
   ├─ Call: GetStudentIdFromSupabaseIdAsync()
   └─ Call: _studentCourseService.GetCoursesByStudentAsync(studentId)
   ↓

3. SERVICE: StudentCourseService.GetCoursesByStudentAsync()
   ├─ Validate: studentId is not null
   ├─ Call: _studentCourseRepository.GetCoursesByStudentIdAsync(studentId)
   ├─ Filter: Only active courses
   ├─ Sort: By business rules
   └─ Return: List<CourseModel>
   ↓

4. REPOSITORY: StudentCourseRepository.GetCoursesByStudentIdAsync()
   ├─ Query 1: SELECT * FROM enrollments WHERE studentId = X AND status = 'Active'
   ├─ Query 2: SELECT * FROM courses
   ├─ Filter: Only courses in enrollment list
   └─ Return: List<CourseModel> (raw)
   ↓

5. Back to CONTROLLER
   ├─ Got List<CourseModel> from service
   ├─ Transform to: List<CourseCardViewModel>
   │   (adds CardColor which only UI cares about)
   └─ Return View(viewModels)
   ↓

6. VIEW displays course cards
```

---

## 📋 Quick Reference: What Goes Where

| Task | Repository | Service | Controller |
|------|-----------|---------|-----------|
| Query database | ✅ YES | ❌ NO* | ❌ NO |
| Validate input | ❌ NO | ✅ YES | ❌ NO |
| Calculate scores | ❌ NO | ✅ YES | ❌ NO |
| Filter by business rule | ❌ NO | ✅ YES | ❌ NO |
| Look up IDs by name | ❌ NO | ✅ YES | ❌ NO |
| Read authentication | ❌ NO | ❌ NO | ✅ YES |
| Transform to ViewModel | ❌ NO | ❌ NO | ✅ YES |
| Natural sorting | ✅ OK | ⚠️ OK | ❌ NO |
| Combine multiple queries | ❌ NO | ✅ YES | ❌ NO |

`*` Service can call Repository to query

---

## ⚠️ Common Mistakes to Avoid

### ❌ WRONG: Query in Controller
```csharp
public async Task<IActionResult> Courses()
{
    // ❌ DON'T query directly
    var courses = await _supabaseClient
        .From<CourseModel>()
        .Get();
    
    return View(courses);
}
```

### ✅ RIGHT: Query through Service → Repository
```csharp
public async Task<IActionResult> Courses()
{
    var courses = await _studentCourseService.GetCoursesByStudentAsync(studentId);
    var viewModels = courses.Select(c => new CourseCardViewModel { ... }).ToList();
    return View(viewModels);
}
```

---

## ✅ Summary Using Your Code

| Component | Location | Responsibility | Example |
|-----------|----------|-----------------|---------|
| **Repository** | `Data/Repositories/` | Raw Supabase queries | `GetCoursesByStudentIdAsync()` - just fetches data |
| **Service** | `Services/Services/` | Business logic, validation, transformation | `StudentService.CreateStudentAsync()` - validates, looks up IDs, creates records |
| **Controller** | `WebApp/Controllers/` | Handle requests, transform to ViewModel | `StudentController.Courses()` - calls service, adds UI properties |
| **ViewModel** | `Services/ServiceModels/` | **Your DTOs** (StudentViewModel, StudentCreateDto) | Contains data + validation attributes |

**In Your Code:**
- ✅ You HAVE services (StudentService, StudentCourseService)
- ✅ You HAVE repositories (StudentCourseRepository)
- ✅ You HAVE ViewModels (StudentViewModel acts as DTO)
- ✅ Controllers call services correctly

**What to improve:**
- Move grade calculations OUT of Repository
- Move student creation logic fully to Service
- Add more validation in Services
- Create helper methods in Services for lookups

Your architecture is actually pretty good! The main thing is ensuring business logic stays in Service, not Repository. 🎯
