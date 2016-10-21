import * as angular from "angular";
import {BpArtifactCollectionEditor} from "./bp-collection-editor";
import {CollectionService} from "./collection.svc";

angular.module("bp.editors.collection", [])
    .service("collectionService", CollectionService)
    .component("bpCollection", new BpArtifactCollectionEditor());    
