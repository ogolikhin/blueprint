require("./bp-collection.scss");

import * as angular from "angular";
import {CollectionEditor} from "./editor";
import {CollectionHeader} from "./header";
import {CollectionService} from "./collection.service";

export const CollectionEditors = angular.module("editorsCollection", [
        CollectionEditor,
        CollectionHeader
    ])
    .service("collectionService", CollectionService)
    .name;
