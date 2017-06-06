using System.Collections.Generic;
using System.Linq;

namespace Model.NovaModel.AdminStoreModel
{
    // Found in: blueprint/svc/lib/ServiceLibrary/Models/OperationScope.cs (in bp-offshore/blueprint repo)
    public class OperationScope
    {
        public bool SelectAll { get; set; }

        public List<int> Ids { get; set; }

        public bool IsEmpty()
        {
            return !SelectAll && (Ids == null || !Ids.Any());
        }
    }
}
