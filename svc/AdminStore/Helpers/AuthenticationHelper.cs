using System;
using AdminStore.Models;

namespace AdminStore.Helpers
{
    public class AuthenticationHelper
    {
        /// <summary>
        /// Authenticate database user and validate password expiration if 'passwordExpirationInDays' parameter provided
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="passwordExpirationInDays"></param>
        /// <returns></returns>
        public AuthenticationStatus AuthenticateDatabaseUser(User user, string password, int? passwordExpirationInDays)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (user.Source != UserGroupSource.Database)
            {
                throw new ArgumentException("Database user shoud be provided");
            }

            var hashedPassword = HashingUtilities.GenerateSaltedHash(password, user.UserSalt);
            if (!string.Equals(user.Password, hashedPassword))
            {
                return AuthenticationStatus.InvalidCredentials;
            }
            if (passwordExpirationInDays.HasValue)
            {
                if (HasExpiredPassword(user, passwordExpirationInDays.Value))
                {
                    return AuthenticationStatus.PasswordExpired;
                }
            }

            return AuthenticationStatus.Success;
        }

        private bool HasExpiredPassword(User user, int passwordExpirationInDays)
        {
            if (!user.ExpirePassword.GetValueOrDefault())
            {
                return false;
            }

            // If the value is 0 then password never expires
            if (passwordExpirationInDays == 0)
            {
                return false;
            }

            var currentUtcTime = DateTime.UtcNow;
            var hasExpiredPassword = user.LastPasswordChangeTimestamp.Value.AddDays(passwordExpirationInDays) <= currentUtcTime;

            return hasExpiredPassword;
        }
    }
}