import {ISelectionManager, ISelection} from "../selection-manager";
import {StatefulArtifactFactory} from "./artifact/artifact.factory";
import {IStatefulItem} from "./item";
import {
    ArtifactService,
    IArtifactService,
    IStatefulArtifactFactory,
    StatefulArtifact,
    StatefulProcessArtifact,
    IStatefulArtifact
} from "./artifact";
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
import {ValidationService, IValidationService} from "./validation/validation.svc";

export {IArtifactState} from "./state/state";


angular.module("bp.managers.artifact", [])
    .service("artifactService", ArtifactService)
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("metadataService", MetaDataService)
    .service("artifactRelationships", ArtifactRelationshipsService)
    .service("statefulArtifactFactory", StatefulArtifactFactory)
    .service("validationService", ValidationService);


export {
    IStatefulItem,
    IStatefulArtifact,
    IStatefulSubArtifact,
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
    ValidationService,
    IValidationService
};
