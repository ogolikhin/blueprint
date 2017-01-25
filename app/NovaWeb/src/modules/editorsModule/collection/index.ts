require("./bp-collection.scss");

import * as angular from "angular";
import {BpArtifactCollectionEditor} from "./bp-collection-editor";
import {BpCollectionHeader} from "./bp-collection-header";
import {CollectionService} from "./collection.service";

export const CollectionEditors = angular.module("bp.editors.collection", [])
    .service("collectionService", CollectionService)
    .component("bpCollection", new BpArtifactCollectionEditor())
    .component("bpCollectionHeader", new BpCollectionHeader()).name;
