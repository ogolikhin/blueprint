
import { IArtifactManager, ArtifactManager, ISelectionManager,  ISelection,  SelectionSource  } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import { IStatefulArtifact, IStatefulSubArtifact, IStatefulItem } from "../models";
import { ArtifactService  } from "./artifact";
import { IDocumentRefs, DocumentRefs } from "./docrefs";
import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactAttachments,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
} from "./attachments";

import { IMetaData, MetaDataService, IMetaDataService } from "./metadata";

angular.module("bp.managers.artifact", [])
    .service("artifactService", ArtifactService)
    .service("artifactManager", ArtifactManager)
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("metadataService", MetaDataService)
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
    IDocumentRefs,
    DocumentRefs,
    IMetaData,
    MetaDataService,
    IMetaDataService
};
