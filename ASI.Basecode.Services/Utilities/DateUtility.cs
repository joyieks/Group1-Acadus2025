using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Utilities
{
    /// <summary>
    /// Utility class for date and week-related calculations.
    /// </summary>
    public static class DateUtility
    {
        /// <summary>
        /// Gets the start date (Monday) of the current week.
        /// </summary>
        /// <returns>DateTime representing Monday of the current week.</returns>
        public static DateTime GetWeekStartDate()
        {
            return GetWeekStartDate(DateTime.Now);
        }

        /// <summary>
        /// Gets the start date (Monday) of the week containing the specified date.
        /// </summary>
        /// <param name="date">The date to calculate the week for.</param>
        /// <returns>DateTime representing Monday of the week containing the date.</returns>
        public static DateTime GetWeekStartDate(DateTime date)
        {
            // Get the current day of week (0 = Sunday, 1 = Monday, etc.)
            int dayOfWeek = (int)date.DayOfWeek;
            
            // Calculate days to subtract to get to Monday (1)
            int daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            
            DateTime monday = date.AddDays(-daysToSubtract);
            return monday.Date;  // Return with time set to 00:00:00
        }

        /// <summary>
        /// Gets the end date (Sunday) of the current week.
        /// </summary>
        /// <returns>DateTime representing Sunday of the current week.</returns>
        public static DateTime GetWeekEndDate()
        {
            return GetWeekEndDate(DateTime.Now);
        }

        /// <summary>
        /// Gets the end date (Sunday) of the week containing the specified date.
        /// </summary>
        /// <param name="date">The date to calculate the week for.</param>
        /// <returns>DateTime representing Sunday of the week containing the date (23:59:59).</returns>
        public static DateTime GetWeekEndDate(DateTime date)
        {
            DateTime monday = GetWeekStartDate(date);
            DateTime sunday = monday.AddDays(6);
            // Set time to end of day
            return sunday.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        /// <summary>
        /// Gets a formatted string representing the current week (e.g., "Nov 11 - Nov 17, 2025").
        /// </summary>
        /// <returns>Formatted week string.</returns>
        public static string GetWeekDisplayText()
        {
            return GetWeekDisplayText(DateTime.Now);
        }

        /// <summary>
        /// Gets a formatted string representing the week (e.g., "Nov 11 - Nov 17, 2025").
        /// </summary>
        /// <param name="date">The date to calculate the week for.</param>
        /// <returns>Formatted week string.</returns>
        public static string GetWeekDisplayText(DateTime date)
        {
            DateTime startDate = GetWeekStartDate(date);
            DateTime endDate = GetWeekEndDate(date);

            // Format: "MMM DD - MMM DD, YYYY" or "MMM DD - DD, YYYY" if same month
            if (startDate.Month == endDate.Month)
            {
                return $"{startDate:MMM dd} - {endDate:dd, yyyy}";
            }
            else
            {
                return $"{startDate:MMM dd} - {endDate:MMM dd, yyyy}";
            }
        }
    }
}
