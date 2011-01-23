using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Security.Cryptography;
using System.Data.Entity;

namespace Web.Models
{
    public class AuditableRepository
    {
        public static void DefaultAuditableToNow(IAuditable obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = obj.Created;
        }
    }
}