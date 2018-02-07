using System.ComponentModel;
using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("Signature")]
    public class IeSignature
    {
        #region Properties

        [XmlElement(IsNullable = false)]
        public string SecretToken { get; set; }

        [XmlElement(IsNullable = false)]
        public string Algorithm { get; set; }

        #endregion

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeSignature other)
        {
            return string.Equals(SecretToken, other.SecretToken) &&
                   string.Equals(Algorithm, other.Algorithm);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeSignature)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {

                int hashCode = SecretToken != null ? SecretToken.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Algorithm != null ? Algorithm.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}