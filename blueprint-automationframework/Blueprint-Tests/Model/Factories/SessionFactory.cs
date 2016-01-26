using System;
using Model.Impl;
using Utilities.Factories;

namespace Model.Factories
{
    public static class SessionFactory
    {
        /// <summary>
        /// Creates a new session with random values.
        /// </summary>
        /// <returns>A new random Session object.</returns>
        public static ISession CreateRandomSession()
        {
            return new Session(RandomGenerator.RandomNumber(), RandomGenerator.RandomAlphaNumeric(7), 3, true);
        }

        /// <summary>
        /// Creates a new session for specified user.
        /// </summary>
        /// <paran name=user>IUser object.</paran>
        /// <returns>A new Session object.</returns>
        public static ISession CreateSession(IUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            else
            {
                return new Session(user.UserId, user.Username, 3, true);
            }
        }
    }
}
