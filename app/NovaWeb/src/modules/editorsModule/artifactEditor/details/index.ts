import "angular";
import {BpArtifactDetailsEditor} from "./artifactDetailsEditor.component";

export const DetailEditor = angular.module("detailsEditor", [])
    .component("bpArtifactDetailsEditor", new BpArtifactDetailsEditor())
    .name;
