using System.Threading.Tasks;

namespace HtmlLibrary
{
    public interface IMentionValidator
    {
        Task<bool> IsEmailBlocked(string email);
    }
}
