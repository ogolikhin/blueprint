using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.Repositories
{
    public class GenerateActionRepository : ActionHandlerServiceRepository, IGenerateActionsRepository
    {
        public GenerateActionRepository(string connectionString) : 
            this(new SqlConnectionWrapper(connectionString))
        {
        }

        public GenerateActionRepository(ISqlConnectionWrapper srvConnectionWrapper) : 
            this(srvConnectionWrapper, 
                new SqlArtifactPermissionsRepository(srvConnectionWrapper),
                new SqlUsersRepository(srvConnectionWrapper))
        {
        }

        public GenerateActionRepository(ISqlConnectionWrapper srvConnectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IUsersRepository usersRepository) :
            this(srvConnectionWrapper,
                artifactPermissionsRepository,
                usersRepository,
                new JobsRepository(srvConnectionWrapper,
                    new SqlArtifactRepository(srvConnectionWrapper,
                        new SqlItemInfoRepository(srvConnectionWrapper),
                        artifactPermissionsRepository),
                    artifactPermissionsRepository, usersRepository),
                new SqlItemTypeRepository(srvConnectionWrapper))
        {
        }

        public GenerateActionRepository(ISqlConnectionWrapper srvConnectionWrapper,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IUsersRepository usersRepository,
            IJobsRepository jobsRepository,
            ISqlItemTypeRepository sqlItemTypeRepository) : 
            base(srvConnectionWrapper, 
                artifactPermissionsRepository, 
                usersRepository)
        {
            JobsRepository = jobsRepository;
            ItemTypeRepository = sqlItemTypeRepository;
        }

        public IJobsRepository JobsRepository { get; }
        public ISqlItemTypeRepository ItemTypeRepository { get; }
    }
}
