using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.IO;
using System.Configuration;
using Web.Infrastructure.FormsAuthenticationService;
using Web.Models;

namespace Web.Common
{

    public static class CacheHelper
    {
        public const string UserFriendlyNameKey = "FriendlyName";

        /// <summary>
        /// Cache any info about the user we'll be accessing frequently like their name, email, or Organization.
        /// </summary>
        /// <param name="FormsAuthService"></param>
        /// <param name="user"></param>
        public static void CacheUserData(IFormsAuthenticationService FormsAuthService, User user)
        {
            ////cache all the info we can for the user.
            //friendlyname:
            HttpContext.Current.Cache.GetOrStore<string>(GetUserSignInKey(FormsAuthService, UserFriendlyNameKey), user.Username);
        }

        /// <summary>
        /// Return how we want to display the user's name in the UI. Currently set to Username, could be changed to First/Last, etc,
        /// </summary>
        /// <param name="FormsAuthService"></param>
        /// <returns></returns>
        public static string GetUserFriendlyName(IFormsAuthenticationService FormsAuthService)
        {
            return HttpContext.Current.Cache.GetOrStore<string>(
                GetUserSignInKey(FormsAuthService, UserFriendlyNameKey),
                () =>
                {
                    if (FormsAuthService.IsAuthenticated())
                    {
                        using (SiteDB db = new SiteDB())
                        {
                            var user = UserRepository.GetUser(db, FormsAuthService.GetCurrentUserId());
                            if (user != null)
                            {
                                return user.Username;
                            }
                            else
                            {
                                return "";
                            }
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            );
        }

        /// <summary>
        /// Data is cached by User and their last login. So that the cache will reload when the user does a logout/login.
        /// </summary>
        /// <param name="FormsAuthService"></param>
        /// <param name="Prefix"></param>
        /// <returns></returns>
        public static string GetUserSignInKey(IFormsAuthenticationService FormsAuthService, string Prefix)
        {
            return (Prefix.AsNullIfWhiteSpace() ?? "") + FormsAuthService.GetCurrentUserName() + "-" + FormsAuthService.GetCurrentUserSignInTicks();
        }

    }
}