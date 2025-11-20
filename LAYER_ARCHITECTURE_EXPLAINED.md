# 🏗️ Layer Architecture: Repositories vs Services vs Controllers vs Models

## The Confusion Explained

You're right to be confused! **BOTH Repository and Service do queries**, but they serve **DIFFERENT purposes** at **DIFFERENT levels**.

---

## 📊 Visual Flow

```
REQUEST FROM BROWSER
        ↓
   CONTROLLER (ASI.Basecode.WebApp/Controllers)
   └─ "Hey Service, get me student courses"
        ↓
   SERVICE (ASI.Basecode.Services/Services)
   └─ "Apply business logic, validation, transformation"
   └─ "Hey Repository, fetch this data"
        ↓
   REPOSITORY (ASI.Basecode.Data/Repositories)
   └─ "Query the database using Supabase client"
        ↓
   MODELS (ASI.Basecode.Data/Models)
   └─ "Here's the raw data from Supabase"
        ↓
   SERVICE (Transform Models → DTOs)
   └─ "Here's the processed data"
        ↓
   CONTROLLER (Transform DTOs → ViewModels)
   └─ "Here's the presentation data"
        ↓
   VIEW (ASI.Basecode.WebApp/Views)
   └─ Display to user
```

---

## 🔍 Example From Your Code

### Step 1: CONTROLLER Receives Request
**File:** `StudentController.cs` (Line 51-65)

```csharp
[HttpGet]
public async Task<IActionResult> Courses()
{
    var supabaseUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);

    // ❌ WRONG WAY: Call Repository directly
    // var courses = await _studentCourseRepository.GetCoursesByStudentIdAsync(studentId);

    // ✅ RIGHT WAY: Call Service
    List<CourseModel> enrolledCourses = await _studentCourseService.GetCoursesByStudentAsync(studentId);
    
    // Transform to ViewModel
    var courseViewModels = enrolledCourses.Select(c => new CourseCardViewModel { ... }).ToArray();
    
    return View(courseViewModels);
}
```

**Controller's Job:**
- ✅ Gets user from context
- ✅ Calls SERVICE (not Repository!)
- ✅ Transforms result to ViewModel
- ✅ Returns View

---

### Step 2: SERVICE Does Business Logic & Calls Repository
**File:** `StudentCourseService.cs`

```csharp
public class StudentCourseService : IStudentCourseService
{
    private readonly IStudentCourseRepository _repository;
    
    public async Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId)
    {
        // BUSINESS LOGIC 1: Validate student exists
        if (string.IsNullOrWhiteSpace(studentId))
            return new List<CourseModel>();
        
        // BUSINESS LOGIC 2: Fetch from repository
        var courses = await _repository.GetCoursesByStudentIdAsync(studentId);
        
        // BUSINESS LOGIC 3: Filter/transform result
        var activeCourses = courses.Where(c => c.IsActive).ToList();
        
        // BUSINESS LOGIC 4: Apply business rules
        // e.g., "only show courses from current semester"
        var currentSemester = GetCurrentSemester();
        var filteredCourses = activeCourses.Where(c => c.SemesterId == currentSemester.Id).ToList();
        
        return filteredCourses;
    }
}
```

**Service's Job:**
- ✅ Apply BUSINESS RULES & VALIDATION
- ✅ Call Repository to fetch data
- ✅ Transform/filter results
- ✅ Handle errors & exceptions
- ❌ NOT return ViewModels (return Models/DTOs)

---

### Step 3: REPOSITORY Executes Database Query
**File:** `StudentCourseRepository.cs` (Line 23-45)

