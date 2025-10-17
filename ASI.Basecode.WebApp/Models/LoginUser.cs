using ASI.Basecode.Data.Models;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Lightweight container returned by authentication logic containing the login result and user data.
    /// </summary>
    public class LoginUser
    {
        /// <summary>
        /// Result of the login attempt (success/failed etc.).
        /// </summary>
        public LoginResult loginResult { get; set; }

        /// <summary>
        /// User data retrieved from the data store when authentication succeeds.
        /// </summary>
        public User userData { get; set; }

        public LoginUser()
        {
            loginResult = LoginResult.Failed;
            userData = new User();
        }
    }
}








