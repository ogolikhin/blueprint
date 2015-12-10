﻿using System;
using Model.Factories;

namespace AccessControlTests
{
    public class Session
    {
        public static Session CreateSession()
        {
            return new Session(RandomGenerator.RandomNumber(), RandomGenerator.RandomAlphaNumeric(7), 3, true);
        }
        public Session(int UserId, string UserName, int LicenseLevel, bool IsSso)
        {
            this.UserId = UserId;
            this.UserName = UserName;
            this.LicenseLevel = LicenseLevel;
            this.IsSso = IsSso;
        }

        public bool Equals(Session session)
        {
            if (session == null)
                return false;
            return (this.UserId == session.UserId) && (this.UserName == session.UserName) &&
                (this.LicenseLevel == session.LicenseLevel) && (this.IsSso == session.IsSso);
        }
        public int UserId { get; private set; }
        public DateTime? BeginTime { get; private set; }
        public DateTime? EndTime { get; private set; }
        public string UserName { get; private set; }
        public bool IsSso { get; private set; }
        public int LicenseLevel { get; private set; }
    }
}