```csharp
public class StudentCourseRepository : IStudentCourseRepository
{
    private readonly Client _supabaseClient;
    
    public async Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId)
    {
        // This is the ONLY place that talks to database
        
        // QUERY 1: Get enrollments
        var enrollmentsResponse = await _supabaseClient
            .From<EnrollmentModel>()
            .Where(e => e.StudentId == studentId && e.Status == "Active")
            .Get();
        
        // QUERY 2: Get courses
        var allCoursesResponse = await _supabaseClient
            .From<CourseModel>()
            .Get();
        
        // JOIN in memory
        var enrolledCourses = allCoursesResponse.Models
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .ToList();
        
        return enrolledCourses; // Raw data, no business logic
    }
}
```

**Repository's Job:**
- ✅ Query database using Supabase client
- ✅ Transform database rows to Models
- ✅ Return raw data (Models)
- ❌ NO business logic
- ❌ NO validation
- ❌ NO filtering

---

## 📝 Models: Data Containers

### Data Layer Model (Raw Database)
**File:** `ASI.Basecode.Data/Models/CourseModel.cs`

```csharp
public class CourseModel
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Credits { get; set; }
    public long SemesterId { get; set; }
    public bool IsActive { get; set; }
}
```

**Purpose:** Maps Supabase table columns to C# properties

### Service Layer DTO (Business Data)
**File:** `ASI.Basecode.Services/ServiceModels/CourseDto.cs`

```csharp
public class CourseDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Credits { get; set; }
    // ⚠️ Simplified - only what services need
}
```

**Purpose:** Service passes DTOs to Controller (not Models)

### WebApp Layer ViewModel (UI Presentation)
**File:** `ASI.Basecode.WebApp/Models/CourseCardViewModel.cs`

```csharp
public class CourseCardViewModel
{
    public long Id { get; set; }
    public string CourseCode { get; set; }
    public string CourseTitle { get; set; }
    public string SemesterInfo { get; set; }
    public string CardColor { get; set; } // ← Only UI cares about color!
}
```

**Purpose:** Controller transforms Service data for View display

---

## 🎯 Separation of Concerns

| Layer | File Location | Responsibility | Example |
|-------|---------------|-----------------|---------|
| **Repository** | `Data/Repositories/*.cs` | **Raw database queries only** | "Get rows from `enrollments` table where status = 'Active'" |
| **Service** | `Services/Services/*.cs` | **Business logic & rules** | "Get student's courses, filter by current semester, check enrollment status" |
| **Controller** | `WebApp/Controllers/*.cs` | **Request handling & presentation** | "Get student ID from claims, call Service, map to ViewModel, return View" |
| **Model** | `Data/Models/*.cs` or `WebApp/Models/*.cs` | **Data containers** | Course with Id, Code, Name, etc. |

---

## ❌ WRONG Architecture (What You Had Before)

```csharp
// StudentController
public async Task<IActionResult> Courses()
{
    // ❌ Calling Repository directly!
    var courses = await _studentCourseRepository.GetCoursesByStudentIdAsync(studentId);
    
    // ❌ Putting Supabase queries in Controller!
    var enrollments = await _supabaseClient.From<EnrollmentModel>().Get();
    
    // ❌ Business logic scattered everywhere!
    var filtered = courses.Where(c => c.SemesterId == currentSemester).ToList();
    
    return View(filtered);
}
```

**Problems:**
- 🔴 Controller knows about database queries
- 🔴 No reusability (duplicate queries across controllers)
- 🔴 Hard to test (mock database calls in multiple places)
- 🔴 Business rules scattered (maintenance nightmare)

---

## ✅ CORRECT Architecture (What You Have Now)

