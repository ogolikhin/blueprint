using System.Collections.Generic;

namespace ServiceLibrary.Models.VersionControl
{
    public class SqlDiscardPublishDetailsResult
    {
        public IList<SqlDiscardPublishDetails> Details { get; } = new List<SqlDiscardPublishDetails>();

        public IDictionary<int, string> ProjectInfos { get; } = new Dictionary<int, string>();
    }
}
