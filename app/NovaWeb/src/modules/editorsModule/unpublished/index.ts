require("./unpublished.scss");

import {UnpublishedArtifactsService} from "./unpublished.service";
import {UnpublishedComponent} from "./unpublished.component";

export const UnpublishedEditor = angular.module("unpublishedEditor", [])
    .service("publishService", UnpublishedArtifactsService)
    .component("unpublished", new UnpublishedComponent())
    .name;
