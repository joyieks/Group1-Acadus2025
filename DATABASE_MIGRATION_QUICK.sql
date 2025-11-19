-- =====================================================
-- QUICK DATABASE MIGRATION SCRIPT
-- Run this for essential changes only
-- =====================================================

-- 1. Add submissionContent field for student answers
ALTER TABLE public.activity_submission
ADD COLUMN IF NOT EXISTS "submissionContent" text;

-- 2. Make ALL activities visible to students
-- (isVisible = false means visible, isVisible = true means hidden)
UPDATE activities 
SET "isVisible" = false,
    "invisible_at" = NULL
WHERE "isVisible" = true;

-- Done! ✅


