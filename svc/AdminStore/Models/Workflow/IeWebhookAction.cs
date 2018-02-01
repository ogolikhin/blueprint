using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using AdminStore.Helpers.Workflow;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.Workflow
{
    // !!! Updating of this class requires regenerating of the xml schema IeWorkflow.xsd is required, see below:
    // !!! xsd.exe AdminStore.dll /t:IeWorkflow
    [XmlType("WebhookAction")]
    public class IeWebhookAction : IeBaseAction
    {
        #region Properties

        [XmlIgnore]
        public override ActionTypes ActionType => ActionTypes.Webhook;

        [XmlElement(IsNullable = false)]
        public string Url { get; set; }

        // Optional
        [XmlElement]
        public bool? IgnoreInvalidSSLCertificate { get; set; }
        public bool ShouldSerializeIgnoreInvalidSSLCertificate() { return IgnoreInvalidSSLCertificate.HasValue; }

        // Optional
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("HttpHeaders"), XmlArrayItem("Header")]
        public List<string> HttpHeaders { get; set; }
        public bool ShouldSerializeHttpHeaders() { return HttpHeaders != null; }

        // Optional
        [XmlElement]
        public IeBasicAuth BasicAuth { get; set; }
        public bool ShouldSerializeBasicAuth() { return BasicAuth != null; }

        // Optional
        [XmlElement]
        public IeSignature Signature { get; set; }
        public bool ShouldSerializeSignature() { return Signature != null; }

        #endregion

        #region Generated and modified Equals and GetHashCode methods

        protected bool Equals(IeWebhookAction other)
        {
            return base.Equals(other) &&
                string.Equals(Url, other.Url) &&
                IgnoreInvalidSSLCertificate.GetValueOrDefault() == other.IgnoreInvalidSSLCertificate.GetValueOrDefault() &&
                WorkflowHelper.CollectionEquals(HttpHeaders, other.HttpHeaders) &&
                Equals(BasicAuth, other.BasicAuth) &&
                Equals(Signature, other.Signature);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IeWebhookAction)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Url != null ? Url.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IgnoreInvalidSSLCertificate.GetHashCode();
                hashCode = (hashCode * 397) ^ (HttpHeaders != null ? HttpHeaders.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BasicAuth != null ? BasicAuth.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Signature != null ? Signature.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}