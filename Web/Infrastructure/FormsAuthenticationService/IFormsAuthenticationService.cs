using System;
using System.Collections.Generic;
using System.Linq;

namespace Web.Infrastructure.FormsAuthenticationService
{
    public interface IFormsAuthenticationService
    {
        bool IsAuthenticated();
        long GetCurrentUserId();
        string GetCurrentUserName();
        string GetCurrentUserSignInTicks();

        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }
}
