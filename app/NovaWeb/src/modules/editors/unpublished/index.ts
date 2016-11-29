require("./unpublished.scss");

import * as angular from "angular";
import {UnpublishedComponent} from "./unpublished";

angular.module("bp.editors.unpublished", [])
    .component("unpublished", new UnpublishedComponent());
