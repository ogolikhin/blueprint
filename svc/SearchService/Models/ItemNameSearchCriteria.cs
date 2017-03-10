using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class ItemNameSearchCriteria : SearchCriteria
    {
        /// <summary>
        /// The set of IDs of Projects to include.
        /// </summary>
        [Required]
        public IEnumerable<int> ProjectIds { get; set; }

        /// <summary>
        /// True to include regular artifacts; False otherwise. The default is True.
        /// </summary>
        public bool ShowArtifacts { get; set; } = true;

        /// <summary>
        /// True to include Baseline and Review artifacts; False otherwise. The default is False.
        /// </summary>
        public bool ShowBaselinesAndReviews { get; set; }

        /// <summary>
        /// True to include Collection artifacts in the search; False otherwise. The default is False.
        /// </summary>
        public bool ShowCollections { get; set; }

        /// <summary>
        /// The set of IDs of predefined item types to include (if allowed by the <see cref="ShowArtifacts"/>,
        /// <see cref="ShowBaselinesAndReviews"/> and <see cref="ShowCollections"/> parameters). Ignored if empty.
        /// </summary>
        public IEnumerable<int> PredefinedTypeIds { get; set; }

        /// <summary>
        /// The set of IDs of item types to include. Ignored if empty.
        /// </summary>
        public IEnumerable<int> ItemTypeIds { get; set; }

        /// <summary>
        /// True to include the artifact path in the results, False Otherwise. The default is False.
        /// </summary>
        public bool IncludeArtifactPath { get; set; }
    }
}
