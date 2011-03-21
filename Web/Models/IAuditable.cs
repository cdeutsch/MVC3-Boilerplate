using System;

namespace Web.Models
{
    public interface IAuditable
    {
        DateTime Updated { get; set; }
        DateTime Created { get; set; }
    }
}
