import * as angular from "angular";
import {BpArtifactCollectionEditor} from "./bp-collection-editor";

angular.module("bp.editors.collection", []) 
    .component("bpCollection", new BpArtifactCollectionEditor());
