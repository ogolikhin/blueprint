require("./unpublished.scss");

import {UnpublishedComponent} from "./unpublished";

angular.module("bp.editors.unpublished", [])
    .component("unpublished", new UnpublishedComponent());
