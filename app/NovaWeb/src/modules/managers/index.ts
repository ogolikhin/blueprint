import "./artifact-manager";
<<<<<<< Updated upstream
import "./selection-manager";
//import { } from "./project-manager";

angular.module("bp.managers", [
    "bp.managers.artifact",
    "bp.managers.selection"
]);

export { IArtifactManager } from  "./models";
=======
import "./project-manager";

angular.module("bp.managers", [
    "bp.managers.artifact",
    "bp.managers.project"
]);

export { IArtifactManager } from  "./models";
export { IProjectManager } from  "./project-manager";
  
>>>>>>> Stashed changes
