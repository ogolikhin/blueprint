using System.Collections.Generic;

namespace ServiceLibrary.Models.VersionControl
{
    public class SqlDiscardPublishDetailsResult
    {
        public ICollection<SqlDiscardPublishDetails> Details { get; set; }

        public IDictionary<int, string> ProjectInfos { get; set; }
    }
}
