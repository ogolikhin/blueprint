using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model.ArtifactModel
{
    #region Enums

    /// <summary>
    /// These are all the base artifact types (including some that cannot be created by OpenAPI).
    /// Full list available at:  blueprint-current/Source/BluePrintSys.RC.Service.Business/Models/Api/BaseArtifactTypes.cs
    /// </summary>
    public enum BaseArtifactType
    {
        /// <summary>Not used by OpenAPI.</summary>
        Undefined = 0,
        PrimitiveFolder = 1,
        Glossary = 2,
        TextualRequirement = 3,
        BusinessProcess = 4,    // It should be BusinessProcessDiagram, but development code is missing the 'Diagram'!
        Actor = 5,
        UseCase = 6,
        /// <summary>Not used by OpenAPI.</summary>
        DataElement = 7,
        UIMockup = 8,
        GenericDiagram = 9,
        Document = 10,
        Storyboard = 11,
        DomainDiagram = 12,
        UseCaseDiagram = 13,
        /// <summary>Not used by OpenAPI.</summary>
        Baseline = 14,
        /// <summary>Not used by OpenAPI.</summary>
        BaselineFolder = 15,
        ArtifactBaseline = 16,
        ArtifactReviewPackage = 17,
        Process     // This doesn't exist in BluePrintSys.RC.Service.Business/Models/Api/BaseArtifactTypes.cs.
    }

    #endregion Enums

    public interface IArtifactBase : IArtifactObservable, IDeepCopyable<IArtifactBase>
    {
        #region Properties

        /// <summary>
        /// Set this to true if you want the Delete method to also delete child artifacts.
        /// Default is false.
        /// </summary>
        bool ShouldDeleteChildren { get; set; }
        IUser LockOwner { get; set; }
        string Address { get; set; }
        IUser CreatedBy { get; set; }
        bool IsPublished { get; set; }
        bool IsSaved { get; set; }
        bool IsMarkedForDeletion { get; set; }
        bool IsDeleted { get; set; }
        IProject Project { get; set; }

        // XXX: These 3 properties don't appear to be set anywhere.
        bool AreTracesReadOnly { get; set; }
        bool AreAttachmentsReadOnly { get; set; }
        bool AreDocumentReferencesReadOnly { get; set; }

        #region Serialized JSON Properties

        BaseArtifactType BaseArtifactType { get; set; }     // OpenAPI-Add-Get
        int Id { get; set; }                                // OpenAPI-Add-Get
        string Name { get; set; }                           // OpenAPI-Add-Get
        int ProjectId { get; set; }                         // OpenAPI-Add-Get
        int Version { get; set; }                           // OpenAPI-Add-Get
        int ParentId { get; set; }                          // OpenAPI-Add-Get
        /// <summary>
        /// This is a URL link to this artifact.
        /// Ex. BlueprintUrl=http://silver02.blueprintsys.net/Web/#/Storyteller/5816
        /// </summary>
        Uri BlueprintUrl { get; set; }                      // OpenAPI-Get
        int ArtifactTypeId { get; set; }                    // OpenAPI-Add-Get
        string ArtifactTypeName { get; set; }               // OpenAPI-Add-Get
        ArtifactStatus Status { get; set; }                 // OpenAPI-Add

        List<OpenApiProperty> Properties { get; }           // OpenAPI-Add-Get

        #endregion Serialized JSON Properties
        #endregion Properties

        /// <summary>
        /// Delete the artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="user">(optional) The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        List<DeleteArtifactResult> Delete(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool? deleteChildren = null);

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation.
        /// (Runs:  svc/shared/navigation/{id1}/{id2}...)
        /// </summary>
        /// <param name="user">The user credentials for breadcrumb navigation</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The List of ArtifactReferences after the get navigation call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<ArtifactReference> GetNavigation(IUser user, List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void Publish(IUser user = null, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
