using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactsQueryResult<T> : QueryResult<T> where T : BaseReviewArtifact
    {
        public bool IsFormal { get; set; }
    }
}
