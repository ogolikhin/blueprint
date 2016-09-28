import * as angular from "angular";
import { IProjectService, ProjectService } from "./project-service";
import { IProjectManager, ProjectManager} from "./project-manager";

angular.module("bp.managers.project", [])
    .service("projectService", ProjectService)
    .service("projectManager", ProjectManager);

export { Project } from "./project"
export { IProjectManager, IProjectService }  