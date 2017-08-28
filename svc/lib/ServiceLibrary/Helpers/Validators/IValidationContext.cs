using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ServiceLibrary.Helpers.Validators
{
    public interface IValidationContext
    {
        IEnumerable<SqlUser> Users { get; }
        IEnumerable<SqlGroup> Groups { get; }
    }
}
