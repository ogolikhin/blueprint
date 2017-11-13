using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    public class ReviewChangeParticipantsStatusResult : ReviewChangeItemsStatusResult
    {
        public IEnumerable<DropdownItem> DropdownItems { get; set; }

    }
}