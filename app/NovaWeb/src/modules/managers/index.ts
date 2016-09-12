import "./artifact-manager";
import "./selection-manager";
//import { } from "./project-manager";

angular.module("bp.managers", [
    "bp.managers.artifact",
    "bp.managers.selection"
]);

export { IArtifactManager } from  "./models";
