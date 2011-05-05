using System;
using System.Data;
using System.Linq;
using System.Configuration;
using System.Configuration.Provider;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Specialized;
using Web.Models;

namespace Web.Infrastructure.UserMembershipProvider
{
    public class UserMembershipProvider : MembershipProvider
    {
        #region inner class miniUser
        /// <summary>
        /// Simple class used to pass around User data internally.
        /// </summary>
        internal class miniUser
        {
            public string Name { get; set; }
            public long MemberId { get; set; }
            public string Email { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime? LastLoginDate { get; set; }
        }
        #endregion

        #region Fields
        public const string EMAIL_REGEX = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        public const string DEFAULT_APPLICATION = "Web";

        private string _application = DEFAULT_APPLICATION;
        private NameValueCollection _config;

        #endregion


        #region Public (Overridden) Properties
        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get
            {
                return _application;
            }
            set
            {
                _application = value;
            }
        }
        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return Convert.ToInt32(_config["MaxInvalidPasswordAttempts"]);
            }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                return Convert.ToInt32(_config["MinRequiredNonAlphanumericCharacters"]);
            }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return Convert.ToInt32(_config["MinRequiredPasswordLength"]);
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return Convert.ToInt32(_config["PasswordAttemptWindow"]);
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get
            {
                return MembershipPasswordFormat.Hashed;
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _config["PasswordStrengthRegularExpression"]; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get
            {
                return Convert.ToBoolean(_config["RequiresUniqueEmail"]);
            }
        }
        #endregion

        #region Public (Overridden) Methods
        #region Implemented Methods

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool bSuccess = false;
            ValidatePasswordEventArgs args = OnValidatingPassword(username, newPassword, false);
            if (args.Cancel)
            {
                throw new ProviderException("Cannot change password.  See inner exception", args.FailureInformation);
            }
            else
            {
                using (SiteDB db = new SiteDB())
                {
                    bSuccess = UserRepository.ChangePassword(db, username, oldPassword, newPassword);
                }
            }
            return bSuccess;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            MembershipUser memUser = null;
            //validate email address.
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, EMAIL_REGEX))
            {
                status = MembershipCreateStatus.InvalidEmail;
            }
            else
            {
                try
                {
                    using (SiteDB db = new SiteDB())
                    {
                        //make sure a user with this username or email does not exist.
                        User user = db.Users.FirstOrDefault(uu => uu.Email.ToLower() == email.ToLower() || uu.Username.ToLower() == username.ToLower());
                        if (user != null)
                        {
                            if (string.Equals(user.Username, username, StringComparison.InvariantCultureIgnoreCase))
                            {
                                status = MembershipCreateStatus.DuplicateUserName;
                            }
                            else
                            {
                                status = MembershipCreateStatus.DuplicateEmail;
                            }
                        }
                        else
                        {
                            //create a new user.
                            user = UserRepository.CreateUser(db, username, password, email);

                            memUser = new MembershipUser(this.Name, user.Username, null, user.Email, string.Empty, string.Empty, true, false, user.Created, DateTime.MinValue, user.Created, user.Created, DateTime.MinValue);
                            status = MembershipCreateStatus.Success;
                        }
                    }
                }
                catch (System.Data.SqlClient.SqlException sqlExp)
                {
                    if (sqlExp.Message.StartsWith("Cannot insert duplicate key row in object 'dbo.User' with unique index 'IX_User'"))
                    {
                        status = MembershipCreateStatus.DuplicateEmail;
                        memUser = null;
                    }
                    else
                    {
                        status = MembershipCreateStatus.ProviderError;
                        memUser = null;
                    }
                }
                catch (Exception exp)
                {
                    status = MembershipCreateStatus.ProviderError;
                    memUser = null;
                    //throw exp;
                }
            }
            return memUser;
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (SiteDB db = new SiteDB())
            {
                bool bSuccess = false;
                try
                {
                    UserRepository.DeleteUser(db, username, deleteAllRelatedData);
                    bSuccess = true;
                }
                catch (ApplicationException exp)
                {
                    bSuccess = false;
                }
                return bSuccess;
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (SiteDB db = new SiteDB())
            {
                //build the query we'll use to find the user.
                IQueryable<User> qUsers = UserRepository.FindUserByEmail(db, emailToMatch);

                //get count.
                totalRecords = qUsers.Count();

                //filter result set for paging.
                IQueryable<miniUser> qMiniUsers = SelectMiniUsers(qUsers).Skip(pageIndex * pageSize).Take(pageSize);

                return BuildMembershipUserCollection(qMiniUsers);
            }
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (SiteDB db = new SiteDB())
            {
                //build the query we'll use to find the user.
                IQueryable<User> qUsers = UserRepository.FindUserByUsername(db, usernameToMatch);

                //get count.
                totalRecords = qUsers.Count();

                //filter result set for paging.
                IQueryable<miniUser> qMiniUsers = SelectMiniUsers(qUsers).Skip(pageIndex * pageSize).Take(pageSize);

                return BuildMembershipUserCollection(qMiniUsers);
            }
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"></see> collection that contains a page of pageSize<see cref="T:System.Web.Security.MembershipUser"></see> objects beginning at the page specified by pageIndex.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            using (SiteDB db = new SiteDB())
            {
                //build the query we'll use to find the user.
                IQueryable<User> qUsers = from uu in db.Users
                                          where uu.Enabled == true
                                          orderby uu.Email
                                          select uu;

                //get count.
                totalRecords = qUsers.Count();

                //filter result set for paging.
                IQueryable<miniUser> qMiniUsers = SelectMiniUsers(qUsers).Skip(pageIndex * pageSize).Take(pageSize);

                return BuildMembershipUserCollection(qMiniUsers);
            }
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override int GetNumberOfUsersOnline()
        {
            DateTime dtWindow = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);

            using (SiteDB db = new SiteDB())
            {
                var qUsers = from uu in db.Users
                             where uu.LastLogin.HasValue && uu.LastLogin.Value.CompareTo(dtWindow) > 0
                             select uu;
                return qUsers.Count();
            }
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (SiteDB db = new SiteDB())
            {
                //build the query we'll use to find the user.
                IQueryable<User> qUsers = from uu in db.Users
                                          where uu.Username.ToLower() == username.ToLower()
                                          && uu.Enabled == true
                                          select uu;

                //the internal function takes care of the rest.
                return _GetUser(db, qUsers, userIsOnline);
            }
        }

        /// <summary>
        /// Gets information from the data source for a user based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the specified user's information from the data source.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {

            using (SiteDB db = new SiteDB())
            {
                //build the query we'll use to find the user.
                IQueryable<User> qUsers = from uu in db.Users
                                          where uu.UserId == (long)providerUserKey
                                          && uu.Enabled == true
                                          select uu;

                //the internal function takes care of the rest.
                return _GetUser(db, qUsers, userIsOnline);
            }
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override string GetUserNameByEmail(string email)
        {
            using (SiteDB db = new SiteDB())
            {
                string username = (from uu in db.Users
                                   where uu.Enabled == true
                                   && uu.Email.ToLower() == email.ToLower()
                                   select uu.Username).SingleOrDefault();

                return username;
            }

        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <remarks>EDIT ON REUSE</remarks>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Verify that config isn't null
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
            {
                name = "UserMembershipProvider";
            }

            EnsureString(config, "description", @"User Membership Provider");

            base.Initialize(name, config);

            _config = config;

            // Set Default parameters
            EnsureInt(_config, "MaxInvalidPasswordAttempts", 5);
            EnsureInt(_config, "MinRequiredNonAlphanumericCharacters", 0);
            EnsureInt(_config, "MinRequiredPasswordLength", 6);
            EnsureInt(_config, "PasswordAttemptWindow", 10);
            EnsureBoolean(_config, "RequiresUniqueEmail", true);
            EnsureString(_config, "PasswordStrengthRegularExpression", "");

        }

        public override string ResetPassword(string username, string answer)
        {
            //reset the users password to a temporary one.
            using (SiteDB db = new SiteDB())
            {
                User user = UserRepository.GetUser(db, username);
                if (user != null)
                {
                    //create a new password, hash it and save it.
                    string sPassword = Membership.GeneratePassword(8, 1);
                    user.PasswordSalt = UserRepository.CreateSalt();
                    user.PasswordHash = UserRepository.CreatePasswordHash(sPassword, user.PasswordSalt);
                    db.SaveChanges();

                    return sPassword;
                }
                else
                {
                    throw new ProviderException("The user could not be found.");
                }
            }
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        public override bool ValidateUser(string username, string password)
        {
            bool valid = false;
            using (SiteDB db = new SiteDB())
            {
                valid = UserRepository.ValidateUser(db, username, password);
            }
            return valid;
        }

        #endregion
        #endregion

        #region Private Methods

        /// <summary>
        /// Builds a MembershipUser object from the given data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        private MembershipUser BuildMemberObject(miniUser user)
        {
            if (user != null)
            {
                DateTime lastLoginDate = DateTime.MinValue;
                if (user.LastLoginDate.HasValue)
                {
                    lastLoginDate = user.LastLoginDate.Value;
                }
                return new MembershipUser(this.Name, user.Name, (object)user.MemberId, user.Email,
                           null, null, true, false, user.CreateDate, lastLoginDate, lastLoginDate, DateTime.MinValue, DateTime.MinValue);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds a MembershipUser object from the given data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        private MembershipUser BuildMemberObject(User user)
        {
            if (user != null)
            {
                //convert User to miniUser and pass into BuildMemberObject.
                miniUser muu = new miniUser();
                muu.CreateDate = user.Created;
                muu.Email = user.Email;
                muu.LastLoginDate = user.LastLogin;
                muu.MemberId = user.UserId;
                muu.Name = user.Username;
                return BuildMemberObject(muu);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Turns the MiniUsers into a MembershipUserCollection.
        /// </summary>
        /// <param name="qMiniUsers"></param>
        private MembershipUserCollection BuildMembershipUserCollection(IQueryable<miniUser> qMiniUsers)
        {
            MembershipUserCollection vCollection = new MembershipUserCollection();
            foreach (miniUser user in qMiniUsers)
            {
                vCollection.Add(BuildMemberObject(user));
            }
            return vCollection;
        }

        private MembershipUser _GetUser(SiteDB db, IQueryable<User> qUsers, bool userIsOnline)
        {
            User user = qUsers.SingleOrDefault();

            UpdateLastLogin(db, user, userIsOnline);

            return BuildMemberObject(user);
        }

        /// <summary>
        /// Maps LINQ User object to local miniUser object.
        /// </summary>
        /// <param name="qUsers"></param>
        /// <returns></returns>
        /// <remarks>EDIT ON REUSE: User</remarks>
        private IQueryable<miniUser> SelectMiniUsers(IQueryable<User> qUsers)
        {
            IQueryable<miniUser> qMiniUsers = from uu in qUsers
                                              select new miniUser
                                              {
                                                  Name = uu.Username,
                                                  MemberId = uu.UserId,
                                                  Email = uu.Email,
                                                  CreateDate = uu.Created,
                                                  LastLoginDate = uu.LastLogin
                                              };
            return qMiniUsers;
        }

        /// <summary>
        /// If userIsOnline is true it updates the LastLoginDate.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="user"></param>
        /// <param name="userIsOnline"></param>
        /// <remarks>EDIT ON REUSE: User</remarks>
        private void UpdateLastLogin(SiteDB db, User user, bool userIsOnline)
        {
            if (userIsOnline && user != null)
            {
                user.LastLogin = DateTime.Now;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Called when a password needs validating.  Helper function to trigger ValidingPassword event.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="isNewUser">if set to <c>true</c> [is new user].</param>
        /// <returns></returns>
        private ValidatePasswordEventArgs OnValidatingPassword(string userName, string password, bool isNewUser)
        {
            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, password, isNewUser);
            base.OnValidatingPassword(e);
            return e;
        }

        /// <summary>
        /// Ensures the int is in the collection.
        /// </summary>
        /// <param name="config">The collection of configuration options.</param>
        /// <param name="tag">The tag that must be in the collection.</param>
        /// <param name="defaultValue">The default value used if the tag is missing.</param>
        public static void EnsureInt(NameValueCollection config, string tag, int defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue.ToString());
            }
            else
            {
                int dummy;
                if (!int.TryParse(val, out dummy))
                {
                    throw new ArgumentException(String.Format("Value for {0} parameter must be an integer (found \"{1}\")", tag, val));
                }
            }
        }
        
        /// <summary>
        /// Ensures the boolean is in the collection.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        public static void EnsureBoolean(NameValueCollection config, string tag, bool defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue.ToString());
            }
            else
            {
                bool dummy;
                if (!Boolean.TryParse(val, out dummy))
                {
                    throw new ArgumentException(String.Format("Value for {0} parameter must be an \"True\" or \"False\" (found \"{1}\")", tag, val));
                }
            }
        }

        /// <summary>
        /// Ensures the string is in the collection.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="defaultValue">The default value.</param>
        public static void EnsureString(NameValueCollection config, string tag, string defaultValue)
        {
            string val = config[tag];
            if (String.IsNullOrEmpty(val))
            {
                config.Remove(tag);
                config.Add(tag, defaultValue);
            }
        }

        #endregion
    }
}
