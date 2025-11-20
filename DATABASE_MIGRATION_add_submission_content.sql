-- Migration: Add submissionContent field to activity_submission table
-- This allows students to submit their answers/work

ALTER TABLE public.activity_submission
ADD COLUMN IF NOT EXISTS "submissionContent" text;

-- Add a comment to document the field
COMMENT ON COLUMN public.activity_submission."submissionContent" IS 'The student''s submitted answer or work content for this activity';
