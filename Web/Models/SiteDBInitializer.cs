using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure;
using Web.Infrastructure;
using System.Data.Entity.Database;

namespace Web.Models
{

    public class SiteDBInitializer : DropCreateDatabaseIfModelChanges<SiteDB>
    {
        public IMembershipService MembershipService { get; set; }

        public SiteDBInitializer(IMembershipService MembershipService)
        {
            this.MembershipService = MembershipService;
        }

        /// <summary>
        /// Seed database with defaults.
        /// </summary>
        /// <param name="db"></param>
        /// <remarks>WARNING: Does not run until you try to access the DB.</remarks>
        protected override void Seed(SiteDB db)
        {
            ////add a User.
            MembershipService.CreateUser("cdeutsch", "Mvc3333", "cd@cdeutsch.com");
            //finish the registration that the MembershipProvider did not handle.
            User user = UserRepository.CompleteRegistration(db, "cdeutsch", "Christopher", "Deutsch");

            db.SaveChanges();
        }
    }

}