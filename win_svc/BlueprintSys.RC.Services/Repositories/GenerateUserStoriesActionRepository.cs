using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Repositories
{
    public interface IGenerateUserStoriesRepository : IActionHandlerServiceRepository
    {
        IJobsRepository JobsRepository { get; }
    }

    public class GenerateUserStoriesActionRepository : ActionHandlerServiceRepository, IGenerateUserStoriesRepository
    {
        public GenerateUserStoriesActionRepository(string connectionString) : this(new SqlConnectionWrapper(connectionString))
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper) : this(connectionWrapper, new JobsRepository(connectionWrapper))
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper,
            IJobsRepository jobsRepository) : base(connectionWrapper, new SqlArtifactPermissionsRepository(connectionWrapper))
        {
            JobsRepository = jobsRepository;
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper,
            IJobsRepository jobsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository) : 
            base(connectionWrapper, artifactPermissionsRepository)
        {
            JobsRepository = jobsRepository;
        }

        public IJobsRepository JobsRepository { get; }
    }
}
