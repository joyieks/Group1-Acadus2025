-- =====================================================
-- Audit Logs Table for Tracking Teacher Activities
-- =====================================================
-- This table tracks all activities performed by teachers
-- such as creating activities, grading students, adding students, etc.
-- Can also be used by admin in the future
-- =====================================================

-- Create audit_logs table
CREATE TABLE IF NOT EXISTS public.audit_logs (
    id BIGSERIAL PRIMARY KEY,
    
    -- Who performed the action
    "userId" TEXT NOT NULL,  -- User's userTypeId (UUID from Supabase Auth)
    "userRole" TEXT NOT NULL,  -- Role: 'Teacher', 'Admin', etc.
    "userName" TEXT,  -- User's full name for display (e.g., "John Doe")
    
    -- What action was performed
    "actionType" TEXT NOT NULL,  -- e.g., 'CREATE_ACTIVITY', 'GRADE_STUDENT', 'ADD_STUDENT', 'REMOVE_STUDENT', 'UPDATE_ACTIVITY', 'ARCHIVE_ACTIVITY'
    "actionDescription" TEXT NOT NULL,  -- Human-readable description (e.g., "Created activity 'Midterm Exam'")
    
    -- Related entities (nullable, depends on action type)
    "courseId" BIGINT,  -- Related course ID
    "courseCode" TEXT,  -- Course code for display (e.g., "141001")
    "courseName" TEXT,  -- Course name for display
    
    "studentId" TEXT,  -- Related student's userTypeId (UUID)
    "studentName" TEXT,  -- Student's full name for display
    
    "activityId" INTEGER,  -- Related activity ID
    "activityTitle" TEXT,  -- Activity title for display
    
    -- Additional details
    "details" JSONB,  -- Additional structured data (e.g., old grade, new grade, etc.)
    "metadata" TEXT,  -- Additional free-form metadata if needed
    
    -- Timestamps
    "created_at" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Indexes for better query performance
    CONSTRAINT audit_logs_userId_fkey FOREIGN KEY ("userId") REFERENCES public.users("userTypeId") ON DELETE SET NULL,
    CONSTRAINT audit_logs_courseId_fkey FOREIGN KEY ("courseId") REFERENCES public.courses(id) ON DELETE SET NULL
);

-- Create indexes for faster queries
CREATE INDEX IF NOT EXISTS idx_audit_logs_userId ON public.audit_logs("userId");
CREATE INDEX IF NOT EXISTS idx_audit_logs_actionType ON public.audit_logs("actionType");
CREATE INDEX IF NOT EXISTS idx_audit_logs_courseId ON public.audit_logs("courseId");
CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at ON public.audit_logs("created_at" DESC);
CREATE INDEX IF NOT EXISTS idx_audit_logs_userRole ON public.audit_logs("userRole");

-- Add comment to table
COMMENT ON TABLE public.audit_logs IS 'Tracks all activities performed by teachers and admins for audit and recent activities display';

-- Add comments to columns
COMMENT ON COLUMN public.audit_logs."actionType" IS 'Type of action: CREATE_ACTIVITY, GRADE_STUDENT, ADD_STUDENT, REMOVE_STUDENT, UPDATE_ACTIVITY, ARCHIVE_ACTIVITY, etc.';
COMMENT ON COLUMN public.audit_logs."details" IS 'JSON object containing additional structured data about the action (e.g., {"oldScore": 85, "newScore": 90})';

-- =====================================================
-- Example Usage:
-- =====================================================
-- 
-- 1. When a teacher creates an activity:
-- INSERT INTO public.audit_logs ("userId", "userRole", "userName", "actionType", "actionDescription", "courseId", "courseCode", "courseName", "activityId", "activityTitle")
-- VALUES ('teacher-uuid', 'Teacher', 'John Doe', 'CREATE_ACTIVITY', 'Created activity ''Midterm Exam''', 13, '141001', 'Introduction to Programming', 45, 'Midterm Exam');
--
-- 2. When a teacher grades a student:
-- INSERT INTO public.audit_logs ("userId", "userRole", "userName", "actionType", "actionDescription", "courseId", "courseCode", "courseName", "studentId", "studentName", "activityId", "activityTitle", "details")
-- VALUES ('teacher-uuid', 'Teacher', 'John Doe', 'GRADE_STUDENT', 'Graded student ''Jane Smith'' for activity ''Midterm Exam''', 13, '141001', 'Introduction to Programming', 'student-uuid', 'Jane Smith', 45, 'Midterm Exam', '{"score": 90, "maxScore": 100}'::jsonb);
--
-- 3. When a teacher adds a student to a course:
-- INSERT INTO public.audit_logs ("userId", "userRole", "userName", "actionType", "actionDescription", "courseId", "courseCode", "courseName", "studentId", "studentName")
-- VALUES ('teacher-uuid', 'Teacher', 'John Doe', 'ADD_STUDENT', 'Added student ''Jane Smith'' to course', 13, '141001', 'Introduction to Programming', 'student-uuid', 'Jane Smith');
--
-- 4. When a teacher removes a student from a course:
-- INSERT INTO public.audit_logs ("userId", "userRole", "userName", "actionType", "actionDescription", "courseId", "courseCode", "courseName", "studentId", "studentName")
-- VALUES ('teacher-uuid', 'Teacher', 'John Doe', 'REMOVE_STUDENT', 'Removed student ''Jane Smith'' from course', 13, '141001', 'Introduction to Programming', 'student-uuid', 'Jane Smith');
--
-- 5. When a teacher archives an activity:
-- INSERT INTO public.audit_logs ("userId", "userRole", "userName", "actionType", "actionDescription", "courseId", "courseCode", "courseName", "activityId", "activityTitle")
-- VALUES ('teacher-uuid', 'Teacher', 'John Doe', 'ARCHIVE_ACTIVITY', 'Archived activity ''Midterm Exam''', 13, '141001', 'Introduction to Programming', 45, 'Midterm Exam');
--
-- =====================================================
-- Query Examples for Recent Activities:
-- =====================================================
--
-- Get recent activities for a specific teacher:
-- SELECT * FROM public.audit_logs 
-- WHERE "userId" = 'teacher-uuid' 
-- ORDER BY "created_at" DESC 
-- LIMIT 10;
--
-- Get recent activities for a specific course:
-- SELECT * FROM public.audit_logs 
-- WHERE "courseId" = 13 
-- ORDER BY "created_at" DESC 
-- LIMIT 10;
--
-- Get recent activities by action type:
-- SELECT * FROM public.audit_logs 
-- WHERE "actionType" = 'GRADE_STUDENT' 
-- ORDER BY "created_at" DESC 
-- LIMIT 10;
--
-- =====================================================






