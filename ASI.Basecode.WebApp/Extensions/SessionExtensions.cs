using Microsoft.AspNetCore.Http;
using System;

namespace ASI.Basecode.WebApp.Extensions
{
    /// <summary>
    /// Extension methods for ISession to simplify working with session data.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Set a string value in the session.
        /// </summary>
        public static void SetString(this ISession session, string key, string value)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            
            session.SetString(key, value ?? string.Empty);
        }

        /// <summary>
        /// Get a string value from the session.
        /// </summary>
        public static string GetString(this ISession session, string key)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            
            if (session.TryGetValue(key, out byte[] value))
            {
                return System.Text.Encoding.UTF8.GetString(value);
            }

            return null;
        }
    }
}
