using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Cryptography;

namespace Web.Models
{
    public class UserRepository
    {
        public const string EMAIL_REGEX = @"^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";
        public const int MIN_PASSWORD_LENGTH = 8;

        public static User RegisterUser(SiteDB db, string Username, string Password, string ConfirmPassword, string Email)
        {
            User user = null;
            //validate email address.
            if (!System.Text.RegularExpressions.Regex.IsMatch(Email, EMAIL_REGEX))
            {
                throw new InvalidOperationException("Invalid email address");
            }
            else if (string.IsNullOrEmpty(Password) || Password.Length < MIN_PASSWORD_LENGTH)
            {
                throw new InvalidOperationException(string.Format("Password must be at least {0} characters long.", MIN_PASSWORD_LENGTH));
            }
            else if (Password != ConfirmPassword)
            {
                throw new InvalidOperationException("Passwords do not match.");
            }
            else if (db.Users.SingleOrDefault(oo => oo.Email == Email) != null)
            {
                throw new InvalidOperationException("This email address is already in use.");
            }
            else if (db.Users.SingleOrDefault(oo => oo.Username == Username) != null)
            {
                throw new InvalidOperationException("This username is already in use.");
            }
            else
            {
                try
                {
                    //create a new user.
                    user = new User();
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

                }
                catch (System.Data.SqlClient.SqlException sqlExp)
                {
                    if (IsDuplicateUserError((Exception)sqlExp))
                    {
                        throw new InvalidOperationException("This email is already in our system");
                    }
                    else
                    {
                        throw new InvalidOperationException("There's a problem saving your information");
                    }
                }
                catch (Exception exp)
                {
                    throw new InvalidOperationException("There's a problem saving your information");
                }
            }
            return user;
        }

        public static User GetUser(SiteDB db, string Username, string Password)
        {
            User user = GetUser(db, Username);
            if (user != null)
            {
                ////clear user if password doesn't validate.
                //validate password by creating hash using salt.
                if (CreatePasswordHash(Password, user.PasswordSalt) != user.PasswordHash)
                {
                    user = null;
                }
            }
            return user;
        }

        public static User GetUser(SiteDB db, string Username)
        {
            return GetUser(db, Username, false);
        }

        public static User GetUser(SiteDB db, string Username, bool IncludeDisabled)
        {
            return db.Users.SingleOrDefault(oo => (oo.Username.ToLower() == Username.ToLower() || oo.Email.ToLower() == Username.ToLower()) && (IncludeDisabled || oo.Enabled == true));
        }

        /// <summary>
        /// Creates Salt with default size of 16.
        /// </summary>
        /// <returns></returns>
        protected static string CreateSalt()
        {
            //default size to 16.
            return CreateSalt(16);
        }

        protected static string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        protected static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = String.Concat(pwd, salt);
            string hashedPwd = FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "sha1");

            return hashedPwd;
        }

        protected static bool IsDuplicateUserError(Exception exp)
        {
            //return (exp.Message.StartsWith("Column names in each table must be unique") || exp.Message.StartsWith("Violation of UNIQUE KEY constraint"));
            return (exp.Message.StartsWith("Cannot insert duplicate key row in object 'dbo.User' with unique index 'IX_Users'"));
        }


    }
}