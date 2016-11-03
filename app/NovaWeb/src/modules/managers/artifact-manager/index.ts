import * as angular from "angular";
import {IArtifactManager, ArtifactManager} from "./artifact-manager";
import {PublishService, IPublishService} from "./publish.svc";
import {ISelectionManager,  ISelection} from "../selection-manager";
import {StatefulArtifactFactory} from "./artifact/artifact.factory";
import {IStatefulItem} from "./item";
import {
    ArtifactService,
    IArtifactService,
    IStatefulArtifactFactory,
    StatefulArtifact,
    StatefulProcessArtifact
} from "./artifact";
import {IStatefulArtifact} from "./artifact";
import {StatefulSubArtifact, ISubArtifactCollection, IStatefulSubArtifact} from "./sub-artifact";
import {IDocumentRefs, DocumentRefs} from "./docrefs";
import {IChangeSet, IItemChangeSet, IChangeCollector, ChangeTypeEnum, ChangeSetCollector} from "./changeset";
import {
    ArtifactRelationships,
    IArtifactRelationships,
    ArtifactRelationshipsService,
    IArtifactRelationshipsService
} from "./relationships";

import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactAttachments,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService
} from "./attachments";

import {IMetaData, MetaDataService, IMetaDataService} from "./metadata";

export {IArtifactState, IState} from "./state/state";


angular.module("bp.managers.artifact", [])
    .service("artifactService", ArtifactService)
    .service("artifactManager", ArtifactManager)
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("metadataService", MetaDataService)
    .service("artifactRelationships", ArtifactRelationshipsService)
    .service("statefulArtifactFactory", StatefulArtifactFactory)
    .service("publishService", PublishService);


export {
    IStatefulItem,
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactManager,
    ArtifactManager,
    ISelectionManager,
    ISelection,
    StatefulArtifactFactory,
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactAttachments,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
    IDocumentRefs,
    DocumentRefs,
    IMetaData,
    MetaDataService,
    IMetaDataService,
    ArtifactService,
    IArtifactService,
    IStatefulArtifactFactory,
    StatefulSubArtifact,
    StatefulArtifact,
    ISubArtifactCollection,
    IChangeSet, IItemChangeSet, IChangeCollector, ChangeTypeEnum, ChangeSetCollector,
    ArtifactRelationships,
    IArtifactRelationships,
    ArtifactRelationshipsService,
    IArtifactRelationshipsService,
    StatefulProcessArtifact,
    PublishService,
    IPublishService
};
