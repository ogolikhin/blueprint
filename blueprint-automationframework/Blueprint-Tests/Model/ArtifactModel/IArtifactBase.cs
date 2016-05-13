﻿using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model.ArtifactModel
{
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
        AgilePackEpic,
        AgilePackFeature,
        AgilePackScenario,
        AgilePackTheme,
        AgilePackUserStory,
        Baseline,
        BaselinesAndReviews,
        BaselinesAndReviewsFolder,
        BusinessProcess, //it is BusinessProcessDiagram!
        Collection,
        CollectionFolder,
        Collections,
        Document,
        DomainDiagram,
        Folder,
        GenericDiagram,
        Glossary,
        PrimitiveFolder,
        Process,
        Project,
        Review,
        Storyboard,
        TextualRequirement,
        UIMockup,
        UseCase,
        UseCaseDiagram
    }

    public interface IArtifactBase
    {
        BaseArtifactType BaseArtifactType { get; set; }
        ItemTypePredefined BaseItemTypePredefined { get; set; }
        int Id { get; set; }
        string Name { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }
        int ParentId { get; set; }
        Uri BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
        string ArtifactTypeName { get; set; }
        bool AreTracesReadOnly { get; set; }
        bool AreAttachmentsReadOnly { get; set; }
        bool AreDocumentReferencesReadOnly { get; set; }
        string Address { get; set; }
        IUser CreatedBy { get; set; }
        bool IsPublished { get; set; }
        bool IsSaved { get; set; }

        /// <summary>
        /// Get ArtifactReference list which is used to represent breadcrumb navigation
        /// </summary>
        /// <param name="user">The user credentials for breadcrumb navigation</param>
        /// <param name="artifacts">The list of artifacts used for breadcrumb navigation</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The List of ArtifactReferences after the get navigation call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<ArtifactReference> GetNavigation(IUser user, List<IArtifact> artifacts,
            List<HttpStatusCode> expectedStatusCodes = null);
    }
}
