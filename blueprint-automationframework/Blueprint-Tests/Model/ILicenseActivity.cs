using System;

namespace Model
{
    public interface ILicenseActivity
    {
        #region Properties
        int LicenseActivityId { get; set; }
        int UserId { get; set; }
        int LicenseType { get; set; }
        int TransactionType { get; set; }//release, acquire
        int ActionType { get; set; }//login, logout, timeout
        int ConsumerType { get; set; }//client, analytics
        DateTime Date { get; set; }
        string Username { get; set; }
        string Department { get; set; }
        #endregion Properties
        
        #region Methods
        #endregion Methods
    }
}
