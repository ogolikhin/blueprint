
import { ArtifactManager } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService 
} from "./attachments";

angular.module("bp.managers.artifact", [])
    .service("artifactManager", ArtifactManager )
    .service("statefulArtifactFactory", StatefulArtifactFactory );

export {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService
};
