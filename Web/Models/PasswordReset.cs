using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Web.Models {

    public class PasswordReset : IAuditable
    {
        [Key]
        public long PasswordResetId { get; set; }

        public long UserId { get; set; }

        [Required(ErrorMessage = "Reset Code is required")]
        [StringLength(50)] 
        public string ResetCode { get; set; }
                
        public DateTime Updated { get; set; }
        public DateTime Created { get; set; }
        
        //relationships:
        public virtual User User { get; set; }

        
        //overrides basic equality. By overriding this
        //you're telling the container how to find this object
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(PasswordReset))
            {
                var comp = (PasswordReset)obj;
                return comp.PasswordResetId.Equals(this.PasswordResetId);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override string ToString()
        {
            return this.PasswordResetId.ToString();
        }

        

    }

}
