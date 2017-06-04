using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public interface ITrigger
    {
        int Id { get; set; }

        string Name { get; set; }
        
        string Description { get; set; }
    }

    public interface ITriggerExecutor<in T, TK>
    {
        Task<TK> Execute(T input);
    }

    public class PropertyChangeTrigger : ITriggerExecutor<Artifact, Boolean>
    {
        public async Task<bool> Execute(Artifact input)
        {
            return await Task.FromResult(true);
        }
    }
}
