using AdminStore.Models.Enums;

namespace AdminStore.Models.Workflow
{
    public class IePortPair
    {
        public DiagramPort FromPort { get; set; }
        public DiagramPort ToPort { get; set; }

        protected bool Equals(IePortPair other)
        {
            return FromPort == other.FromPort && ToPort == other.ToPort;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IePortPair)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)FromPort * 397) ^ (int)ToPort;
            }
        }
    }
}