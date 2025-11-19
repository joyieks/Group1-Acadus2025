-- =====================================================
-- VERIFY SUBMISSION CONTENT COLUMN EXISTS
-- Run this to check if the column exists and see sample data
-- =====================================================

-- Check if column exists
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'activity_submission' 
  AND column_name = 'submissionContent';

-- If the above returns no rows, the column doesn't exist yet
-- Run the migration: DATABASE_MIGRATION_QUICK.sql or DATABASE_MIGRATION_COMPLETE.sql

-- Check sample submissions with content
SELECT 
    id,
    "activityId",
    "studentId",
    "submissionStatus",
    LENGTH("submissionContent") as content_length,
    LEFT("submissionContent", 50) as content_preview
FROM activity_submission
WHERE "submissionContent" IS NOT NULL 
  AND "submissionContent" != ''
LIMIT 10;

-- Check all submissions (to see if any have content)
SELECT 
    COUNT(*) as total_submissions,
    COUNT("submissionContent") as submissions_with_content,
    COUNT(*) - COUNT("submissionContent") as submissions_without_content
FROM activity_submission;


