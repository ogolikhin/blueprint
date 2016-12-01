require("./unpublished.scss");

import {UnpublishedArtifactsService} from "./unpublished.svc";
import {UnpublishedComponent} from "./unpublished";

angular.module("bp.editors.unpublished", [])
    .service("publishService", UnpublishedArtifactsService)
    .component("unpublished", new UnpublishedComponent());
