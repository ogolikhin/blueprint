using AdminStore.Helpers;
using AdminStore.Helpers.Workflow;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Files;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("workflow")]
    [BaseExceptionFilter]
    public class WorkflowController : LoggableApiController
    {
        public override string LogSource => "AdminStore.Workflow";

        private readonly IWorkflowService _workflowService;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly PrivilegesManager _privilegesManager;

        private const string InvalidXmlErrorMessageTemplate = "There was an error uploading {0}. The supplied XML is not valid.  Please edit your file and upload again. \r\n {1}";

        public WorkflowController() : this(new WorkflowRepository(), new WorkflowService(), new ServiceLogRepository(), new SqlPrivilegesRepository())
        {
        }

        public WorkflowController(IWorkflowRepository workflowRepository, IWorkflowService workflowService, IServiceLogRepository log,
            IPrivilegesRepository privilegesRepository) : base(log)
        {
            _workflowService = workflowService;
            _workflowRepository = workflowRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Assign projects and standart artifact types to the workflow
        /// </summary>
        /// <param name="workFlowid"></param>
        /// <param name="workflowAssign"></param>
        /// <response code="200">OK. The projects and artifact types were assigned to the workflow.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to assign projects and artifact types</response>
        /// <response code="404">Not Found. The workflow with current id were not found.</response>
        /// <response code="409">Conflict. The workflow with the current id is active.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns></returns>
        [HttpPost]
        [Route("{workflowId:int:min(1)}/assign"), SessionRequired]
        [ResponseType(typeof(AssignProjectsResult))]
        public async Task<IHttpActionResult> AssignProjectsAndArtifactTypesToWorkflow(int workFlowid, [FromBody] WorkflowAssignScope workflowAssign)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (workflowAssign == null)
            {
                throw new BadRequestException(ErrorMessages.AssignMemberScopeEmpty, ErrorCodes.BadRequest);
            }

            if (workflowAssign.IsEmpty())
            {
                return Ok(AssignProjectsResult.Empty);
            }

            var result = await _workflowRepository.AssignProjectsAndArtifactTypesToWorkflow(workFlowid, workflowAssign);

            return Ok(result);
        }


        /// <summary>
        /// Sync the existing project artifact types assignments for the specified project: delete absent and add the new ones.
        /// </summary>
        /// <param name="workflowId">Id of chosen Workflow</param>
        /// <param name="projectId">Id of chosen Project</param>
        /// <param name="scope">scope of artifact types assignment</param>
        /// <response code="200">OK. The artifact types were assigned to the workflow and to the project.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to assign projects and artifact types</response>
        /// <response code="404">Not Found. The workflow or project with current id were not found.</response>
        /// <response code="409">Conflict. The workflow with the current id is active or workflow's project doesn't have any artifact types</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <returns>Returns TotalAdded and TotalDeleted. TotalAdded is a quantity of added artifact types, TotalDeleted is a quantity of removed artifact types</returns>
        [HttpPost]
        [Route("{workflowId:int:min(1)}/project/{projectId:int:min(1)}/assign"), SessionRequired]
        [ResponseType(typeof(SyncResult))]
        public async Task<IHttpActionResult> AssignArtifactTypesToProjectInWorkflow(int workflowId, int projectId, [FromBody] OperationScope scope)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (scope == null) // just check this, for empty list of ids we should get custom message from SP
                 throw new BadRequestException(ErrorMessages.ArtifactTypeIdsNotValid, ErrorCodes.BadRequest);

            var result = await _workflowRepository.AssignArtifactTypesToProjectInWorkflow(workflowId, projectId, scope);

            return Ok(result);
        }

        /// <summary>
        /// Import Workflow
        /// </summary>
        /// <remarks>
        /// Imports a workflow specified in the uploaded XML file. The file name is specified in Content-Disposition header,
        /// for example, Content-Disposition: workflow;filename=workflow.xml. The first parameter does not matter, can be workflow, file etc.
        /// The file name is required for messages.
        ///
        /// </remarks>
        /// <response code="200">OK. The workflow is imported successfully from the uploaded XML file. The response contains Id of the new workflow.</response>
        /// <response code="400">Bad Request.
        /// * The workflow XML format is invalid.
        /// * The workflow model validation failed. The validation errors can be retrieved with
        ///   'Get Import Workflow Errors' call by the GUID returned in the response of this call.
        /// </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden.
        /// * The user does not permissions to import workflows (currently the user is not an instance administrator).
        /// * The product does not have a license for the Workflow feature.</response>
        /// <response code="409">Conflict. The specified workflow conflicts with existing workflows or some specified elements,
        ///   e.g. projects, artifact types etc., are not found.
        ///   The errors can be retrieved with 'Get Import Workflow Errors' call
        ///   by the GUID returned in the response of this call.
        /// </response>
        [HttpPost]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("import"), SessionRequired]
        [ResponseType(typeof(ImportWorkflowResult))]
        public async Task<IHttpActionResult> ImportWorkflowAsync()
        {
            return await UploadWorkflowAsync();
        }

        /// <summary>
        /// Get Import Workflow Errors
        /// </summary>
        /// <remarks>
        /// Returns workflow import errors as plain text (do not confuse with the response format that can be XML or JSON) for the specified GUID.
        /// The GUID is returned in the response of 'Import Workflow' call.
        /// </remarks>
        /// <param name="guid">The GUID of the workflow import errors.</param>
        /// <response code="200">OK. The requested workflow import errors are successfully returned.</response>
        /// <response code="400">Bad Request. The GUID format is invalid.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden.
        /// * The user does not permissions to import workflows (currently the user is not an instance administrator).
        /// * The product does not have a license for the Workflow feature.</response>
        /// <response code="404">Not Found. The workflow import errors are not found for the specified GUID.</response>
        [HttpGet, NoCache]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("import/errors"), SessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetImportWorkflowErrorsAsync(string guid)
        {
            var session = Session;
            Debug.Assert(session != null, "The session is null.");

            await _privilegesManager.Demand(session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            _workflowService.FileRepository = GetFileRepository();
            var errors = await _workflowService.GetImportWorkflowErrorsAsync(guid, session.UserId);

            return Ok(errors);
        }


        /// <summary>
        /// Get workflow details by workflow identifier
        /// </summary>
        /// <param name="workflowId">Workflow's identity</param>
        /// <returns>
        /// <response code="200">OK. Returns the specified workflow.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">User doesn’t have permission to view workflow or Workflow license is not available.</response>
        /// <response code="404">Not Found. The workflow with the provided Id was not found.</response>
        /// </returns>
        [SessionRequired]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("{workflowId:int:min(1)}")]
        [ResponseType(typeof(WorkflowDetailsDto))]
        public async Task<IHttpActionResult> GetWorkflow(int workflowId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var workflowDetails = await _workflowService.GetWorkflowDetailsAsync(workflowId);

            return Ok(workflowDetails);
        }

        /// <summary>
        /// Get not assigned projects for Workflow by workflowId and by folderId
        /// </summary>
        /// <param name="workFlowId"></param>
        /// <param name="folderId"></param>
        /// <response code="200">OK. List not assigned projects for Workflow</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">User doesn’t have permission to view projects not assigned to workflow.</response>
        /// <response code="404">Not Found. The workflow with workflowId or folder with folderId were not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        ///
        [HttpGet, NoCache]
        [Route("{workflowId:int:min(1)}/folders/{folderId:int:min(1)}/availablechildren"), SessionRequired]
        [ResponseType(typeof(List<InstanceItem>))]
        public async Task<IHttpActionResult> GetWorkflowAvailableProjects(int workFlowId, int folderId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var availiableProjects = await _workflowRepository.GetWorkflowAvailableProjectsAsync(workFlowId, folderId);

            return Ok(availiableProjects);
        }

        /// <summary>
        /// Get list of project artifact types assigned to a workflowId
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="pagination">Limit and offset values to query workflows</param>
        /// <param name="search">(optional) Search query parameter</param>
        /// <response code="200">OK. List of assigned project artifact types for Workflow</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">User doesn’t have permission to access project artifact types assigned to workflow.</response>
        /// <response code="404">Not Found. Project artifact types with workflowId were not found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        ///
        [HttpGet, NoCache]
        [Route("{workflowId:int:min(1)}/projects"), SessionRequired]
        [ResponseType(typeof(QueryResult<WorkflowProjectArtifactTypesDto>))]
        public async Task<IHttpActionResult> GetProjectArtifactTypesAssignedToWorkflowAsync(int workflowId, [FromUri] Pagination pagination, string search = null)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var availiableProjects = await _workflowRepository.GetProjectArtifactTypesAssignedtoWorkflowAsync(workflowId, pagination,
                 search);

            return Ok(availiableProjects);
        }

        /// <summary>
        /// Get workflows list according to the input parameters
        /// </summary>
        /// <param name="pagination">Limit and offset values to query workflows</param>
        /// <param name="sorting">(optional) Sort and its order</param>
        /// <param name="search">(optional) Search query parameter</param>
        /// <response code="200">OK if admin user session exists and user is permitted to list workflows</response>
        /// <response code="400">BadRequest if pagination object didn't provide</response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to get workflows list or Workflow license is not available.</response>
        [SessionRequired]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("")]
        [ResponseType(typeof(QueryResult<WorkflowDto>))]
        public async Task<IHttpActionResult> GetWorkflows([FromUri] Pagination pagination, [FromUri] Sorting sorting = null, string search = null)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var result = await _workflowRepository.GetWorkflows(pagination, sorting, search, SortingHelper.SortWorkflows);

            return Ok(result);
        }

        /// <summary>
        /// Search projects by name
        /// </summary>
        /// <param name="workflowId">workflow's id</param>
        /// <param name="search">name of the project (or wildcard selection pattern).</param>
        /// <response code="200">OK.</response>
        /// <response code="400">BadRequest. Search parameter is not valid</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to search projects</response>
        /// <response code="404">Not Found. Workflow with workflowId was not found.</response>
        [HttpGet, NoCache]
        [Route("{workflowId:int:min(1)}/projectsearch"), SessionRequired]
        [ResponseType(typeof(IEnumerable<WorkflowProjectSearch>))]
        public async Task<IHttpActionResult> SearchProjectsByName(int workflowId, string search = null)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            SearchFieldValidator.Validate(search);

            var result = await _workflowRepository.SearchProjectsByName(workflowId, search);

            return Ok(result);
        }

        /// <summary>
        /// Create workflow with the name that is passed in the parameters and optional description
        /// </summary>
        /// <param name="createWorkflowDto">Workflow name (required parameter) and workflow description (optional)</param>
        /// <response code="400">BadRequest if model is malformed or workflow name less then 4 or greater then 65 characters. Also if workflow description greater then 400 characters.</response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to create a workflow.</response>
        /// <response code="409">Conflict. Workflow name with the such a name already exists.</response>
        [SessionRequired]
        [FeatureActivation(FeatureTypes.Workflow)]
        [Route("create")]
        [HttpPost]
        [ResponseType(typeof(int))]
        public async Task<HttpResponseMessage> CreateWorkflow([FromBody]CreateWorkflowDto createWorkflowDto)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            if (createWorkflowDto != null)
            {
                createWorkflowDto.Name = createWorkflowDto.Name?.Trim() ?? createWorkflowDto.Name;
                createWorkflowDto.Description = createWorkflowDto.Description?.Trim() ?? createWorkflowDto.Description;
            }
            else
            {
                throw new BadRequestException(ErrorMessages.CreateWorkfloModelIsEmpty, ErrorCodes.BadRequest);
            }

            createWorkflowDto.Validate();

            var result = await _workflowService.CreateWorkflow(createWorkflowDto.Name, createWorkflowDto.Description, Session.UserId);
            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Delete workflow/workflows from system
        /// </summary>
        /// <param name="scope">list of user ids and selectAll flag</param>
        /// <param name="search">search filter</param>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to delete workflows or Workflow license is not available.</response>
        /// <returns></returns>
        [HttpPost]
        [FeatureActivation(FeatureTypes.Workflow)]
        [SessionRequired]
        [Route("delete")]
        [ResponseType(typeof(DeleteResult))]
        public async Task<IHttpActionResult> DeleteWorkflows([FromBody]OperationScope scope, string search = null)
        {
            SearchFieldValidator.Validate(search);

            if (scope == null)
            {
                return BadRequest(ErrorMessages.InvalidDeleteWorkflowsParameters);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            var result = await _workflowService.DeleteWorkflows(scope, search, Session.UserId);

            return Ok(new DeleteResult { TotalDeleted = result });
        }

        /// <summary>
        /// Delete the specified projects workflow assignment
        /// </summary>
        /// <param name="workflowId">workflow's id</param>
        /// <param name="scope">list of projects ids, selectAll flag</param>
        /// <param name="search">search filter</param>
        /// <response code="200">OK. Count of deleted projects workflow assignment.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to remove projects workflow assignment</response>
        /// <response code="404">NotFound. if the workflow with workId doesn’t exists or removed from the system.</response>
        /// <response code="409">Conflict. The current workflow from the request is active (should not be active).</response>
        [HttpPost]
        [SessionRequired]
        [Route("{workflowId:int:min(1)}/unassign")]
        [ResponseType(typeof(DeleteResult))]
        public async Task<IHttpActionResult> UnassignProjectsAndArtifactTypesFromWorkflowAsync(int workflowId, [FromBody] OperationScope scope, string search = null)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            SearchFieldValidator.Validate(search);

            if (scope == null)
            {
                throw new BadRequestException(ErrorMessages.UnassignMemberScopeEmpty, ErrorCodes.BadRequest);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            var result = await _workflowRepository.UnassignProjectsAndArtifactTypesFromWorkflowAsync(workflowId, scope, search);

            return Ok(new DeleteResult { TotalDeleted = result });
        }
        /// <summary>
        /// Copy workflow with specified workflowId
        /// </summary>
        /// <param name="workflowId">workflow's id</param>
        /// <param name="name">workflow's name</param>
        /// <response code="200">OK. Copy of a workflow was successfuly done.</response>
        /// <response code="400">BadRequest. name parameter is invalid. </response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to copy workflow</response>
        /// <response code="404">NotFound. if the workflow with workflowId doesn’t exists or removed from the system.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [SessionRequired]
        [Route("copy/{workflowId:int:min(1)}")]
        [ResponseType(typeof(int))]
        public async Task<IHttpActionResult> CopyWorkflowAsync(int workflowId, [FromUri] string name)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            name = name?.Trim(); // remove whitespaces

            // create a DTO for name validation only
            CreateWorkflowDto copyWorkflowDto = new CreateWorkflowDto()
            {
                Name = name,
                Description = string.Empty
            };

            copyWorkflowDto.Validate();

            var result = await _workflowRepository.CopyWorkflowAsync(workflowId, Session.UserId, name);

            return Ok(result);
        }
        /// <summary>
        /// Update workflow's status
        /// </summary>
        /// <param name="workflowId">Workflow identity</param>
        /// <param name="statusUpdate">StatusUpdate model</param>
        /// <remarks>
        /// Returns versionId.
        /// </remarks>
        /// <response code="200">Ok. Workflow is updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the workflow or Workflow license is not available.</response>
        /// <response code="404">NotFound. The workflow with the current id doesn’t exist or removed from the system.</response>
        /// <response code="409">Conflict. The current version of the workflow from the request doesn’t match the current version in DB.</response>
        [HttpPut]
        [FeatureActivation(FeatureTypes.Workflow)]
        [SessionRequired]
        [ResponseType(typeof(int))]
        [Route("{workflowId:int:min(1)}/status")]
        public async Task<IHttpActionResult> UpdateStatus(int workflowId, [FromBody] StatusUpdate statusUpdate)
        {
            if (statusUpdate == null)
            {
                throw new BadRequestException(ErrorMessages.WorkflowModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            var versionId = await _workflowService.UpdateWorkflowStatusAsync(statusUpdate, workflowId, Session.UserId);

            return Ok(versionId);
        }

        /// <summary>
        /// Update workflow
        /// </summary>
        /// <param name="workflowId">Workflow identity</param>
        /// <param name="workflowDto">WorkflowDto model</param>
        /// <response code="204">NoContent. Workflow is updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the workflow.</response>
        /// <response code="404">NotFound. The workflow with the current id doesn’t exist or removed from the system.</response>
        /// <response code="409">Conflict. The workflow with the current id is active,
        /// or workflow without project/artifact type assignments cannot be activated,
        /// or there is at least one project-artifact type assigned to the current workflow which is also assigned to another active workflow.
        /// or if we try to make an update with  the name which is already exist</response>
        [HttpPut]
        [FeatureActivation(FeatureTypes.Workflow)]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("{workflowId:int:min(1)}")]
        public async Task<HttpResponseMessage> UpdateWorkflow(int workflowId, [FromBody] UpdateWorkflowDto workflowDto)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (workflowDto == null)
            {
                throw new BadRequestException(ErrorMessages.WorkflowModelIsEmpty, ErrorCodes.BadRequest);
            }

            workflowDto.Validate();

            await _workflowService.UpdateWorkflowAsync(workflowDto, workflowId, Session.UserId);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Export Workflow
        /// </summary>
        /// <param name="workflowId">Workflow identity</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">Ok. Workflow is exported.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the workflow or Workflow license is not available.</response>
        /// <response code="404">NotFound. The workflow with the current id doesn’t exist or removed from the system.</response>
        [SessionRequired]
        [FeatureActivation(FeatureTypes.Workflow)]
        [HttpGet, NoCache]
        [ResponseType(typeof(string))]
        [Route("export/{workflowId:int:min(1)}")]
        public async Task<IHttpActionResult> ExportWorkflow(int workflowId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            var ieWorkflow = await _workflowService.GetWorkflowExportAsync(workflowId);
            var workflowXml = SerializationHelper.ToXml(ieWorkflow, true);
            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(workflowXml);

            response.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"workflow{workflowId}.xml"
                };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

            return ResponseMessage(response);
        }

        /// <summary>
        /// Update Workflow via the import
        /// </summary>
        /// <param name="workflowId">Workflow identity</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. The workflow is updated successfully from the uploaded XML file.</response>
        /// <response code="400">Bad Request.
        /// * The workflow XML format is invalid.
        /// * The workflow model validation failed. The validation errors can be retrieved with
        ///   'Get Import Workflow Errors' call by the GUID returned in the response of this call.
        /// </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden.
        /// * The user does not permissions to import workflows (currently the user is not an instance administrator).
        /// * The product does not have a license for the Workflow feature.</response>
        /// <response code="409">Conflict. The specified workflow conflicts with existing workflows or some specified elements,
        ///   e.g. projects, artifact types etc., are not found.
        ///   The errors can be retrieved with 'Get Import Workflow Errors' call
        ///   by the GUID returned in the response of this call.
        /// </response>
        [SessionRequired]
        [FeatureActivation(FeatureTypes.Workflow)]
        [HttpPut]
        [ResponseType(typeof(ImportWorkflowResult))]
        [Route("update/{workflowId:int:min(1)}")]
        public async Task<IHttpActionResult> UpdateWorkflowViaImport(int workflowId)
        {
            return await UploadWorkflowAsync(workflowId);
        }

        #region Private methods

        // Upload means Import (Create) or Update
        private async Task<IHttpActionResult> UploadWorkflowAsync(int? workflowId = null)
        {
            var session = Session;
            Debug.Assert(session != null, "The session is null.");

            await _privilegesManager.Demand(session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            // The file name is specified in Content-Disposition header,
            // for example, Content-Disposition: workflow;filename=workflow.xml
            // The first parameter does not matter, can be workflow, file etc.
            // Required for messages.
            var fileName = Request.Content.Headers?.ContentDisposition?.FileName;
            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                IeWorkflow workflow;
                string xmlSerError = null;
                try
                {
                    ValidateWorkflowXmlAgainstXsd(stream);
                    workflow = DeserializeWorkflow(stream);
                }
                catch (Exception ex)
                {
                    workflow = null;
                    xmlSerError = ex.Message;
                }

                _workflowService.FileRepository = GetFileRepository();

                var result = workflowId == null
                    ? await _workflowService.ImportWorkflowAsync(workflow, fileName, session.UserId, xmlSerError)
                    : await _workflowService.UpdateWorkflowViaImport(workflowId.Value, workflow, fileName, session.UserId, xmlSerError);

                switch (result.ResultCode)
                {
                    case ImportWorkflowResultCodes.Ok:
                        return Ok(result);
                    case ImportWorkflowResultCodes.InvalidModel:
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, result));
                    case ImportWorkflowResultCodes.Conflict:
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, result));
                    default:
                        // Should never happen.
                        return InternalServerError(new Exception("Unknown error."));
                }
            }
        }

        // Validate xml of IeWorkflow against xml schema.
        // To generate xml schema use: xsd.exe AdminStore.dll /t:IeWorkflow
        internal static void ValidateWorkflowXmlAgainstXsd(Stream stream)
        {
            var xml = new XmlDocument();
            xml.Load(stream);
            var xsdStream = Assembly.GetExecutingAssembly().
                GetManifestResourceStream("AdminStore.Models.Workflow.IeWorkflow.xsd");
            xml.Schemas.Add(null, XmlReader.Create(xsdStream));
            xml.Validate(null);
        }

        private IFileRepository GetFileRepository()
        {
            var session = ServerHelper.GetSession(Request);
            var baseUri = WebApiConfig.FileStore != null ? new Uri(WebApiConfig.FileStore) : Request.RequestUri;

            return new FileRepository(new FileHttpWebClient(baseUri, Session.Convert(session.SessionId)));
        }

        private static IeWorkflow DeserializeWorkflow(Stream workflowContent)
        {
            try
            {
                return SerializationHelper.FromXml<IeWorkflow>(workflowContent);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message, ErrorCodes.InvalidWorkflowXml);
            }
        }

        #endregion
    }
}
