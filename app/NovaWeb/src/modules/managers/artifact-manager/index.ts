
import { ArtifactManager } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";
import { IStatefulArtifact, IStatefulSubArtifact } from "../models";
import {
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService,
} from "./attachments";


angular.module("bp.managers.artifact", [])
    .service("artifactManager", ArtifactManager )
    .service("artifactAttachments", ArtifactAttachmentsService)
    .service("statefulArtifactFactory", StatefulArtifactFactory);

export {
    IStatefulArtifact,
    IStatefulSubArtifact,
    ArtifactManager,
    StatefulArtifactFactory,
    ArtifactAttachmentsService,
    IArtifactAttachment,
    IArtifactDocRef,
    IArtifactAttachmentsResultSet,
    IArtifactAttachmentsService
};
