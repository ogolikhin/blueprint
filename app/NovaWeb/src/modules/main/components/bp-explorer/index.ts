import * as angular from "angular";
import {ProjectExplorer} from "./bp-explorer";
import {ProjectExplorerService} from "./project-explorer.service";

angular.module("bp.components.explorer", ["commonModule"])
    .service("projectExplorerService", ProjectExplorerService)
    .component("bpProjectExplorer", new ProjectExplorer());
