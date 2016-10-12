

namespace Model
{
    public interface IServiceErrorMessage
    {
        int ErrorCode { get; }
        string Message { get; }

        /// <summary>
        /// Tests whether the specified ServiceErrorMessage is equal to this one.
        /// </summary>
        /// <param name="expectedErrorMessage">The expected ServiceErrorMessage to compare against.</param>
        void AssertEquals(IServiceErrorMessage expectedErrorMessage);
    }
}
