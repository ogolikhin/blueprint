import { ProjectManager } from "./project-manager";

angular.module("bp.managers.project", [])
     .service("projectManager", ProjectManager);

export { Project } from "./project"
export { IProjectManager } from "./project-manager"  