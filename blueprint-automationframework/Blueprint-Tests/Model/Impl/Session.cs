
namespace Model.Impl
{
    public class Session : ISession
    {
        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public bool IsSso { get; private set; }
        public int LicenseLevel { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userId">The User ID.</param>
        /// <param name="userName">The Username.</param>
        /// <param name="licenseLevel">The license level</param>  // TODO: Should this be an enum?
        /// <param name="isSso"></param>  // TODO: What is this for?
        public Session(int userId, string userName, int licenseLevel, bool isSso)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.LicenseLevel = licenseLevel;
            this.IsSso = isSso;
        }

        /// <summary>
        /// Tests whether the specified Session is equal to this one.
        /// </summary>
        /// <param name="session">The Session to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        public bool Equals(ISession session)
        {
            if (session == null)
            {
                return false;
            }

            return (this.UserId == session.UserId) && (this.UserName == session.UserName) &&
                (this.LicenseLevel == session.LicenseLevel) && (this.IsSso == session.IsSso);
        }
    }
}