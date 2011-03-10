using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Infrastructure.Session
{
    public class WebUserSession : IUserSession
    {

        public long GetCurrentUserId()
        {
            long userId;
            if (long.TryParse(GetCurrentUserName(), out userId))
            {
                return userId;
            }
            else
            {
                return -1;
            }
        }

        public string GetCurrentUserName()
        {
            string userName = "";
            if (HttpContext.Current != null)
                userName = HttpContext.Current.User.Identity.Name;
            else
                userName = Environment.UserName;

            return userName;
        }

        public void Logout()
        {
            //clear session variables.
            HttpContext.Current.Session.RemoveAll();
            //signout
            System.Web.Security.FormsAuthentication.SignOut();
        }
    }
    
   
}