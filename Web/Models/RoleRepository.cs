using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public static class RoleRepository
    {
        public static Role CreateRole(SiteDB db, string RoleName)
        {
            Role role = new Role();
            role.RoleName = RoleName;
            db.Roles.Add(role);
            db.SaveChanges();
            
            return role;
        }

        public static Role GetRole(SiteDB db, string RoleName)
        {
            return db.Roles.SingleOrDefault(oo => oo.RoleName.ToLower() == RoleName.ToLower());
        }

        public static bool IsUserInRole(SiteDB db, string Username, string RoleName)
        {
            return FindUsersInRole(db, RoleName, Username).Count() == 1;
        }

        public static bool RoleExists(SiteDB db, string RoleName)
        {
            return db.Roles.Where(rr => rr.RoleName.ToLower() == RoleName.ToLower()).Count() == 1;
        }

        public static IQueryable<User> FindUsersInRole(SiteDB db, string RoleName)
        {
            return from uu in db.Users
                   where uu.Roles.Any(rr => rr.RoleName.ToLower() == RoleName.ToLower())
                    && uu.Enabled == true
                   orderby uu.Username
                   select uu;
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="RoleName"></param>
        /// <param name="UsernameQuery">Partial username.</param>
        /// <returns></returns>
        public static IQueryable<User> FindUsersInRole(SiteDB db, string RoleName, string UsernameQuery)
        {
            return from uu in FindUsersInRole(db, RoleName)
                   where uu.Username.ToLower().Contains(UsernameQuery.ToLower())
                   orderby uu.Username
                   select uu;
        }

        public static IQueryable<Role> GetRolesForUser(SiteDB db, string Username)
        {
            return from rr in db.Roles
                   where rr.Users.Any(uu => uu.Username.ToLower() == Username.ToLower() && uu.Enabled == true)
                   select rr;
        }

        public static void AddUsersToRoles(SiteDB db, string[] Usernames, string[] RoleNames)
        {
            //get all roles
            List<Role> lstRoles = db.Roles.ToList();
            //loop thru users.
            foreach (String username in Usernames)
            {
                //get the user.
                User user = UserRepository.GetUser(db, username);
                if (user != null)
                {
                    //loop thru roles.
                    foreach (string rolename in RoleNames)
                    {
                        //find the roleid of the role we need to add.
                        Role role = lstRoles.SingleOrDefault(oo => oo.RoleName.ToLower() == rolename.ToLower());
                        if (role != null)
                        {
                            //check if the user already has this role.
                            if (!user.Roles.Contains(role))
                            {
                                //add the role.
                                user.Roles.Add(role);
                            }
                        }
                    }
                }
            }
            db.SaveChanges();
        }

        public static void RemoveUsersFromRoles(SiteDB db, string[] Usernames, string[] RoleNames)
        {
            foreach (string roleName in RoleNames)
            {
                //get the role
                Role role = GetRole(db, roleName);
                if (role != null)
                {
                    foreach (string userName in Usernames)
                    {
                        User user = role.Users.SingleOrDefault(uu => uu.Username.ToLower() == userName.ToLower());
                        if (user != null)
                        {
                            role.Users.Remove(user);
                        }
                    }
                }
            }
            db.SaveChanges();
        }

        public static bool DeleteRole(SiteDB db, string RoleName, bool ErrorIfPopulated)
        {
            Role role = GetRole(db, RoleName);
            if (role != null)
            {
                if (ErrorIfPopulated) //  && GetUsersInRole(roleName).Length > 0)
                {
                    if (role.Users.Count() > 0)
                    {
                        throw new ApplicationException("Cannot delete a populated role.");
                    }
                }
                else
                {
                    //remove all users in this role (we shouldn't get here if throwOnPopulatedRole is true.
                    foreach (User user in role.Users)
                    {
                        role.Users.Remove(user);
                    }
                }

                //remove the role.
                db.Roles.Remove(role);

                db.SaveChanges();
                return true;
            }
            else
            {
                throw new ApplicationException("Role does not exist.");
            }
        }
    }
}