```csharp
// StudentController
public async Task<IActionResult> Courses()
{
    var studentId = await GetStudentIdFromSupabaseIdAsync(supabaseUserId);
    
    // ✅ Call Service (black box)
    var courses = await _studentCourseService.GetCoursesByStudentAsync(studentId);
    
    // ✅ Transform to ViewModel
    var viewModels = courses.Select(c => new CourseCardViewModel { ... }).ToArray();
    
    return View(viewModels);
}

// StudentCourseService
public async Task<List<CourseModel>> GetCoursesByStudentAsync(string studentId)
{
    // ✅ Business logic: validate & filter
    if (string.IsNullOrWhiteSpace(studentId)) return new List<CourseModel>();
    
    // ✅ Call Repository (where database queries happen)
    var courses = await _repository.GetCoursesByStudentIdAsync(studentId);
    
    // ✅ Apply business rules
    return courses.Where(c => c.IsActive).ToList();
}

// StudentCourseRepository
public async Task<List<CourseModel>> GetCoursesByStudentIdAsync(string studentId)
{
    // ✅ ONLY database queries here
    var enrollments = await _supabaseClient
        .From<EnrollmentModel>()
        .Where(e => e.StudentId == studentId && e.Status == "Active")
        .Get();
    
    return enrollments.Models.ToList();
}
```

**Benefits:**
- 🟢 Controller doesn't know about Supabase
- 🟢 Service reusable across multiple controllers
- 🟢 Easy to test (mock Service in tests)
- 🟢 Business rules in one place (Service)
- 🟢 Database logic in one place (Repository)

---

## 🧪 Testing Example: Why Layers Matter

### Without Layers (Bad)
```csharp
// Test: Hard to mock database
var studentController = new StudentController(
    fakeAuthService,
    fakeSupabaseClient, // ← Hard to create fakes!
    realDatabase        // ← Can't avoid database!
);

var result = studentController.Courses();
// Test fails because database isn't connected
```

### With Layers (Good)
```csharp
// Test: Easy to mock Service
var mockService = new Mock<IStudentCourseService>();
mockService.Setup(s => s.GetCoursesByStudentAsync(It.IsAny<string>()))
    .ReturnsAsync(new List<CourseModel> { /* fake data */ });

var studentController = new StudentController(
    mockAuthService,
    mockService // ← Just mock the Service!
);

var result = studentController.Courses();
// Test passes without touching database
```

---

## 📚 Quick Reference: What Goes Where

### Repository Only
- SQL/Supabase queries
- Raw data fetching
- Database joins
- Entity mapping

### Service Only
- Business rules
- Validation
- Data transformation
- Multiple repository calls
- Error handling & logging
- **Authorization checks**

### Controller Only
- Request handling
- Authentication/Authorization context
- ViewModel mapping
- Redirect/Response handling
- HTTP status codes

### Never in Controller
- ❌ Direct database queries
- ❌ Business logic
- ❌ Repository calls

### Never in Service
- ❌ HTTP requests/responses
- ❌ ViewModels
- ❌ Authentication claims reading

### Never in Repository
- ❌ Business logic
- ❌ Validation rules
- ❌ Multiple table joins (do in Service)

---

## 🎓 Summary

| Question | Answer |
|----------|--------|
| **"Why do both Service and Repository query?"** | Repository queries database **raw**, Service adds **business logic** on top |
| **"What's the point of Service?"** | Reusable business logic, centralized validation, consistent filtering |
| **"Can I call Repository from Controller?"** | **NO** - always go through Service for consistency |
| **"Can I put business logic in Repository?"** | **NO** - keeps Repository simple & testable |
| **"Where does validation go?"** | **Service** - Repository just fetches |
| **"Where does filtering go?"** | **Service** - Repository returns raw data |
| **"Where do I read authentication claims?"** | **Controller** - pass student ID to Service |

---

## 🔗 Flow in Your App

```
1. Browser → StudentController.Courses()
2. StudentController calls → IStudentCourseService.GetCoursesByStudentAsync()
3. StudentCourseService applies business logic, then calls → IStudentCourseRepository.GetCoursesByStudentIdAsync()
4. StudentCourseRepository queries Supabase → Returns List<CourseModel>
5. StudentCourseService returns filtered List<CourseModel>
6. StudentController transforms → List<CourseCardViewModel>
7. StudentController returns View(viewModels)
8. View renders to HTML
9. Browser displays courses
```

Each layer has ONE job. This is **Single Responsibility Principle** (SOLID).
