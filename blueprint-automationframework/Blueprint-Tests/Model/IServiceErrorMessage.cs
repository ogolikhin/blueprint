

namespace Model
{
    public interface IServiceErrorMessage
    {
        int ErrorCode { get; }
        string Message { get; }

        bool Equals(IServiceErrorMessage errorMessage);
    }
}
