using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Web.Infrastructure.FormsAuthenticationService
{
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        protected const string UserSignInTicksCookieKey = "SessInit";

        //this id is just unique to the user not globally
        private string SetCurrentUserSignInTicks()
        {
            //use Ticks to create an id unique to the user.
            string sessionVersion = DateTime.Now.Ticks.ToString();
            HttpContext.Current.Response.Cookies[UserSignInTicksCookieKey].Value = sessionVersion;
            HttpContext.Current.Response.Cookies[UserSignInTicksCookieKey].Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies[UserSignInTicksCookieKey].HttpOnly = true;
            return sessionVersion;
        }

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

        /// <summary>
        /// This key is only gaurnteed to be unique to the user's last BeginSession call. It could be the same for multiple users.
        /// It is mainly used to expire Cached items when the user re-logins.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentUserSignInTicks()
        {
            if (HttpContext.Current.Request.Cookies[UserSignInTicksCookieKey] != null)
            {
                return HttpContext.Current.Request.Cookies[UserSignInTicksCookieKey].Value;
            }
            else
            {
                //set a new cookie and return the value.
                return SetCurrentUserSignInTicks();
            }
        }

        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);

            //store a cookie for the user that indicates their login time.
            SetCurrentUserSignInTicks();
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();

            //remove all cookies.

        }
    }
}