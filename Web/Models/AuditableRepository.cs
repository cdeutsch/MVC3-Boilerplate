using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public static class AuditableRepository
    {
        public static void DefaultAuditableToNow(IAuditable obj)
        {
            obj.Created = DateTime.Now;
            obj.Updated = obj.Created;
        }
    }
}