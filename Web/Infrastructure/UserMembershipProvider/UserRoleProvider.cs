using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
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
    public class UserRoleProvider : RoleProvider
    {
        #region Fields

        private string _application = UserMembershipProvider.DEFAULT_APPLICATION;
        private NameValueCollection _config;
        private string _connectionStringName = null;
        private ConnectionStringSettings _csSettings;

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



        #endregion

        #region Public (Overridden) Methods
        #region Implemented Methods

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (SiteDB db = new SiteDB())
            {
                RoleRepository.AddUsersToRoles(db, usernames, roleNames);
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                throw new ProviderException("Role name cannot be empty or null.");
            if (roleName.IndexOf(',') > 0)
                throw new ArgumentException("Role names cannot contain commas.");
            if (roleName.Length > 255)
                throw new ProviderException("Role name cannot exceed 255 characters.");
            if (RoleExists(roleName))
                throw new ProviderException("Role name already exists.");

            using (SiteDB db = new SiteDB())
            {
                RoleRepository.CreateRole(db, roleName);
            }
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if <paramref name="roleName"/> has one or more members and do not delete <paramref name="roleName"/>.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (SiteDB db = new SiteDB())
            {
                try
                {
                    return RoleRepository.DeleteRole(db, roleName, throwOnPopulatedRole);
                }
                catch (ApplicationException exp)
                {
                    throw new ProviderException(exp.Message, exp);
                }
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches <paramref name="usernameToMatch"/> and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (SiteDB db = new SiteDB())
            {
                return RoleRepository.FindUsersInRole(db, roleName, usernameToMatch).Select(uu => uu.Email).ToArray();
            }            
        }
        
        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            using (SiteDB db = new SiteDB())
            {
                var qRoles = from rr in db.Roles
                             select rr.RoleName;

                return qRoles.ToArray();
            }
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            using (SiteDB db = new SiteDB())
            {
                return RoleRepository.GetRolesForUser(db, username).Select(rr => rr.RoleName).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            using (SiteDB db = new SiteDB())
            {
                return RoleRepository.FindUsersInRole(db, roleName).Select(uu => uu.Email).ToArray();
            }            
        }

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
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
                name = "UserRoleProvider";
            }

            UserMembershipProvider.EnsureString(config, "description", @"User Role Provider");

            base.Initialize(name, config);

            _config = config;

        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            using (SiteDB db = new SiteDB())
            {
                return RoleRepository.IsUserInRole(db, username, roleName);
            }            
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (SiteDB db = new SiteDB())
            {
                RoleRepository.RemoveUsersFromRoles(db, usernames, roleNames);
            }
            return;
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            using (SiteDB db = new SiteDB())
            {
                return RoleRepository.RoleExists(db, roleName);
            }
        }

        #endregion
        #endregion

        
        
    }
}
