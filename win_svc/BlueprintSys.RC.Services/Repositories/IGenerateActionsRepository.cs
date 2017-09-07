using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Repositories
{
    public interface IGenerateActionsRepository : IActionHandlerServiceRepository
    {
        IJobsRepository JobsRepository { get; }

        ISqlItemTypeRepository ItemTypeRepository { get; }
    }
}
