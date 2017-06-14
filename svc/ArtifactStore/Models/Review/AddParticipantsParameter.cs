﻿using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class AddParticipantsParameter
    {
        public IEnumerable<int> UserIds { get; set; }   // = new List<int>();

        public IEnumerable<int> GroupIds { get; set; }  // = new List<int>();
    }
}
