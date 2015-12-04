using System;

namespace AccessControlTests
{
    public class Session
    {
        public int UserId { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string UserName { get; set; }
        public bool IsSsso { get; set; }
        public int LicenseLevel { get; set; }
    }
}