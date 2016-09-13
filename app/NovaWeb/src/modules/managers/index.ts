import "./artifact-manager";
import "./project-manager";
import "./selection-manager";

angular.module("bp.managers", [
    "bp.managers.artifact",
    "bp.managers.selection",
    "bp.managers.project"
]);

export { IArtifactManager } from  "./models";
export { IProjectManager } from  "./project-manager";
