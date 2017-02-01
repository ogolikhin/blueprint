import * as angular from "angular";

import {BpArtifactCollectionEditor} from "./collectionEditor.component";

export const CollectionEditor = angular.module("collectionEditor", [])
    .component("bpCollection", new BpArtifactCollectionEditor()).name;
