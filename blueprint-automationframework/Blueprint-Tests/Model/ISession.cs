
namespace Model
{
    public interface ISession
    {
        int UserId { get; }
        string UserName { get; }
        bool IsSso { get; }
        int LicenseLevel { get; }

        /// <summary>
        /// Tests whether the specified Session is equal to this one.
        /// </summary>
        /// <param name="session">The Session to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        bool Equals(ISession session);
    }
}
