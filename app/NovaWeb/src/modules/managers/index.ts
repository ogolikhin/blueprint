import "./artifact-manager";
import "./project-manager";
import "./selection-manager";

angular.module("bp.managers", [
    "bp.managers.selection",
    "bp.managers.project",
    "bp.managers.artifact"
]);

export { IArtifactManager, ISelectionManager, ISelection,  SelectionSource } from  "./artifact-manager";
export { IProjectManager } from  "./project-manager";
