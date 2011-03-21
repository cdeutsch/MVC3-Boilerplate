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
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserActivity> UserActivitys { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ////define "many-to-many" relationships:
            modelBuilder.Entity<User>()
                .HasMany(uu => uu.Roles)
                .WithMany(oo => oo.Users)
                .Map(mm => mm.ToTable("User_Roles"));
        }
    }
}