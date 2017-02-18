import * as angular from "angular";
import {IProjectService, ProjectService} from "./project-service";

angular.module("bp.managers.project", [])
    .service("projectService", ProjectService);

export {IProjectService}
