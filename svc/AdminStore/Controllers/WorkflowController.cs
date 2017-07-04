using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Files;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Files;

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
        internal readonly PrivilegesManager _privilegesManager;

        public WorkflowController() : this(new WorkflowRepository(), new WorkflowService(), new ServiceLogRepository(), new SqlPrivilegesRepository())
        {
        }

        public WorkflowController(IWorkflowRepository workflowRepository, IWorkflowService workflowService, IServiceLogRepository log, IPrivilegesRepository privilegesRepository) : base(log)
        {
            _workflowService = workflowService;
            _workflowRepository = workflowRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
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
        [Route("import"), SessionRequired]
        [ResponseType(typeof(ImportWorkflowResult))]
        public async Task<IHttpActionResult> ImportWorkflowAsync()
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            Debug.Assert(session != null, "The session is null.");

            // The file name is specified in Content-Disposition header,
            // for example, Content-Disposition: workflow;filename=workflow.xml
            // The first parameter does not matter, can be workflow, file etc.
            // Required for messages.
            var fileName = Request.Content.Headers?.ContentDisposition?.FileName;

            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                IeWorkflow workflow;
                try
                {
                    workflow = DeserializeWorkflow(stream);
                }
                catch (Exception ex)
                {
                    var errorResult = new ImportWorkflowResult
                    {
                        ErrorMessage = ex.Message
                    };

                    var response = Request.CreateResponse(HttpStatusCode.BadRequest, errorResult);
                    return ResponseMessage(response);
                }

                _workflowService.FileRepository = GetFileRepository();
                var result = await _workflowService.ImportWorkflowAsync(workflow, fileName, session.UserId);

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
        [Route("import/errors"), SessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetImportWorkflowErrorsAsync(string guid)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            Debug.Assert(session != null, "The session is null.");

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
        /// <response code="403">User doesn’t have permission to view workflow.</response>
        /// <response code="404">Not Found. The workflow with the provided Id was not found.</response>
        /// </returns>
        [SessionRequired]
        [Route("{workflowId:int:min(1)}")]
        [ResponseType(typeof (SqlWorkflow))]
        public async Task<IHttpActionResult> GetWorkflow(int workflowId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var workflowDetails = await _workflowService.GetWorkflowDetailsAsync(workflowId);

            return Ok(workflowDetails);
        }



        public async Task<IHttpActionResult> GetWorkflows([FromUri] Pagination pagination, [FromUri] Sorting sorting = null, string search = null)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            pagination.Validate();

            var result = await _workflowRepository.GetWorkflows(pagination, sorting, search, UsersHelper.SortUsers);
            return Ok(result);

        }

        #region Private methods

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
                throw new BadRequestException(I18NHelper.FormatInvariant("Invalid workflow XML: {0}", ex.Message), ErrorCodes.InvalidWorkflowXml);
            }
        }

        #endregion

    }
}