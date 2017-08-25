using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models;

namespace ArtifactStore.Helpers.Validators
{
    public class ValidationContext : IValidationContext
    {
        #region Properties

        /// <summary>
        /// Gets all available users.
        /// </summary>
        public IEnumerable<SqlUser> Users { get; }

        /// <summary>
        /// Gets all available groups.
        /// </summary>
        public IEnumerable<SqlGroup> Groups { get; }

        #endregion

        #region Constrcution

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext"/> class.
        /// </summary>
        public ValidationContext(IEnumerable<SqlUser> users, IEnumerable<SqlGroup> groups)
        {
            Users = users.ToList().AsReadOnly();
            Groups = groups.ToList().AsReadOnly();
        }

        #endregion

    }
}