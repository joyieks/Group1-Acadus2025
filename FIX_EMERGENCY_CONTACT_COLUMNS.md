# ?? Quick Fix - Emergency Contact Column Names

## ? **Issue Fixed**

**Error:**
```
Could not find the 'contact_number' column of 'emergencyContact' in the schema cache
```

## ?? **What Was Wrong**

Your `emergencyContact` table uses **camelCase** column names:
- `firstName`, `lastName`, `middleName`, `contactNumber`

But the C# `Contact` model was using **snake_case**:
- `first_name`, `last_name`, `middle_name`, `contact_number`

## ? **What Was Fixed**

### **Contact.cs Model - Updated to camelCase**

```csharp
// Before ?
[Column("first_name")]
[Column("last_name")]
[Column("middle_name")]
[Column("contact_number")]

// After ?
[Column("firstName")]
[Column("lastName")]
[Column("middleName")]
[Column("contactNumber")]
```

## ?? **Verify Your Database Schema**

Make sure your `emergencyContact` table has these columns (camelCase):

```sql
-- Run this in Supabase SQL Editor to check
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'emergencyContact'
ORDER BY ordinal_position;
```

**Expected columns:**
- `id`
- `firstName`
- `lastName`
- `middleName`
- `suffix`
- `contactNumber`
- `email`
- `created_at`

## ?? **Test Now**

1. **Restart your application**
2. **Try creating a student again**
3. Emergency contact should now save successfully! ??

---

## ?? **Important Note**

Different tables in your database use different naming conventions:

**camelCase (emergencyContact table):**
- `firstName`, `lastName`, `contactNumber`

**snake_case (addresses table):**
- `house_number`, `street_name`, `zip_code`

Make sure each C# model matches its corresponding table's column naming!

---

**Status: ? FIXED - Ready to Test**
