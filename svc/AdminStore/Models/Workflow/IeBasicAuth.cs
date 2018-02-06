using System.Xml.Serialization;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("BasicAuth")]
    public class IeBasicAuth
    {
        #region Properties

        [XmlElement(IsNullable = false)]
        public string Username { get; set; }

        [XmlElement(IsNullable = false)]
        public string Password { get; set; }

        #endregion

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeBasicAuth other)
        {
            return string.Equals(Username, other.Username) &&
                   string.Equals(Password, other.Password);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeBasicAuth)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {

                int hashCode = Username != null ? Username.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}