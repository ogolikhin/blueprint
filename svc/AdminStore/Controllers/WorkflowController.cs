using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
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
        public async Task<ImportWorkflowResult> ImportWorkflowAsync()
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            Debug.Assert(session != null, "The session is null.");

            // The file name is specified in Content-Disposition header,
            // for example, Content-Disposition: workflow;filename=workflow.xml
            // The first parameter does not matter, can be workflow, file etc.
            // TODO: Required for messages
            var fileName = Request.Content.Headers?.ContentDisposition?.FileName;

            using (var stream = await Request.Content.ReadAsStreamAsync())
            {
                var workflow = DeserializeWorkflow(stream);
                return await _workflowRepository.ImportWorflowAsync(workflow, session.UserId);
            }
        }

        [HttpGet, NoCache]
        [Route("import/errors"), SessionRequired]
        public async Task<string> GetImportWorkflowErrorsAsync(string guid)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            Debug.Assert(session != null, "The session is null.");

            return await _workflowRepository.GetImportWorkflowErrorsAsync(guid, session.UserId);
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