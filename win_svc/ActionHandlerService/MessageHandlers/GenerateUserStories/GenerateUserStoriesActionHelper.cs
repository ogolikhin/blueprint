using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateUserStories
{
    public class GenerateUserStoriesActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var generateUserStoriesMessage = actionMessage as GenerateUserStoriesMessage;
            if (generateUserStoriesMessage == null)
            {
                return await Task.FromResult(false);
            }
            return await UserStoryGenerationRepository.GenerateUserStories(generateUserStoriesMessage.ProjectId, 
                generateUserStoriesMessage.ArtifactId,
                generateUserStoriesMessage.ProjectName,
                generateUserStoriesMessage.UserName,
                generateUserStoriesMessage.UserId,
                generateUserStoriesMessage.BaseHostUri);
        }
    }
}
