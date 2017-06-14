using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("workflow")]
    [BaseExceptionFilter]
    public class WorkflowController : LoggableApiController
    {
        public override string LogSource => "AdminStore.Workflow";

        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowController() : this(new WorkflowRepository(), new ServiceLogRepository())
        {
        }

        public WorkflowController(IWorkflowRepository workflowRepository, IServiceLogRepository log) : base(log)
        {
            _workflowRepository = workflowRepository;
        }

        [HttpPost]
        [Route("import"), SessionRequired]
        [ResponseType(typeof(ServiceStatus))]
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
                var result = await _workflowRepository.ImportWorkflowAsync(workflow, fileName, session.UserId);

                switch (result.ResultCode)
                {
                    case ImportWorkflowResultCodes.Ok:
                        return Ok(result);
                    case ImportWorkflowResultCodes.InvalidModel:
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, result));
                    case ImportWorkflowResultCodes.Conflict:
                        var response = Request.CreateResponse(HttpStatusCode.BadRequest, result);
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.Conflict, result));
                    default:
                        // Should never happen.
                        return InternalServerError(new Exception("Unknown error."));
                }
            }
        }

        [HttpGet, NoCache]
        [Route("import/errors"), SessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetImportWorkflowErrorsAsync(string guid)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            Debug.Assert(session != null, "The session is null.");

            var errors = await _workflowRepository.GetImportWorkflowErrorsAsync(guid, session.UserId);
            return Ok(errors);
        }

        #region Private methods

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