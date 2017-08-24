using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using AdminStore.Helpers;
using AdminStore.Models;
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
using ServiceLibrary.Models.Enums;
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
        [ResponseType(typeof(WorkflowDto))]
        public async Task<IHttpActionResult> GetWorkflow(int workflowId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var workflowDetails = await _workflowService.GetWorkflowDetailsAsync(workflowId);

            return Ok(workflowDetails);
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
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            pagination.Validate();

            var result = await _workflowRepository.GetWorkflows(pagination, sorting, search, SortingHelper.SortWorkflows);

            return Ok(result);
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
        /// Update workflow's status
        /// </summary>
        /// <param name="workflowId">Workflow identity</param>
        /// <param name="statusUpdate">StatusUpdate model</param>
        /// <remarks>
        /// Returns Ok result.
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
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("{workflowId:int:min(1)}/status")]
        public async Task<IHttpActionResult> UpdateStatus(int workflowId, [FromBody] StatusUpdate statusUpdate)
        {
            if (statusUpdate == null)
            {
                throw new BadRequestException(ErrorMessages.WorkflowModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);
            await _workflowService.UpdateWorkflowStatusAsync(statusUpdate, workflowId, Session.UserId);

            return Ok();
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
        [ResponseType(typeof (string))]
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
                try
                {
                    ValidateWorkflowXmlAgainstXsd(stream);
                    workflow = DeserializeWorkflow(stream);
                }
                catch (Exception ex)
                {
                    var errorResult = new ImportWorkflowResult
                    {
                        ErrorMessage = I18NHelper.FormatInvariant(InvalidXmlErrorMessageTemplate, fileName, ex.Message)
                    };

                    var response = Request.CreateResponse(HttpStatusCode.BadRequest, errorResult);

                    return ResponseMessage(response);
                }

                _workflowService.FileRepository = GetFileRepository();

                var result = workflowId == null
                    ? await _workflowService.ImportWorkflowAsync(workflow, fileName, session.UserId)
                    : await _workflowService.UpdateWorkflowViaImport(workflowId.Value, workflow, fileName, session.UserId);

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
