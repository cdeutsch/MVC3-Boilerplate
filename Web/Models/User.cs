using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Web.Models {

    public class User : IAuditable
    {
        public User()
        {
            //this seems to be necessary if DB is empty.
            if (Roles == null)
            {
                Roles = new List<Role>();
            }
        }

        [Key] 
        public long UserId { get; set; }

        [Required(ErrorMessage="Email is required")]
        [StringLength(320)] 
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)] 
        public string Username { get; set; }

        [StringLength(100)] 
        public string PasswordHash { get; set; }
        [StringLength(25)] 
        public string PasswordSalt { get; set; }
        [StringLength(100)] 
        public string FirstName { get; set; }
        [StringLength(100)] 
        public string LastName { get; set; }
        [StringLength(250)] 
        public string Address1 { get; set; }
        [StringLength(250)] 
        public string Address2 { get; set; }
        [StringLength(250)] 
        public string City { get; set; }
        [StringLength(250)] 
        public string State { get; set; }
        [StringLength(250)] 
        public string Zip { get; set; }
        [StringLength(250)] 
        public string Country { get; set; }
        [StringLength(50)] 
        public string Phone { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime Updated { get; set; }
        public DateTime Created { get; set; }

        //relationships:
        public virtual ICollection<Role> Roles { get; set; }

        public string GetFriendlyName()
        {
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return (FirstName + " " + LastName).Trim();
            }
            else
            {
                return "";
            }
        }

        public void JustLoggedIn()
        {
            Updated = DateTime.Now;
            LastLogin = DateTime.Now;
        }

        //overrides basic equality. By overriding this
        //you're telling the container how to find this object
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(User))
            {
                var comp = (User)obj;
                return comp.UserId.Equals(this.UserId);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString()
        {
            return this.UserId.ToString();
        }

        

    }

}
