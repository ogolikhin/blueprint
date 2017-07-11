namespace AdminStore.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Scope { get; set; }
        public int LicenseId { get; set; }
        public byte Source { get; set; }
        public string Email { get; set; }
        public int CurrentVersion { get; set; }
        public int? ProjectId { get; set; }
        public string GroupType { get; set; }
    }
}