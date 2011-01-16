using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;

namespace Web.Models
{
    public class SiteDB : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserActivity> UserActivitys { get; set; }
    }
}