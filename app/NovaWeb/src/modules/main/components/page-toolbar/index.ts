import {CreateArtifactService} from "./create-artifact.svc";
import {PageToolbar} from "./page-toolbar";

angular.module("bp.components.pagetoolbar", [])
    .component("pageToolbar", new PageToolbar())
    .service("createArtifactService", CreateArtifactService);
