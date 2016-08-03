using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel;

namespace Helper
{
    /// <summary>
    /// This class contains commonly used data sources for the NUnit TestCaseSource attribute.
    /// Usage example:  [Test, TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.AllArtifactTypesForOpenApiRestMethods))]
    /// </summary>
    public static class TestCaseSources
    {
        #region private variables

        private static readonly List<BaseArtifactType> _allArtifactTypesForOpenApiRestMethods = new List<BaseArtifactType>
        {
            BaseArtifactType.Actor,
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.Document,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Glossary,
            BaseArtifactType.PrimitiveFolder,
            BaseArtifactType.Process,
            BaseArtifactType.Storyboard,
            BaseArtifactType.TextualRequirement,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCase,
            BaseArtifactType.UseCaseDiagram
        };

        private static readonly List<BaseArtifactType> _allDiagramArtifactTypesForOpenApiRestMethods = new List<BaseArtifactType>
        {
            BaseArtifactType.BusinessProcess,
            BaseArtifactType.DomainDiagram,
            BaseArtifactType.GenericDiagram,
            BaseArtifactType.Storyboard,
            BaseArtifactType.UIMockup,
            BaseArtifactType.UseCaseDiagram
        };

        private static readonly List<HttpStatusCode> _allHttpErrorStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.BadRequest,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.NotFound,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.NotAcceptable,
            HttpStatusCode.Conflict,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable
        };

        #endregion private variables

        /// <summary>Returns a list of all possible artifact types that can be used by the OpenAPI REST methods.</summary>
        public static IReadOnlyCollection<BaseArtifactType> AllArtifactTypesForOpenApiRestMethods => _allArtifactTypesForOpenApiRestMethods.AsReadOnly();

        /// <summary>Returns a list of all possible diagram artifact types that can be used by the OpenAPI REST methods.</summary>
        public static IReadOnlyCollection<BaseArtifactType> AllDiagramArtifactTypesForOpenApiRestMethods => _allDiagramArtifactTypesForOpenApiRestMethods.AsReadOnly();

        /// <summary>Returns a list of all error (4xx & 5xx) HttpStatusCodes that Blueprint might return.</summary>
        public static IReadOnlyCollection<HttpStatusCode> AllHttpErrorStatusCodes => _allHttpErrorStatusCodes.AsReadOnly();
    }
}
