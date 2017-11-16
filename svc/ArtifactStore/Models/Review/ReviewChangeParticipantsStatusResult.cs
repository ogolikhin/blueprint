using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    public class ReviewChangeParticipantsStatusResult : ReviewChangeItemsStatusResult
    {
        public IEnumerable<DropdownItem> DropdownItems { get; set; }

    }
}