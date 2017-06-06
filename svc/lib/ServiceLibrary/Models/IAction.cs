using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
   public  interface IAction
    {
        Task<bool> Execute();
    }

    public class EmailAction : IAction
    {
        public async Task<bool> Execute()
        {
            return await Task.FromResult(true);
        }
    }
}
