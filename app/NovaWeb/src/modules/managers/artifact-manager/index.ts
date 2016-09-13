
import { ArtifactManager } from "./artifact-manager";
import { StatefulArtifactFactory } from "./artifact/artifact.factory";

angular.module("bp.managers.artifact", [])
    .service("artifactManager", ArtifactManager )
    .service("statefulArtifactFactory", StatefulArtifactFactory );

export {
    
};
