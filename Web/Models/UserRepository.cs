﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using Web.Infrastructure;
using System.IO;
using System.Net.Mail;
using Web.Models.Config;
using RazorEngine;
using DevOne.Security.Cryptography.BCrypt;

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
            user.PasswordHash = CreatePasswordHash(Password);
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
                if (BCryptHelper.CheckPassword(Password, user.PasswordHash))
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
                if (BCryptHelper.CheckPassword(OldPassword, user.PasswordHash))
                {
                    //ok to change password.
                    user.PasswordHash = CreatePasswordHash(NewPassword);
                    db.SaveChanges();
                    bSuccess = true;
                }
            }        
            return bSuccess;
        }

        public static bool ResetPassword(SiteDB db, User User, string NewPassword)
        {
            //ok to change password.
            User.PasswordHash = CreatePasswordHash(NewPassword);
            db.SaveChanges();

            return true;
        }

        public static void SendForgotPassword(SiteDB db, string Email, string ForgotPasswordUrlTemplate, string EmailTemplatesPath)
        {
            //get user by email and re-send verification email.
            User user = db.Users.SingleOrDefault(oo => oo.Email.ToLower() == Email.ToLower() && oo.Enabled);
            if (user == null)
            {
                throw new ApplicationException("User not found.");
            }
            //send email.
            SendForgotPassword(db, user, ForgotPasswordUrlTemplate, EmailTemplatesPath);
        }

        public static void SendForgotPassword(SiteDB db, User User, string ForgotPasswordUrlTemplate, string EmailTemplatesPath)
        {
            //delete existing PasswordResets
            foreach (PasswordReset pr in User.PasswordResets.ToList())
            {
                db.PasswordResets.Remove(pr);
            }
            //add new PasswordReset.
            PasswordReset passwordReset = new PasswordReset();
            passwordReset.ResetCode = RandomDataGenerator.GetRandomString(12);
            AuditableRepository.DefaultAuditableToNow(passwordReset);
            User.PasswordResets.Add(passwordReset);

            db.SaveChanges();

            string template = File.OpenText(EmailTemplatesPath + "ForgotPassword.html.cshtml").ReadToEnd();
            var model = new
            {
                Email = User.Email,
                FirstName = User.FirstName,
                LastName = User.LastName,
                ForgotPasswordUrl = string.Format(ForgotPasswordUrlTemplate, passwordReset.ResetCode),
                ApplicationName = SiteSettings.ApplicationName
            };
            string body = Razor.Parse(template, model);

            //email the invitation.
            MailMessage message = new MailMessage();
            message.To.Add(User.Email);
            message.Subject = "Reset " + SiteSettings.ApplicationName + " Password";
            message.Body = body;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient();
            client.Send(message);
        }

        public static User VerifyResetCode(SiteDB db, string ResetCode)
        {
            //get user associated with this code.
            PasswordReset passwordReset = db.PasswordResets.Include("User").SingleOrDefault(oo => oo.ResetCode == ResetCode);
            if (passwordReset == null)
            {
                throw new ApplicationException("Invalid reset code.");
            }

            return passwordReset.User;
        }

        public static PasswordReset DeleteResetCode(SiteDB db, string ResetCode)
        {
            //delete the reset code.
            PasswordReset passwordReset = db.PasswordResets.SingleOrDefault(oo => oo.ResetCode == ResetCode);
            if (passwordReset != null)
            {
                db.PasswordResets.Remove(passwordReset);
                db.SaveChanges();
            }

            return passwordReset;
        }
        

        public static string CreatePasswordHash(string pwd)
        {
            string salt = BCryptHelper.GenerateSalt(8);

            var hashedPassword = BCryptHelper.HashPassword(pwd, salt);

            return hashedPassword;
        }
    }
}