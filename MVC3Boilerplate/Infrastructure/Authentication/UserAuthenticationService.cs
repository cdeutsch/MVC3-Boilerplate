using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Web.Models;

namespace Web.Infrastructure.Authentication
{
    public class UserAuthenticationService : IAuthenticationService
    {
        SiteDB _db;

        public UserAuthenticationService()
        {
            _db = new SiteDB();
        }

        public bool IsValidLogin(string userName, string password)
        {
            User user = UserRepository.GetUser(_db, userName, password);
            return (user != null);
        }

        public bool RegisterUser(string userName, string password, string confirmPassword, string email, string reminderQuestion, string reminderAnswer)
        {
            User user = UserRepository.RegisterUser(_db, userName, password, confirmPassword, email);
            return (user != null);
        }
    }

   
}