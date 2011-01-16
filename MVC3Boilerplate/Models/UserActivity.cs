using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Web.Models {
    public class UserActivity {

        public UserActivity()
        {

        }

        [Key] 
        public long UserActivityID { get; set; }
        public long UserID { get; set; }
        [StringLength(50)] 
        public string Activity { get; set; }
        public DateTime Created { get; set; }

        SiteDB _db;
        public UserActivity(SiteDB db)
        {
            _db = db;
        }
        public void LogIt(long userId, string activity)
        {
            var log = new UserActivity();
            log.Activity = activity;
            log.Created = DateTime.Now;
            log.UserID = userId;
            _db.UserActivitys.Add(log);
            _db.SaveChanges();
        }
        
    }
}
