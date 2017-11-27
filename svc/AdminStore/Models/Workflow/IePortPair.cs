namespace AdminStore.Models.Workflow
{
    public class IePortPair
    {
        public int FromPort { get; set; }
        public int ToPort { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IePortPair)obj);
        }

        protected bool Equals(IePortPair other)
        {
            return FromPort == other.FromPort && ToPort == other.ToPort;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FromPort * 397) ^ ToPort;
            }
        }
    }
}