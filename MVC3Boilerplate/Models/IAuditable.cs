using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Web.Models
{
    public interface IAuditable
    {
        DateTime Updated { get; set; }
        DateTime Created { get; set; }
    }
}
