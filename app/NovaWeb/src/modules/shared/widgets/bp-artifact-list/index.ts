require("./bp-artifact-list.scss");

import * as angular from "angular";
import { BPArtifactListComponent, IBPArtifactListController } from "./bp-artifact-list";

angular.module("bp.widgets.artifactList", [])
    .component("bpArtifactList", new BPArtifactListComponent());

export {IBPArtifactListController};
