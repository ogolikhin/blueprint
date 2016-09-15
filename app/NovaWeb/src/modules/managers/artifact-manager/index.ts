
import { IArtifactManager, ArtifactManager, ISelectionManager,  ISelection,  SelectionSource } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import { IStatefulArtifact, IStatefulSubArtifact } from "../models";
import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
} from "./attachments";

import { IMetaData, MetaDataService, IMetaDataService } from "./metadata";

angular.module("bp.managers.artifact", [])
    .service("artifactManager", ArtifactManager )
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("metaDataService", MetaDataService)
    .service("statefulArtifactFactory", StatefulArtifactFactory);

export {
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactManager,
    ISelectionManager,  
    ISelection,  
    SelectionSource,    
    StatefulArtifactFactory,
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
    IMetaData,
    MetaDataService,
    IMetaDataService
};
