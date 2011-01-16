using System;

namespace Web.Infrastructure.Authentication {
    
    public interface IAuthenticationService {
        bool IsValidLogin(string userName, string password);
        bool RegisterUser(string userName, 
            string password, 
            string confirmPassword, 
            string email, 
            string reminderQuestion, 
            string reminderAnswer);
    }
}
