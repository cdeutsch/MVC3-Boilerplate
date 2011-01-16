using System;

namespace Web.Infrastructure.Session {

    public interface IUserSession
    {
        long GetCurrentUserId();
        string GetCurrentUserName();
    }
}
