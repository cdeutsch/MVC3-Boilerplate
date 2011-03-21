using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Security;

namespace Web.Models
{

    public class SiteDBInitializer : DropCreateDatabaseIfModelChanges<SiteDB>
    {
        /// <summary>
        /// Seed database with defaults.
        /// </summary>
        /// <param name="db"></param>
        /// <remarks>WARNING: Does not run until you try to access the DB.</remarks>
        protected override void Seed(SiteDB db)
        {
            ////add a User (we used to use the MembershipService but EF 4.1 doesn't like that anymore since it creates a second SideDB instance, guess we really didn't need MembershipService anyway)
            //http://stackoverflow.com/questions/5365376/system-nullreferenceexception-after-upgrade-to-ef-4-1
            UserRepository.CreateUser(db, "cdeutsch", "Mvc3333", "cd@cdeutsch.com");
            //finish the registration that the MembershipProvider did not handle.
            User user = UserRepository.CompleteRegistration(db, "cdeutsch", "Christopher", "Deutsch");

            db.SaveChanges();
        }
    }

}