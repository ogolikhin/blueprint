import { IArtifactManager, ArtifactManager, ISelectionManager,  ISelection,  SelectionSource } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import { IStatefulArtifact, IStatefulSubArtifact, IStatefulItem } from "../models";
import { IDocumentRefs, DocumentRefs } from "./docrefs";
import { IArtifactHistoryService, ArtifactHistoryService, IArtifactHistoryVersion } from "./history";
import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactAttachments,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
} from "./attachments";

import { IMetaData, MetaDataService, IMetaDataService } from "./metadata";

angular.module("bp.managers.artifact", ["bp.managers.selection"])
    .service("artifactManager", ArtifactManager )
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("artifactHistory", ArtifactHistoryService)
    .service("metaDataService", MetaDataService)
    .service("statefulArtifactFactory", StatefulArtifactFactory);


export {
    IStatefulItem,
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactManager,
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
    ArtifactHistoryService,
    IArtifactHistoryService,
    IArtifactHistoryVersion,
    IDocumentRefs,
    DocumentRefs,
    IMetaData,
    MetaDataService,
    IMetaDataService
};
