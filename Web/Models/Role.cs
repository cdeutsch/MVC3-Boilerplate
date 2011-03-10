using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Web.Infrastructure;

namespace Web.Models {

    public class Role 
    {
        public Role()
        {
            //this seems to be necessary if DB is empty.
            if (Users == null)
            {
                Users = new List<User>();
            }
        }

        [Key]
        public long RoleId { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50)]
        public string RoleName { get; set; }

        //relationships:
        public virtual ICollection<User> Users { get; set; }

        //overrides basic equality. By overriding this
        //you're telling the container how to find this object
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Role))
            {
                var comp = (Role)obj;
                return comp.RoleId.Equals(this.RoleId);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString()
        {
            return this.RoleId.ToString();
        }

        

    }

}
