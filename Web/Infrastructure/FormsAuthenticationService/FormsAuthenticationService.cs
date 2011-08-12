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

        public bool IsAuthenticated()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated;
        }

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

        /// <summary>
        /// UserId is stored in ther User Data of the ticket. This allows our custom MembershipProvider to use the "username" value correctly.
        /// </summary>
        /// <returns></returns>
        public long GetCurrentUserId()
        {
            string sUserData = "";
            if ((HttpContext.Current.User != null) && (HttpContext.Current.User.Identity.IsAuthenticated) && (HttpContext.Current.User.Identity is FormsIdentity) && (!string.IsNullOrEmpty(((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData)))
            {
                sUserData = ((FormsIdentity)HttpContext.Current.User.Identity).Ticket.UserData;
                long userId;
                if (long.TryParse(sUserData, out userId))
                {
                    return userId;
                }
                else
                {
                    return -1;
                }
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

        public void SignIn(long UserId, string UserName, bool CreatePersistentCookie)
        {
            if (String.IsNullOrEmpty(UserName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            //FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                UserName,
                DateTime.Now,
                DateTime.Now.AddMinutes(30),
                CreatePersistentCookie,
                UserId.ToString(),
                FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie.
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

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