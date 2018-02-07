using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ServiceLibrary.Models.Workflow
{
    [XmlRoot(ElementName = "WebhookSecurityInfo")]
    public class XmlWebhookSecurityInfo
    {
        [XmlElement(ElementName = "IgnoreInvalidSSLCertificate", IsNullable = false)]
        public bool IgnoreInvalidSSLCertificate { get; set; }

        [XmlElement(ElementName = "Payload", IsNullable = false), DefaultValue("application/json")]
        public string Payload { get; set; }

        private List<string> _httpHeaders;
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("HttpHeaders"), XmlArrayItem("HttpHeader")]
        public List<string> HttpHeaders
        {
            get
            {
                return _httpHeaders ?? (_httpHeaders = new List<string>());
            }
            set
            {
                _httpHeaders = value;
            }
        }

        [XmlElement("BasicAuthentication")]
        public XmlWebhookBasicAuth BasicAuth { get; set; }

        [XmlElement("Signature")]
        public XmlWebhookSignature Signature { get; set; }
    }

    public class XmlWebhookBasicAuth
    {
        [XmlElement("Username")]
        public string Username { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }
    }

    public class XmlWebhookSignature
    {
        [XmlElement("SecretToken")]
        public string SecretToken { get; set; }

        [XmlElement("Algorithm")]
        public string Algorithm { get; set; }

    }
}
