require("./bp-collection.scss");

import * as angular from "angular";
import {BaselineEditor} from "./editor";
import {BaselineHeader} from "./header";
import {BaselineService} from "./baseline.service";
import {CollectionEditors} from "../collection";


export const BaselineEditors = angular.module("editorsCollection", [
        BaselineEditor,
        BaselineHeader,
        CollectionEditors
    ])
    .service("baselineService", BaselineService)
    .name;
