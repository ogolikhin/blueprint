using System.Collections.Generic;
using Common;
using NUnit.Framework;

namespace Helper
{
    public static class CommonServiceHelper
    {
        /// <summary>
        /// Verifies that the JSON content returned by a 'GET /status' call has the expected fields.
        /// </summary>
        /// <param name="content">The content returned from a GET /status call.</param>
        /// <exception cref="AssertionException">If any expected fields are not found.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]   // The first assert already validates for null.
        public static void ValidateStatusResponseContent(string content)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(content), "GET /status returned no content!");

            Logger.WriteDebug("GET /status returned: '{0}'", content);

            var stringsToFind = new List<string> { "ServiceName", "AccessInfo", "AssemblyFileVersion", "NoErrors", "Errors", "StatusResponses", "AccessControlEndpoint", "ConfigControlEndpoint" };

            foreach (string tag in stringsToFind)
            {
                Assert.That(content.Contains(tag), "The content returned from GET /status should contain '{0}'!", tag);
            }
        }
    }
}
