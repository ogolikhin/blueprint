using System;

namespace Model.Impl
{
    public class BlueprintToken : IBlueprintToken
    {
        private string _token;

        public string TokenString { get { return _token; } }

        /// <summary>
        /// Constructor that sets the token to the specified token string.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <exception cref="ArgumentException">If the specified token is invalid.</exception>
        public BlueprintToken(string token)
        {
            if ((token != null) && token.StartsWith("BlueprintToken"))
            {
                _token = token;
            }
            else
            {
                throw new ArgumentException("The specified token is not a Blueprint token!");
            }
        }
    }
}
