﻿using Model;
using Model.Factories;
using Model.Impl;

namespace Helper.Factories
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
    }
}
