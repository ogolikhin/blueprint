import * as angular from "angular";
import "./artifact-manager";
import "./project-manager";
import "./selection-manager";

angular.module("bp.managers", [
    "bp.managers.selection",
    "bp.managers.project",
    "bp.managers.artifact"
]);

export {IArtifactManager, ISelectionManager, ISelection} from  "./artifact-manager";
export {IProjectManager} from  "./project-manager";
