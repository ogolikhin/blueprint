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
        public GenerateUserStoriesActionRepository(string connectionString) : 
            this(new SqlConnectionWrapper(connectionString))
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper) : 
            this(connectionWrapper, new JobsRepository(connectionWrapper))
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper,
            IJobsRepository jobsRepository) : this(connectionWrapper,
                jobsRepository,
                new SqlUsersRepository(connectionWrapper))
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper,
            IJobsRepository jobsRepository, 
            IUsersRepository usersRepository) : 
            this(connectionWrapper, jobsRepository, 
                new SqlArtifactPermissionsRepository(connectionWrapper), 
                usersRepository)
        {
        }

        public GenerateUserStoriesActionRepository(ISqlConnectionWrapper connectionWrapper,
            IJobsRepository jobsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IUsersRepository usersRepository) : 
            base(connectionWrapper, artifactPermissionsRepository, usersRepository)
        {
            JobsRepository = jobsRepository;
        }

        public IJobsRepository JobsRepository { get; }
    }
}
