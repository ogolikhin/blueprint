using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class CollectionsController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Collections";

        private readonly ICollectionsRepository _collectionsRepository;

        private readonly PrivilegesManager _privilegesManager;

        public CollectionsController() : this
            (
                new CollectionsRepository(),
                new SqlPrivilegesRepository())
        {
        }

        public CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository)
        {
            _collectionsRepository = collectionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        public CollectionsController
        (
            ICollectionsRepository collectionsRepository,
            IPrivilegesRepository privilegesRepository,
            IServiceLogRepository log) : base(log)
        {
            _collectionsRepository = collectionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }
    }
}