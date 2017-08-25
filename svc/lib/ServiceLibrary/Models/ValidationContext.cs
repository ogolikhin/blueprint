using System.Collections.Generic;

namespace ServiceLibrary.Models
{

    public interface IValidationContext
    {
        IEnumerable<SqlUser> Users { get; }
        IEnumerable<SqlGroup> Groups { get; }
    }
}
