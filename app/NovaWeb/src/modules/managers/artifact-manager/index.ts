import * as angular from "angular";
import { IArtifactManager, ArtifactManager } from "./artifact-manager";
import { ISelectionManager,  ISelection,  SelectionSource } from "../selection-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import { IStatefulArtifact, IStatefulSubArtifact, IStatefulItem } from "../models";
import { ArtifactService, IArtifactService, IStatefulArtifactFactory, StatefulArtifact } from "./artifact";
import { StatefulSubArtifact, ISubArtifactCollection } from "./sub-artifact";
import { IDocumentRefs, DocumentRefs } from "./docrefs";
import { IChangeSet, IChangeCollector, ChangeTypeEnum, ChangeSetCollector } from "./changeset";
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
    IArtifactAttachmentsService,
} from "./attachments";

import { IMetaData, MetaDataService, IMetaDataService } from "./metadata";

export { IArtifactState, IState } from "./state/state";


angular.module("bp.managers.artifact", [])
    .service("artifactService", ArtifactService)
    .service("artifactManager", ArtifactManager)
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("metadataService", MetaDataService)
    .service("artifactRelationships", ArtifactRelationshipsService)
    .service("statefulArtifactFactory", StatefulArtifactFactory);


export {
    IStatefulItem,
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactManager,
    ArtifactManager,
    ISelectionManager,  
    ISelection,  
    SelectionSource,    
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
    IChangeSet, IChangeCollector, ChangeTypeEnum, ChangeSetCollector,
    ArtifactRelationships, 
    IArtifactRelationships, 
    ArtifactRelationshipsService, 
    IArtifactRelationshipsService
};
