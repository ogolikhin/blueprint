import * as angular from "angular";
import { ProjectExplorer } from "./bp-explorer";

angular.module("bp.components.explorer", [])
    .component("bpProjectExplorer", new ProjectExplorer());
