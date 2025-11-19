-- =====================================================
-- COMPLETE DATABASE MIGRATION SCRIPT
-- Run this script to enable student activity submissions
-- =====================================================

-- =====================================================
-- PART 1: Add submissionContent field for student answers
-- =====================================================
ALTER TABLE public.activity_submission
ADD COLUMN IF NOT EXISTS "submissionContent" text;

-- Add a comment to document the field
COMMENT ON COLUMN public.activity_submission."submissionContent" IS 'The student''s submitted answer or work content for this activity';

-- =====================================================
-- PART 2: Fix activity visibility for ALL courses
-- =====================================================
-- In the database: isVisible = false means visible to students
--                   isVisible = true means hidden from students
-- This makes ALL activities visible to students

-- First, check how many activities are currently hidden
SELECT 
    COUNT(*) as hidden_activities_count,
    COUNT(DISTINCT "courseId") as affected_courses
FROM activities 
WHERE "isVisible" = true;

-- Show sample of hidden activities before fixing
SELECT 
    id, 
    "activityTitle", 
    "isVisible",
    "courseId"
FROM activities 
WHERE "isVisible" = true
ORDER BY "courseId", id
LIMIT 20;

-- Fix ALL activities: Make them visible to students
-- This sets isVisible = false (which means visible to students)
UPDATE activities 
SET "isVisible" = false,
    "invisible_at" = NULL
WHERE "isVisible" = true;

-- =====================================================
-- PART 3: Verification queries
-- =====================================================

-- Verify the submissionContent column was added
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns
WHERE table_name = 'activity_submission' 
  AND column_name = 'submissionContent';

-- Verify all activities are now visible
SELECT 
    COUNT(*) as total_activities,
    SUM(CASE WHEN "isVisible" = false THEN 1 ELSE 0 END) as visible_activities,
    SUM(CASE WHEN "isVisible" = true THEN 1 ELSE 0 END) as hidden_activities
FROM activities;

-- Show activities by course (to verify visibility)
SELECT 
    "courseId",
    COUNT(*) as total_activities,
    SUM(CASE WHEN "isVisible" = false THEN 1 ELSE 0 END) as visible_count,
    SUM(CASE WHEN "isVisible" = true THEN 1 ELSE 0 END) as hidden_count
FROM activities
GROUP BY "courseId"
ORDER BY "courseId";

-- =====================================================
-- SCRIPT COMPLETE
-- =====================================================
-- After running this script:
-- 1. All activities will be visible to students (isVisible = false)
-- 2. Students can submit their answers (submissionContent field exists)
-- 3. Rebuild your application and test the submission functionality
-- =====================================================

