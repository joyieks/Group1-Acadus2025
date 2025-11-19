-- Migration: Fix visibility for ALL activities across ALL courses
-- This makes all activities visible to students (sets isVisible = false)
-- In the database: isVisible = false means visible to students, isVisible = true means hidden

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

-- Verify the fix
SELECT 
    COUNT(*) as total_activities,
    SUM(CASE WHEN "isVisible" = false THEN 1 ELSE 0 END) as visible_activities,
    SUM(CASE WHEN "isVisible" = true THEN 1 ELSE 0 END) as hidden_activities
FROM activities;

