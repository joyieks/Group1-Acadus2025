using ASI.Basecode.Data.Models;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Models
{
    public class LoginUser
    {
        public LoginResult loginResult { get; set; }
        public User userData { get; set; }

        public LoginUser()
        {
            loginResult = LoginResult.Failed;
            userData = new User();
        }
    }
}




