using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Helpers;
using ServiceLibrary.Helpers;

namespace AdminStore.Helpers.Workflow
{
    public static class WorkflowHelper
    {
        public static bool CollectionEquals<T>(IEnumerable<T> col1, IEnumerable<T> col2)
        {
            if (ReferenceEquals(col1, col2))
            {
                return true;
            }

            if (col1.IsEmpty() && col2.IsEmpty())
            {
                return true;
            }

            if (col1?.GetType() != col2?.GetType())
            {
                return false;
            }
            var list1 = col1.ToList() ;
            var list2 = col2.ToList();
            if (list1.Count != list2.Count)
            {
                return false;
            }

            return !list1.Where((t, i) => !t.Equals(list2[i])).Any();
        }

        public static T CloneViaXmlSerialization<T>(T serObject) where T : class
        {
            if (serObject == null)
            {
                return null;
            }

            var xml = SerializationHelper.ToXml(serObject);
            var clone = SerializationHelper.FromXml<T>(xml);
            return clone;
        }
    }
}