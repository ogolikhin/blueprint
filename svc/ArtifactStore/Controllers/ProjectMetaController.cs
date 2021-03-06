﻿using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories.ProjectMeta;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ProjectMetaController : LoggableApiController
    {
        private readonly IProjectMetaRepository ProjectMetaRepository;

        public override string LogSource { get; } = "ArtifactStore.ProjectMeta";

        public ProjectMetaController() : this(new SqlProjectMetaRepository())
        {
        }

        public ProjectMetaController(IProjectMetaRepository projectMetaRepository) : base()
        {
            ProjectMetaRepository = projectMetaRepository;
        }

        public ProjectMetaController(IProjectMetaRepository projectMetaRepository, IServiceLogRepository log) : base(log)
        {
            ProjectMetaRepository = projectMetaRepository;
        }

        /// <summary>
        /// Get artifact, sub-artifact and property types of the project.
        /// </summary>
        /// <remarks>
        /// Returns artifact, sub-artifact and property types of the project with the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/meta/customtypes"), SessionRequired]
        [ActionName("GetProjectTypes")]
        public async Task<ProjectTypes> GetProjectTypesAsync(int projectId)
        {
            return await ProjectMetaRepository.GetCustomProjectTypesAsync(projectId, Session.UserId);
        }

        /// <summary>
        /// Get approval statuses of the project.
        /// </summary>
        /// <remarks>
        /// Returns all approval status types of the project with the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/meta/approvalstatus"), SessionRequired]
        [ActionName("GetProjectApprovalStatuses")]
        public Task<IEnumerable<ProjectApprovalStatus>> GetProjectApprovalStatusesAsync(int projectId)
        {
            return ProjectMetaRepository.GetApprovalStatusesAsync(projectId, Session.UserId);
        }
    }
}
