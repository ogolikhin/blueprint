namespace AdminStore.Models
{
    public class FederatedAuthentication
    {
        public bool IsEnabled { get; set; }

        public byte[] Certificate { get; set; }

        public string Settings { get; set; }
    }
}