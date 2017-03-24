using System.Collections.Generic;
using System.Net;
using Model.Common.Enums;
using Model.NovaModel.Components.RapidReview;
using Model.StorytellerModel.Impl;

namespace Model.StorytellerModel
{

    public interface IStorytellerUserStory
    {
        /// <summary>
        /// The Artifact Id of the User Story
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the User Story
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Project Id of the User Story
        /// </summary>
        int ProjectId { get; set; }

        /// <summary>
        /// TypeId of the User Story
        /// </summary>
        int TypeId { get; set; }

        /// <summary>
        /// TypePrefix of the User Story
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// The Predefined Property Type of the User Story
        /// </summary>
        PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// List of System Properties of the User Story
        /// </summary>
        List<StorytellerProperty> SystemProperties { get; }

        /// <summary>
        /// List of Custom Properties of the User Story
        /// </summary>
        List<StorytellerProperty> CustomProperties { get; }

        /// <summary>
        /// ProcessTaskId of the User Story
        /// </summary>
        int ProcessTaskId { get; set; }

        /// <summary>
        /// Boolean flag indicating whether the User Story is being Created or Updated
        /// </summary>
        bool IsNew { get; set; }

        /// <summary>
        /// Updates Nonfunctional requirements property for user story.
        /// </summary>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="value">Text to update Nonfunctional requirements property.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only '200 OK' is expected.</param>
        /// <returns>Result of updating nonfunctional requrements.</returns>
        UpdateResult<ArtifactProperty> UpdateNonFunctionalRequirements(
            IUser user,
            string value,
            List<HttpStatusCode> expectedStatusCodes = null);
    }
}
