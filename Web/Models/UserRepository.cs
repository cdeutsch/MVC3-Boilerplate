using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace Web.Models
{
    public static class UserRepository
    {
        public static User CreateUser(SiteDB db, string Username, string Password, string Email)
        {
            //create a new user.
            User user = new User();
            user.Username = Username;
            user.Email = Email;
            user.Enabled = true;
            //create salt for password hash.
            user.PasswordSalt = CreateSalt();
            user.PasswordHash = CreatePasswordHash(Password, user.PasswordSalt);
            user.Created = DateTime.Now;
            user.Updated = user.Created;

            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }

        public static User CompleteRegistration(SiteDB db, string Username, string FirstName, string LastName)
        {
            
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                throw new ApplicationException("First name or last name is required.");
            }
            
            //get the user that should have been created by the membership provider.
            User user = GetUser(db, Username);
            if (user == null) 
            {
                throw new ApplicationException("The newly created User could not be found.");
            }

            //update values membership provider did not set.
            user.FirstName = FirstName;
            user.LastName = LastName;
                    
            db.SaveChanges();

            return user;
            
        }

        public static User DeleteUser(SiteDB db, string Username, bool DeleteData)
        {
            User user = GetUser(db, Username);
            if (user == null)
            {
                throw new ApplicationException("User not found.");
            }
            if (DeleteData)
            {
                db.Users.Remove(user);
            }
            else
            {
                user.Enabled = false;
            }
            db.SaveChanges();

            return user;
        }

        /// <summary>
        /// Will return a User if the specified Username matches with the Email or Username fields.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Username"></param>
        /// <returns></returns>
        public static User GetUser(SiteDB db, string Username)
        {
            return GetUser(db, Username, false);
        }
        /// <summary>
        /// Will return a User if the specified Username matches with the Email or Username fields.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="Username"></param>
        /// <param name="IncludeDisabled"></param>
        /// <returns></returns>
        public static User GetUser(SiteDB db, string Username, bool IncludeDisabled)
        {
            return db.Users.SingleOrDefault(oo => (oo.Username.ToLower() == Username.ToLower() || oo.Email.ToLower() == Username.ToLower()) && (IncludeDisabled || oo.Enabled == true));
        }
        public static User GetUser(SiteDB db, long UserId)
        {
            return GetUser(db, UserId, false);
        }
        public static User GetUser(SiteDB db, long UserId, bool IncludeDisabled)
        {
            return db.Users.SingleOrDefault(oo => (oo.UserId == UserId) && (IncludeDisabled || oo.Enabled == true));
        }

        public static User GetUserByUsername(SiteDB db, string Username)
        {
            return (from uu in db.Users
                    where uu.Enabled == true
                    && uu.Username.ToLower() == Username.ToLower()
                    select uu).SingleOrDefault();
        }

        public static User GetUserByEmail(SiteDB db, string Email)
        {
            return (from uu in db.Users
                    where uu.Enabled == true
                    && uu.Email.ToLower() == Email.ToLower()
                    select uu).SingleOrDefault();
        }

        public static IQueryable<User> FindUserByUsername(SiteDB db, string UsernameQuery)
        {
            return from uu in db.Users
                        where uu.Enabled == true
                        && uu.Username.ToLower().Contains(UsernameQuery.ToLower())
                        orderby uu.Username
                        select uu;
        }

        public static IQueryable<User> FindUserByEmail(SiteDB db, string EmailQuery)
        {
            return from uu in db.Users
                   where uu.Enabled == true
                   && uu.Email.ToLower().Contains(EmailQuery.ToLower())
                   orderby uu.Email
                   select uu;
        }

        public static bool ValidateUser(SiteDB db, string Username, string Password)
        {
            bool valid = false;
            User user = GetUser(db, Username);
            if (user != null)
            {
                //validate password by creating hash using salt.
                if (CreatePasswordHash(Password, user.PasswordSalt) == user.PasswordHash)
                {
                    valid = true;
                    user.LastLogin = DateTime.Now;
                    db.SaveChanges();
                }
            }
            return valid;
        }

        public static bool ChangePassword(SiteDB db, string Username, string OldPassword, string NewPassword)
        {
            bool bSuccess = false;
            User user = GetUser(db, Username);
            if (user != null)
            {
                //validate password by creating hash using salt.
                if (CreatePasswordHash(OldPassword, user.PasswordSalt) == user.PasswordHash)
                {
                    //ok to change password.
                    user.PasswordSalt = CreateSalt();
                    user.PasswordHash = CreatePasswordHash(NewPassword, user.PasswordSalt);
                    db.SaveChanges();
                    bSuccess = true;
                }
            }        
            return bSuccess;
        }

        /// <summary>
        /// Creates Salt with default size of 16.
        /// </summary>
        /// <returns></returns>
        public static string CreateSalt()
        {
            //default size to 16.
            return CreateSalt(16);
        }

        public static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        public static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = String.Concat(pwd, salt);
            //string hashedPwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");

            //manually do what FormsAuthentication does so we don't have to rely on a reference to System.Web.
            HashAlgorithm algorithm = SHA1.Create();
            var bytes = algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltAndPwd));
            string hex = BitConverter.ToString(bytes);
            string hashedPwd = hex.Replace("-", "");

            return hashedPwd;
        }
    }
}