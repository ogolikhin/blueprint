﻿using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model.ArtifactModel
{
    #region Enums

    public enum ItemTypePredefined
    {
        None = 0,
        Project = 4097,
        Baseline = 4098,
        Glossary = 4099,
        TextualRequirement = 4101,
        PrimitiveFolder = 4102,
        BusinessProcess = 4103,
        Actor = 4104,
        UseCase = 4105,
        DataElement = 4106,
        UIMockup = 4107,
        GenericDiagram = 4108,
        Document = 4110,
        Storyboard = 4111,
        DomainDiagram = 4112,
        UseCaseDiagram = 4113,
        Process = 4114,
        BaselineFolder = 4353,
        ArtifactBaseline = 4354,
        ArtifactReviewPackage = 4355,
        GDConnector = 8193,
        GDShape = 8194,
        BPConnector = 8195,
        PreCondition = 8196,
        PostCondition = 8197,
        Flow = 8198,
        Step = 8199,
        BaselinedArtifactSubscribe = 8216,
        Term = 8217,
        Content = 8218,
        DDConnector = 8219,
        DDShape = 8220,
        BPShape = 8221,
        SBConnector = 8222,
        SBShape = 8223,
        UIConnector = 8224,
        UIShape = 8225,
        UCDConnector = 8226,
        UCDShape = 8227,
        PROShape = 8228
    }

    public enum BaseArtifactType
    {
        Actor,
        /// <summary>Not used by OpenAPI.</summary>
        AgilePackEpic,
        /// <summary>Not used by OpenAPI.</summary>
        AgilePackFeature,
        /// <summary>Not used by OpenAPI.</summary>
        AgilePackScenario,
        /// <summary>Not used by OpenAPI.</summary>
        AgilePackTheme,
        /// <summary>Not used by OpenAPI.</summary>
        AgilePackUserStory,
        /// <summary>Not used by OpenAPI.</summary>
        Baseline,
        /// <summary>Not used by OpenAPI.</summary>
        BaselinesAndReviews,
        /// <summary>Not used by OpenAPI.</summary>
        BaselinesAndReviewsFolder,
        BusinessProcess, //it is BusinessProcessDiagram!
        /// <summary>Not used by OpenAPI.</summary>
        Collection,
        /// <summary>Not used by OpenAPI.</summary>
        CollectionFolder,
        /// <summary>Not used by OpenAPI.</summary>
        Collections,
        Document,
        DomainDiagram,
        /// <summary>Not used by OpenAPI.</summary>
        Folder,
        GenericDiagram,
        Glossary,
        PrimitiveFolder,
        Process,
        /// <summary>Not used by OpenAPI.</summary>
        Project,
        /// <summary>Not used by OpenAPI.</summary>
        Review,
        Storyboard,
        TextualRequirement,
        UIMockup,
        UseCase,
        UseCaseDiagram
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
        /// <summary>This is a URL link to this artifact.</summary>
        Uri BlueprintUrl { get; set; }                      // OpenAPI-Get  (ex. BlueprintUrl=http://silver02.blueprintsys.net/Web/#/Storyteller/5816)
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
