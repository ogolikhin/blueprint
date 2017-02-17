require("./bp-collection.scss");

import * as angular from "angular";
import {BaselineEditor} from "./editor";
import {BaselineHeader} from "./header";
import {BaselineService} from "./baseline.service";

export const BaselineEditors = angular.module("editorsBaseline", [
        BaselineEditor,
        BaselineHeader
    ])
    .service("baselineService", BaselineService)
    .name;
