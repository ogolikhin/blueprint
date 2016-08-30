using Model.Impl;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class SessionFactory
    {
        // TODO: Find out what 3 means and give this a better name...
        private const int LicenseLevel = 3;

        /// <summary>
        /// Creates a new session with random values.
        /// </summary>
        /// <returns>A new random Session object.</returns>
        public static ISession CreateRandomSession()
        {
            return new Session(RandomGenerator.RandomNumber(), RandomGenerator.RandomAlphaNumeric(7), LicenseLevel, true);
        }

        /// <summary>
        /// Creates a new session for the specified user.
        /// </summary>
        /// <paran name="user">IUser object.</paran>
        /// <returns>A new Session object.</returns>
        public static ISession CreateSession(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return new Session(user.Id, user.Username, LicenseLevel, true);
        }

        /// <summary>
        /// Creates a new ISession for the specified user using the AccessControl token that the user already has.
        /// </summary>
        /// <param name="user">IUser object.</param>
        /// <param name="isSso">(optional) Specifies if this is a Single Sign On session.</param>
        /// <returns>A new Session object.</returns>
        public static ISession CreateSessionWithToken(IUser user, bool isSso = true)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return new Session(user.Id, user.Username, LicenseLevel, isSso, user.Token?.AccessControlToken);
        }
    }
}
