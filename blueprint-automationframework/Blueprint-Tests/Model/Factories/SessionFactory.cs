using System;
using Model.Impl;
using Utilities;
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
        /// <paran name="user">IUser object.</paran>
        /// <returns>A new Session object.</returns>
        public static ISession CreateSession(IUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return new Session(user.UserId, user.Username, 3, true);
        }
    }
}
